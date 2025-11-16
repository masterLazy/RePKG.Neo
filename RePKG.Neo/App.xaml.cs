/**
   Copyright 2025 masterLazy

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
 */
using System.Windows;

namespace RePKG.Neo {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application {
        public static string droppedFile = "";

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            string[] args = e.Args;
            if (args.Length > 0) {
                droppedFile = args[0];
            }
        }
    }

}
