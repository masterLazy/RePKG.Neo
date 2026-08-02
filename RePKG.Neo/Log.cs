/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using System.IO;
using System.Reflection;
using System.Text;

namespace RePKG.Neo {
    /// <summary>
    /// Simple logger. Writes to %AppData%\RePKG.Neo\latest.log (overwritten on each launch).
    /// Console output from the embedded RePKG CLI is redirected here via Console.SetOut/SetError
    /// and logged with a "(CLI)" prefix. Use Log.Info/Warn/Error from the app for structured entries.
    /// </summary>
    public class Log : Core.ILogger {
        private static readonly object Sync = new();
        private static StreamWriter? _file;

        public static string? LogPath { get; private set; }

        /// <summary>
        /// Opens latest.log (overwriting any previous one) and redirects Console output to it.
        /// Safe to call only once.
        /// </summary>
        public static void Init(string directory) {
            lock (Sync) {
                if (_file != null) return;
                Directory.CreateDirectory(directory);
                LogPath = Path.Combine(directory, "latest.log");
                _file = new StreamWriter(LogPath, append: false, Encoding.UTF8) { AutoFlush = true };
            }

            try {
                Console.SetOut(new CliTeeWriter(Console.Out));
                Console.SetError(new CliTeeWriter(Console.Error));
            }
            catch {
                // Logging setup must never break the app
            }
        }

        /// <summary>
        /// Logs the assembly version (with short commit hash) and build time once at startup.
        /// </summary>
        public static void StartupInfo() {
            var buildTime = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            var commit = CommitHash == null ? "" : $" (commit {CommitHash})";

            Info($"RePKG.Neo version {VersionText}{commit}");
            Info($"Build time {buildTime:yyyy-MM-dd HH:mm:ss}");
        }

        /// <summary>
        /// Display version derived from the assembly, without the "+&lt;commit&gt;" suffix.
        /// </summary>
        public static string VersionText { get; } = ComputeVersionText();

        /// <summary>
        /// Short git commit hash taken from the informational version, or null if absent.
        /// </summary>
        public static string? CommitHash { get; } = ComputeCommitHash();

        private static string ComputeVersionText() {
            var informational = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // InformationalVersion may look like "2.1.0+<commit>"; strip the "+..." part for display.
            if (!string.IsNullOrEmpty(informational)) {
                var plus = informational.IndexOf('+');
                return plus >= 0 ? informational[..plus] : informational;
            }

            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        }

        private static string? ComputeCommitHash() {
            var informational = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrEmpty(informational))
                return null;

            var plus = informational.IndexOf('+');
            if (plus < 0 || plus == informational.Length - 1)
                return null;

            var hash = informational[(plus + 1)..];
            return hash.Length > 7 ? hash[..7] : hash;
        }

        public static void Debug(string msg) => Write("debug", msg);
        public static void Info(string msg) => Write("info", msg);
        public static void Warn(string msg) => Write("warn", msg);
        public static void Error(string msg) => Write("error", msg);

        internal static void WriteCli(string line) {
            // Strip leading newlines; skip calls that are entirely empty or "\r\n"-only.
            line = line.TrimStart('\r', '\n', ' ', '*', '#');
            if (line.Length == 0) return;
            if (line.StartsWith("Reading:") || line.StartsWith("Extracting:")) {
                Debug(line);
            } else {
                Info(line);
            }
        }

        private static void Write(string level, string msg) {
            WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {msg}");
        }

        private static void WriteLine(string line) {
            lock (Sync) {
                _file?.WriteLine(line);
            }
        }
    }

    /// <summary>
    /// TextWriter that tees Console output: mirrors each completed line to the original console
    /// and writes it to the log with a "(CLI)" prefix. A single WriteLine call is treated as one
    /// logical log line even when its content contains newlines (e.g. exception stack traces),
    /// so the "(CLI)" prefix is added only once per logical line.
    /// </summary>
    internal sealed class CliTeeWriter : TextWriter {
        private readonly TextWriter _console;
        private readonly StringBuilder _buffer = new();
        private readonly object _sync = new();

        public override Encoding Encoding => Encoding.UTF8;

        public CliTeeWriter(TextWriter console) {
            _console = console;
        }

        public override void Write(char value) {
            lock (_sync) {
                _buffer.Append(value);
            }
        }

        public override void Write(string? value) {
            if (value == null) return;
            lock (_sync) {
                _buffer.Append(value);
            }
        }

        // Console.WriteLine(string) routes here. Note: TextWriter.WriteLine(string) does
        // NOT call the parameterless WriteLine(), so flush the whole call as one logical
        // line here (embedded newlines are preserved inside the body).
        public override void WriteLine(string? value) {
            if (value != null) {
                lock (_sync) {
                    _buffer.Append(value);
                }
            }
            FlushLine();
        }

        // A WriteLine call terminates one logical line; flush the whole buffer at once.
        public override void WriteLine() => FlushLine();

        private void FlushLine() {
            string line;
            lock (_sync) {
                line = _buffer.ToString();
                _buffer.Clear();
            }
            Log.WriteCli(line);
            _console.WriteLine(line);
        }
    }
}
