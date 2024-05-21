using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Polaris.Protocol.Model;

namespace Bee.Modules.Script.Shared.Intellisense
{
      public static class BindingProtocols
    {
        public static readonly DependencyProperty ProtocolFormatsProperty = DependencyProperty.RegisterAttached(
            "ProtocolFormats", typeof(IEnumerable<ProtocolFormat>), typeof(BindingProtocols), new PropertyMetadata(null,null));

        public static void SetProtocolFormats(DependencyObject element, IEnumerable<ProtocolFormat> value)
        {
            element.SetValue(ProtocolFormatsProperty, value);
        }

        public static IEnumerable<ProtocolFormat> GetProtocolFormats(DependencyObject element)
        {
            return (IEnumerable<ProtocolFormat>)element.GetValue(ProtocolFormatsProperty);
        }
    }
}
