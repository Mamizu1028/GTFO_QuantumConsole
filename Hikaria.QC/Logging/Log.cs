namespace Hikaria.QC
{
    public readonly struct Log : ILog
    {
        public string Text { get; }
        public LogLevel Level { get; }
        public bool NewLine { get; }

        public Log(string text, LogLevel level = LogLevel.Message, bool newLine = true)
        {
            Text = text;
            Level = level;
            NewLine = newLine;
        }
    }
}
