using System;

namespace Hikaria.QC.Actions
{
    /// <summary>
    /// Gets the next line of text entered into the console as a user 
    /// response instead of invoking it as a command.
    /// </summary>
    public class ReadLine : ICommandAction
    {
        private readonly Action<string> _getInput;
        private readonly ResponseConfig _config;
        private QuantumConsole _console;
        private string _response;
        private Action<string> _responseCallback;

        public bool IsFinished => _response != null;

        public bool StartsIdle => true;

        /// <param name="getInput">A delegate which returns the input entered by the user.</param>
        /// <param name="config">The config to provide the response flow with.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadLine(Action<string> getInput, ResponseConfig config)
        {
            // validate
            if (getInput == null)
            {
                throw new ArgumentNullException(nameof(getInput));
            }

            // set fields
            _getInput = getInput;
            _config = config;
        }

        /// <param name="getInput">A delegate which returns the input entered by the user.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReadLine(Action<string> getInput) : this(getInput, ResponseConfig.Default)
        {

        }

        public void Complete(ActionContext context)
        {
            if (_response != null)
                _getInput(_response); // push value to the caller
        }

        public void Start(ActionContext context)
        {
            _response = null; // reset flag
            _console = context.Console;
            _responseCallback = OnResponseSubmittedHandler;
            _console.BeginResponse(_responseCallback, _config);
        }

        public void Cancel(ActionContext context)
        {
            if (_console != null && _responseCallback != null)
                _console.CancelResponse(_responseCallback);

            _console = null;
            _responseCallback = null;
        }

        private void OnResponseSubmittedHandler(string response)
        {
            _response = response; // changes IsFinished flag
        }
    }
}
