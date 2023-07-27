using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ServiceTool
{
    public class BaseCommand : ICommand
    {
        public BaseCommand(Action<object> executeAction)
        {
            ExecuteAction = executeAction;
        }

        public Action<object> ExecuteAction { get; set; }


        public Func<object, bool> CanExecuteAction { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (CanExecuteAction != null)
                return CanExecuteAction(parameter);
            return true;
        }

        /// <summary>
        /// 做什么（必须要实现）
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            ExecuteAction?.Invoke(parameter);
        }
    }
}
