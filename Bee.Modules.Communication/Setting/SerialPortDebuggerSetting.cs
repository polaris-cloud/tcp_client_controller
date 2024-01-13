using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;
using Bee.Modules.Communication.Shared;

namespace Bee.Modules.Communication.Setting
{
internal    class SerialPortDebuggerSetting:IAppData
    {
        public string ComName { get; set; }
        public int BaudRate { get; set; }
        public StopBits StopBit { get; set; }
        public int DataBit { get; set; }
        public Parity Parity { get; set; }
        public bool IsOutAsLog { get; set; }
        public FrameFormat SendFrameFormat { get; set; }
        public FrameFormat ReceiveFrameFormat { get; set; }
        public bool IsAddNewLineWhenSend { get; set; }
        public bool IsSendAtRegularTime { get; set; }
        public int SendCycleTime { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public string SubDir { get; } = "cms";
        public string Contract { get; } = "spd";
    }
}
