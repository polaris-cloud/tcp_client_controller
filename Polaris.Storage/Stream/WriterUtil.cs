using System.Globalization;
using System.IO;

namespace Polaris.Storage.Stream;

public static class WriterUtil
{
    public static void SaveDoubleArrayToStreamInString(double[] array,System.IO.Stream stream)
    {
        using var writer = new StreamWriter(stream);
        foreach (var value in array)
        {
            writer.WriteLine(value.ToString(CultureInfo.InvariantCulture));
        }
    }

    public static void SaveDoubleArrayToStreamInBinary(double[] array, System.IO.Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        foreach (var value in array)
        {
            writer.Write(value);
        }
    }
}