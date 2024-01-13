using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bee.Core.DataSource;

namespace Bee.Modules.Communication.Setting
{
    /// <summary>
    /// dto ，do
    /// </summary>
    internal class CommunicationModuleSetting: IAppData
    {

        public bool IsDebugMode { get; set; }


        public CommunicationModuleSetting()
        {
            //SubDir = Guid.NewGuid().ToString();
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public string SubDir { get; } = /*Guid.NewGuid().ToString("N")*/"cms";
        public string Contract { get; } = "cm";
    }
}
