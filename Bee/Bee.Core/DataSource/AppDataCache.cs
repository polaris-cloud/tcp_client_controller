using System;
using System.Collections.Generic;
using System.Text;

namespace Bee.Core.DataSource
{
   
    public class AppDataCache
    {
        private readonly Dictionary<Type, IAppData> _mappings = new Dictionary<Type, IAppData>();



        public void Register(Type contract, IAppData appData)
        {
            if (contract==null)
                throw new ArgumentNullException(nameof(contract));
            if (appData == null)
                throw new ArgumentNullException(nameof(appData));
            if (_mappings.ContainsKey(contract))
                throw new InvalidOperationException(nameof(appData));
            _mappings.Add(contract, appData);
        }

        public void ReplaceOld(Type contract, IAppData appData)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            //if (RegisteredContract(contract))
            _mappings[contract]=appData ?? throw new ArgumentNullException(nameof(appData));
            //else throw new ArgumentException("Contract is not registered", nameof(contract));
        }



        public IAppData GetMapping(Type contract)
        {

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            return _mappings[contract];
        }

        public T GetMapping<T>() where T : IAppData
        {
            Type type=typeof(T);
            return (T)GetMapping(type); 
        }

        public IEnumerable<Type> GetAppTypes => _mappings.Keys;

        public bool RegisteredContract(Type contract) => _mappings.ContainsKey(contract);


    }
}
