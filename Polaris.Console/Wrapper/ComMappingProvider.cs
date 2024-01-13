using System.Windows.Markup;

namespace Polaris.Console.Wrapper
{
    public class ComMappingProvider
    {
        private readonly Dictionary<string, ComOperation> _mappings=new Dictionary<string, ComOperation>();



        public void Register(string order, ComOperation operation)
        {
            if (string.IsNullOrEmpty(order))
                throw new ArgumentNullException(nameof(order));
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (this._mappings.ContainsKey(order))
                throw new InvalidOperationException(nameof(operation));
            this._mappings.Add(order,operation);
        }

        public void TryRegister(string order, ComOperation operation)
        {
            if(!RegisteredOrder(order))
                   Register(order,operation);
        }



        public ComOperation GetMapping(string order)
        {
            
                if (this._mappings.ContainsKey(order))
                    return this._mappings[order];
            
            throw new KeyNotFoundException("not map any key");
        }

        public IEnumerable<string> GetOrders()=> _mappings.Keys;

        public bool RegisteredOrder(string order) => _mappings.ContainsKey(order);

        
        
        
    }
    
}
