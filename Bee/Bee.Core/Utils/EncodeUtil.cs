using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace Bee.Core.Utils
{
    public static class EncodeUtil
    {
         

         public static string HexArrayToString(IEnumerable<byte> byteArray, string separator)
         {
            return string.Join(separator, byteArray.Select(a => a.ToString("X2")))+separator;
        }


         public static string ConvertBackUtf8String(byte[] origin)
         {
             return Encoding.UTF8.GetString(origin);
         }


         public static byte[] ConvertUtf8StringToByteArray(string utf8)
        {
            return Encoding.UTF8.GetBytes(utf8);
        }


         /// <summary>
///  Example  :"ee 01 02"   ==> byte[]{0xee,0x01,0x02}
/// </summary>
/// <param name="hexLongString"></param>
/// <param name="separator"></param>
/// <returns></returns>
         public static IEnumerable<byte> ConvertHexStringToByteEnumerable(string hexLongString,string separator)
         {
            string stringWithoutSpaces = hexLongString.Replace(separator, "");
            return Enumerable.Range(0, stringWithoutSpaces.Length / 2)
                .Select(i => Convert.ToByte(stringWithoutSpaces.Substring(i * 2, 2), 16));

            //return hexLongString.Trim().Split(separator).Select(s =>Convert.ToByte(s)); 
         }


    }
}
