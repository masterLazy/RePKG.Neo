using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application {
        public static string droppedFile = "";

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            string[] args = e.Args;
            if (args.Length > 0 && File.Exists(args[0])) {
                droppedFile = args[0];
            }
        }
    }

}
