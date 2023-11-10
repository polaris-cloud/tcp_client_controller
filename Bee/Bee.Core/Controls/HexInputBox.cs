using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bee.Core.Controls
{
        public class HexInputBox : TextBox
        {
        private const string HexPattern = "^[0-9A-Fa-f]*$";

        public HexInputBox()
        {
            this.TextChanged += OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = this.Text;
            string sanitizedText = Regex.Replace(text, $"[^0-9A-Fa-f]", ""); // Remove non-hex characters
            sanitizedText = AddSpaces(sanitizedText);
            this.Text = sanitizedText;
            this.CaretIndex = sanitizedText.Length; // Move cursor to the end
        }

        private string AddSpaces(string hexString)
        {
            const int chunkSize = 2;
            int length = hexString.Length;
            if (length <= chunkSize) return hexString;

            var chunks = new string[(length + chunkSize - 1) / chunkSize];
            for (int i = 0; i < length; i += chunkSize)
            {
                chunks[i / chunkSize] = hexString.Substring(i, Math.Min(chunkSize, length - i));
            }

            return string.Join(" ", chunks);
        }
    }
}
