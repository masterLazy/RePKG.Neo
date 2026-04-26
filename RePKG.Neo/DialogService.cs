using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace RePKG.Neo {
    /// <summary>
    /// Service for showing dialogs in Avalonia application
    /// </summary>
    public static class DialogService {
        private static Window? _mainWindow;

        /// <summary>
        /// Initialize the dialog service with the main window
        /// </summary>
        public static void Initialize(Window mainWindow) {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Show an info dialog
        /// </summary>
        public static async Task ShowInfoAsync(string title, string content) {
            if (_mainWindow == null) return;
            await ShowDialogWindow(_mainWindow, title, content, isError: false);
        }

        /// <summary>
        /// Show an error dialog
        /// </summary>
        public static async Task ShowErrorAsync(string title, string content) {
            if (_mainWindow == null) return;
            await ShowDialogWindow(_mainWindow, title, content, isError: true);
        }

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        public static async Task<bool> ShowConfirmationAsync(string title, string content) {
            if (_mainWindow == null) return false;
            return await ShowConfirmationWindow(_mainWindow, title, content);
        }

        private static async Task ShowDialogWindow(Window owner, string title, string content, bool isError) {
            var dialog = new Window {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false
            };

            var stackPanel = new StackPanel {
                Margin = new Thickness(20),
                Spacing = 10
            };

            var textBlock = new TextBlock {
                Text = content,
                TextWrapping = TextWrapping.Wrap,
                Foreground = isError ? new SolidColorBrush(Colors.Red) : null
            };

            var button = new Button {
                Content = "OK",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            button.Click += (s, e) => dialog.Close();

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);

            dialog.Content = stackPanel;

            await dialog.ShowDialog(owner);
        }

        private static async Task<bool> ShowConfirmationWindow(Window owner, string title, string content) {
            bool result = false;

            var dialog = new Window {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false
            };

            var stackPanel = new StackPanel {
                Margin = new Thickness(20),
                Spacing = 10
            };

            var textBlock = new TextBlock {
                Text = content,
                TextWrapping = TextWrapping.Wrap
            };

            var buttonPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var yesButton = new Button {
                Content = "Yes",
                Width = 80
            };

            var noButton = new Button {
                Content = "No",
                Width = 80
            };

            yesButton.Click += (s, e) => {
                result = true;
                dialog.Close();
            };

            noButton.Click += (s, e) => {
                result = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;

            await dialog.ShowDialog(owner);

            return result;
        }
    }
}
