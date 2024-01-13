using System;
using System.Text;
using Polaris.Protocol.enums;

namespace Polaris.Protocol.Converter
{
    internal class FrameConverter
    {
        internal static byte[] ConvertValueStringToBytes(
            ProtocolEndian endian,
            ProtocolEncodeFormat format,
            string value,
            int dataLength)
        {
            byte[] data;

            switch (format)
            {
                case ProtocolEncodeFormat.Hex:
                    data = ToHexArray(value, dataLength);
                    break;
                case ProtocolEncodeFormat.String:
                    data = Encoding.UTF8.GetBytes(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
            if (BitConverter.IsLittleEndian ^ endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }


        internal static string ConvertBytesToValueString(
            ProtocolEndian endian,
            ProtocolEncodeFormat format,
            byte[] value,
            int dataLength)
        {
            string data;
            if (BitConverter.IsLittleEndian ^ endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(value);
            }
            switch (format)
            {
                case ProtocolEncodeFormat.Hex:
                    data = ToHexNum(value, dataLength).ToString();
                    break;
                case ProtocolEncodeFormat.String:
                    data = Encoding.UTF8.GetString(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            return data;
        }

        internal static byte[] GenerateCheckBytes(
            ProtocolEndian endian,
            FrameCheckMethod checkMethod,
            byte[] toCheck)
        {

            byte[] data;
            switch (checkMethod)
            {
                case FrameCheckMethod.None:
                    data = Array.Empty<byte>();
                    break;
                case FrameCheckMethod.Sum:
                    data = BitConverter.GetBytes(FrameChecker.sum_check(toCheck));
                    break;
                case FrameCheckMethod.CRC16:
                    data = BitConverter.GetBytes(FrameChecker.CRC16_Check_T(toCheck, toCheck.Length));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(checkMethod), checkMethod, null);
            }
            if (BitConverter.IsLittleEndian ^ endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }

        internal static byte[] GenerateCheckBytes(
            ProtocolEndian endian,
            FrameCheckMethod checkMethod,
            byte[] toCheck,
            int dataLength
            )
        {

            byte[] data = new byte[dataLength];
            switch (checkMethod)
            {
                case FrameCheckMethod.None:
                    data = Array.Empty<byte>();
                    break;
                case FrameCheckMethod.Sum:
                    BitConverter.TryWriteBytes(data, FrameChecker.sum_check(toCheck));
                    break;
                case FrameCheckMethod.CRC16:
                    BitConverter.TryWriteBytes(data, FrameChecker.CRC16_Check_T(toCheck, toCheck.Length));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(checkMethod), checkMethod, null);
            }
            if (BitConverter.IsLittleEndian ^ endian == ProtocolEndian.LittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }



        internal static byte[] GenerateCheckBytes(
            ProtocolEndian endian,
            FrameCheckMethod checkMethod,
            string toCheck)
        {
            throw new NotImplementedException();
        }

        private static byte[] ToHexArray(string value, int dataLength)
        {
            //return value.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            byte[] data;
            switch (dataLength)
            {
                case 4:
                    data = BitConverter.GetBytes(Convert.ToInt32(value));
                    break;
                case 2:
                    data = BitConverter.GetBytes(Convert.ToInt16(value));
                    break;
                case 1:
                    data = new[] { Convert.ToByte(value) };
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(dataLength), dataLength, null);

            }
            return data;
        }

        private static int ToHexNum(byte[] value, int dataLength)
        {

            int data;
            switch (dataLength)
            {
                case 4:
                    data = BitConverter.ToInt32(value, 0);
                    break;
                case 2:
                    data = BitConverter.ToInt16(value, 0);
                    break;
                case 1:
                    data = value[0];
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(dataLength), dataLength, null);

            }
            return data;
        }
    }
}
