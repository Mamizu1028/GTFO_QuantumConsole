namespace Hikaria.QC.Actions
{
    /// <summary>
    /// Serializes and logs a value to the console.
    /// </summary>
    public class Value : ICommandAction
    {
        private readonly object _value;

        public bool IsFinished => true;
        public bool StartsIdle => false;

        /// <param name="value">The value to log to the console.</param>
        public Value(object value)
        {
            _value = value;
        }

        public void Start(ActionContext context) { }

        public void Cancel(ActionContext context) { }

        public void Complete(ActionContext context)
        {
            QuantumConsole console = context.Console;
            string serialized = _value as string ?? console.Serialize(_value);
            console.WriteLog(serialized);
        }
    }
}
