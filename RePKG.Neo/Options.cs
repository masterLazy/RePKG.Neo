using Microsoft.VisualBasic;
using System.IO;
using System.Text.Json;

namespace RePKG.Neo {
    public class Options {
        public bool NoTexConvert { get; set; } = false;
        public bool CopyProject { get; set; } = false;
        public bool AutoExtract {  get; set; } = false;

        public static readonly string OptionsFile = AppDomain.CurrentDomain.BaseDirectory + "options.json";

        public bool Save() {
            try {
                using StreamWriter sw = new(OptionsFile, false);
                var json = JsonSerializer.Serialize(this, App.JsonOptions);
                sw.Write(json);
            }
            catch {
                return false;
            }
            return true;
        }

        static public Options? Load() {
            try {
                using StreamReader sr = new(OptionsFile);
                var json = sr.ReadToEnd();
                var settings = JsonSerializer.Deserialize<Options>(json, App.JsonOptions);
                return settings;
            }
            catch {
                return null;
            }
        }
    }
}
