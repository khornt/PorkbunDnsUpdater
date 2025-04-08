using System.Windows.Input;

namespace PorkbunDnsUpdater.Commands
{
    public abstract class AsyncCommandBase : ICommand
    {
        private bool _isExecuting;              

        public bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                _isExecuting = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual bool CanExecute(object? parameter)
        {
            return !IsExecuting;
        }
         
        public async void Execute(object? parameter)
        {
            IsExecuting = true;            
            await ExecuteAsync(parameter);            
            IsExecuting = false;
        }

        protected abstract Task ExecuteAsync(object? parameter);

        public event EventHandler? CanExecuteChanged;
    }
}
