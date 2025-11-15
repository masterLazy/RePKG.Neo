using RePKG.Command;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.ComponentModel;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isInputEnabled = true;
        public bool IsInputEnabled {
            get { return _isInputEnabled; }
            set {
                if (value == _isInputEnabled) return;
                _isInputEnabled = value;
                OnPropertyChanged(nameof(IsInputEnabled));
            }
        }

        public MainWindow() {
            InitializeComponent();

            if (!string.IsNullOrEmpty(App.droppedFile)) {
                TbInput.Text = App.droppedFile;
                MakeOutputDir();
            }
        }

        public void MakeOutputDir() {
            int i = TbInput.Text.LastIndexOf('.');
            TbOutput.Text = string.Concat(TbInput.Text.AsSpan(0, i), "-repkg\\");
        }

        // Open input file
        private void BtnBrowseIn_Click(object sender, RoutedEventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "scene";
            dialog.DefaultExt = ".pkg";
            dialog.Filter = "Wallpaper Engine Pakage|*.pkg" +
                "|All Files|*.*"; ;
            bool? result = dialog.ShowDialog();
            if (result == true) {
                TbInput.Text = dialog.FileName;
                MakeOutputDir();
            }
        }

        private void BtnBrowseOut_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFolderDialog dialog = new();
            dialog.Multiselect = false;
            dialog.Title = "Select an Output Folder";
            bool? result = dialog.ShowDialog();
            if (result == true) {
                TbOutput.Text = dialog.FolderName;
            }
        }

        private void BtnExtract_Click(object sender, RoutedEventArgs e) {
            IsInputEnabled = false;
            if (Path.Exists(TbOutput.Text)) {
                if (MessageBox.Show($"Folder \"{TbOutput.Text}\" has existed. Do you want to overwrite it?",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) {
                    IsInputEnabled = true;
                    MessageBox.Show($"Extraction canceled.",
                    "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            // Do extract
            ExtractOptions extractOptions = new() {
                Input = TbInput.Text,
                OutputDirectory = TbOutput.Text,
                Overwrite = true,
            };
            var result = Extract.Action(extractOptions);
            // Info
            if (result) {
                MessageBox.Show($"The package has been extracted to \"{TbOutput.Text}\"",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                MessageBox.Show($"File \"{TbInput.Text}\" not found. Please check the input filename.",
                    "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsInputEnabled = true;
        }
    }
}