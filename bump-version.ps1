# bump-version.ps1
[CmdletBinding()]
param(
    [string]$ProjectRoot = $PSScriptRoot,   # repo root
    [string]$NewVersion,                    # optional, otherwise prompted
    [switch]$Yes,                           # skip confirmation
    [switch]$NoPause                        # skip the final "Press Enter"
)

$ErrorActionPreference = 'Stop'

# ---------- Encoding helpers: preserve BOM on read/write ----------
function Read-TextFile {
    param([string]$Path)
    $bytes  = [IO.File]::ReadAllBytes($Path)
    $hasBom = $bytes.Length -ge 3 -and
            $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $offset = 0
    if ($hasBom) { $offset = 3 }
    $text = [Text.Encoding]::UTF8.GetString($bytes, $offset, $bytes.Length - $offset)
    [pscustomobject]@{ Text = $text; HasBom = $hasBom }
}

function Write-TextFile {
    param([string]$Path, [string]$Text, [bool]$HasBom)
    $enc = New-Object System.Text.UTF8Encoding($HasBom)
    [IO.File]::WriteAllText($Path, $Text, $enc)
}

# ---------- Suggest next version ----------
# Handles SemVer: major.minor.patch[-prerelease][+build]
#   - with prerelease: bump the trailing numeric segment (alpha -> alpha.1,
#     alpha.1 -> alpha.2, rc.1 -> rc.2, beta.12 -> beta.13)
#   - without:         bump the patch segment (2.2.1 -> 2.2.2)
function Get-SuggestedVersion {
    param([string]$Version)
    if ($Version -match '^(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.\-]+))?(\+[0-9A-Za-z.\-]+)?$') {
        $major = [int]$Matches[1]
        $minor = [int]$Matches[2]
        $patch = [int]$Matches[3]
        $pre   = $Matches[4]     # without leading '-', may be empty
        $build = $Matches[5]     # includes leading '+', may be empty

        if ($pre) {
            if ($pre -match '^(.*?)(\d+)$') {
                $preNew = $Matches[1] + ([int]$Matches[2] + 1)
            } else {
                $preNew = "$pre.1"
            }
            return "$major.$minor.$patch-$preNew$build"
        }
        return "$major.$minor.$($patch + 1)$build"
    }
    return $null
}

# ---------- Target files ----------
$targets = @(
    [pscustomobject]@{
        Name     = 'RePKG.Neo.csproj'
        Path     = Join-Path $ProjectRoot 'RePKG.Neo\RePKG.Neo.csproj'
        Pattern  = '<Version>([^<]+)</Version>'
        Template = '<Version>{0}</Version>'
    }
    [pscustomobject]@{
        Name     = 'setup-x64.iss'
        Path     = Join-Path $ProjectRoot '.innoSetup/setup-x64.iss'
        Pattern  = '#define\s+MyAppVersion\s+"([^"]+)"'
        Template = '#define MyAppVersion "{0}"'
    }
    [pscustomobject]@{
        Name     = 'setup-x86.iss'
        Path     = Join-Path $ProjectRoot '.innoSetup/setup-x86.iss'
        Pattern  = '#define\s+MyAppVersion\s+"([^"]+)"'
        Template = '#define MyAppVersion "{0}"'
    }
)

try {
    # ---------- Scan current versions ----------
    $entries = @()
    foreach ($t in $targets) {
        if (-not (Test-Path -LiteralPath $t.Path)) {
            Write-Warning "File not found: $($t.Path)"
            continue
        }
        $raw = Read-TextFile $t.Path
        $m   = [regex]::Match($raw.Text, $t.Pattern)
        if (-not $m.Success) {
            Write-Warning "Version not found in $($t.Name)"
            continue
        }
        $entries += [pscustomobject]@{
            Target  = $t
            Version = $m.Groups[1].Value
            Raw     = $raw
            Match   = $m
        }
    }

    if ($entries.Count -eq 0) {
        Write-Error 'No version strings found, aborting.'
        return
    }

    # ---------- Display ----------
    $nameWidth = ($entries | ForEach-Object { $_.Target.Name.Length } | Measure-Object -Maximum).Maximum
    if (-not $nameWidth) { $nameWidth = 20 }

    Write-Host ''
    Write-Host 'Current versions:' -ForegroundColor Cyan
    foreach ($e in $entries) {
        Write-Host (('  {0,-' + $nameWidth + '}  {1}') -f $e.Target.Name, $e.Version)
    }

    $distinct = @($entries | ForEach-Object { $_.Version } | Sort-Object -Unique)
    if ($distinct.Count -gt 1) {
        Write-Host ''
        Write-Host "Warning: version mismatch: $($distinct -join ' / ')" -ForegroundColor Yellow
    }

    # Suggest next version only when all files agree
    $suggested = $null
    if ($distinct.Count -eq 1) {
        $suggested = Get-SuggestedVersion -Version $distinct[0]
    }

    # ---------- Prompt for new version ----------
    if (-not $NewVersion) {
        $prompt = 'Enter new version'
        if ($suggested) { $prompt += " (press Enter for $suggested)" }
        $NewVersion = Read-Host "$prompt"
        if ([string]::IsNullOrWhiteSpace($NewVersion) -and $suggested) {
            $NewVersion = $suggested
        }
    }

    if ([string]::IsNullOrWhiteSpace($NewVersion)) {
        Write-Host 'No version entered, aborted.' -ForegroundColor DarkGray
        return
    }

    # SemVer-ish check: 2 to 4 numeric segments, optional -prerelease, optional +build
    if ($NewVersion -notmatch '^\d+(\.\d+){1,3}(-[0-9A-Za-z.\-]+)?(\+[0-9A-Za-z.\-]+)?$') {
        Write-Warning "Version format looks non-standard: $NewVersion"
    }

    # ---------- Preview and confirm ----------
    Write-Host ''
    Write-Host 'Pending changes:' -ForegroundColor Cyan
    foreach ($e in $entries) {
        Write-Host (('  {0,-' + $nameWidth + '}  {1}  ->  {2}') -f $e.Target.Name, $e.Version, $NewVersion)
    }

    if (-not $Yes) {
        $ans = Read-Host 'Proceed? (y/N)'
        if ($ans -notmatch '^(y|yes)$') {
            Write-Host 'Cancelled.' -ForegroundColor DarkGray
            return
        }
    }

    # ---------- Apply ----------
    foreach ($e in $entries) {
        $new     = $e.Target.Template -f $NewVersion
        $m       = $e.Match
        $updated = $e.Raw.Text.Substring(0, $m.Index) +
                $new +
                $e.Raw.Text.Substring($m.Index + $m.Length)

        Write-TextFile -Path $e.Target.Path -Text $updated -HasBom $e.Raw.HasBom
        Write-Host "Updated $($e.Target.Name)" -ForegroundColor Green
    }

    Write-Host ''
    Write-Host "Done. New version: $NewVersion" -ForegroundColor Green
}
finally {
    # Keep the window open when launched interactively (double-click, right-click
    # "Run with PowerShell", etc.), but stay silent in CI / piped usage.
    if (-not $NoPause -and -not [Console]::IsInputRedirected) {
        Write-Host ''
        Read-Host 'Press Enter to exit' | Out-Null
    }
}