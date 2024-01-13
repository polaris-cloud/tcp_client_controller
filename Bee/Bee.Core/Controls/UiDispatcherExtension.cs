using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Bee.Core.Controls
{
    public  static class UiDispatcherExtension
    {
        /// <summary>
        /// Runs the action on UI dispatcher.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="action">The action to run</param>
        public static void RunOnUiDispatcher(this Dispatcher dispatcher,Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action, null);
            }
        }
    }
}
