using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
namespace Bee.Core.Controls
{

    public enum LogLevel
    {
        Info,
        Debug,
        Warn,
        Error,
        Fatal, 
        Other,
    }

    
    public delegate void LogHandler(string output,LogLevel level);
    
    // Todo:后面是否使用组件的方式插入ViewModel
    public interface IEasyLoggingBindView
    {
        event LogHandler OnLogData;
    }
}
