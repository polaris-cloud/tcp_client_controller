using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polaris.Connect.Tool.Base;

namespace Polaris.Connect.Tool
{
    public class ComManager
    { 
        Dictionary<string ,ICom> _comMappings;

        public EventHandler<ComMappingChangedEventArg>? ComMappingChanged; 
public IEnumerable<string> ActiveComs=> _comMappings.Keys;
        public ComManager()
        {
            _comMappings= new Dictionary<string ,ICom>();
        }

        public void Register(string contract ,ICom com)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (com == null)
                throw new ArgumentNullException(nameof(com));
            if (_comMappings.ContainsKey(contract))
                throw new InvalidOperationException($"already contains {nameof(com)}: {contract}");
            _comMappings.Add(contract, com);
        }

        public ICom GetComMapping(string? contract)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (!_comMappings.ContainsKey(contract))
                throw new InvalidOperationException($"not contains {nameof(contract) }:{contract}");
            return _comMappings[contract];
        }


        public void UnRegister(string contract)
        {
            if (string.IsNullOrEmpty(contract))
                throw new ArgumentNullException(nameof(contract));
            if (!_comMappings.ContainsKey(contract))
                throw new InvalidOperationException($"not contains {nameof(contract)}:{contract}");
             _comMappings.Remove(contract);
        }
    }
}
