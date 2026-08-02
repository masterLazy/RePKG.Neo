namespace RePKG.Core {
    public interface ILogger {
        public static abstract void Debug(string msg);
        public static abstract void Info(string msg);
        public static abstract void Warn(string msg);
        public static abstract void Error(string msg);
    }
}
