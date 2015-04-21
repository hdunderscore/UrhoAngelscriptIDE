using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

namespace XMLEditor.Common
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;
        //private Action<int> action;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<T> action)
            : this(action, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        //public RelayCommand(Action<int> action)
        //{
        //    // TODO: Complete member initialization
        //    this.action = action;

        //}

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            //return _canExecute == null ? true : _canExecute((T)parameter);

            if (_canExecute == null)
            {
                return true;
            }
            if (_canExecute != null)
            {
                if (parameter is T)
                {
                    return _canExecute((T)parameter);
                }
                //For triggerng even user passes null parameter.
                else if (parameter == null)
                {
                    return _canExecute(default(T));
                }
            }

            return true;

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {

            _execute((T)parameter);
        }

        #endregion // ICommand Members
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action<object> action)
            : base(action)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            : base(execute, canExecute)
        { }
    }
}
