using System;

namespace Hikaria.QC.Actions
{
    /// <summary>
    /// Custom action implemented via delegates.
    /// For more complex actions it is usually recommended to create a new action implementing <c>ICommandAction</c>.
    /// </summary>
    public class Custom : ICommandAction
    {
        private readonly Func<bool> _isFinished;
        private readonly Func<bool> _startsIdle;
        private readonly Action<ActionContext> _start;
        private readonly Action<ActionContext> _complete;
        private readonly Action<ActionContext> _cancel;

        public Custom(
            Func<bool> isFinished,
            Func<bool> startsIdle,
            Action<ActionContext> start,
            Action<ActionContext> complete,
            Action<ActionContext>? cancel = null
        )
        {
            _isFinished = isFinished;
            _startsIdle = startsIdle;
            _start = start;
            _complete = complete;
            _cancel = cancel ?? (_ => { });
        }

        public bool IsFinished => _isFinished();
        public bool StartsIdle => _startsIdle();

        public void Start(ActionContext context) { _start(context); }
        public void Complete(ActionContext context) { _complete(context); }
        public void Cancel(ActionContext context) { _cancel(context); }
    }

}
