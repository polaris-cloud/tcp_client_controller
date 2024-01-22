using System;
using System.IO;

namespace Polaris.Storage.Stream
{
    public static class FileUtil
    {
        //如何将超大的 double 数组逐块写入文件   
        public static void SaveLargeDoubleArrayToFile(double[] dataArray, string filePath, int bufferSize)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            var buffer = new byte[bufferSize];
            int startIndex = 0;

            while (startIndex < dataArray.Length)
            {
                var bytesToWrite = Math.Min(bufferSize, dataArray.Length - startIndex);

                // 将 double 数组转换为字节数组
                Buffer.BlockCopy(dataArray, startIndex * sizeof(double), buffer, 0, bytesToWrite * sizeof(double));

                // 写入缓冲区数据到文件
                fileStream.Write(buffer, 0, bytesToWrite * sizeof(double));

                startIndex += bytesToWrite;
            }
        }

        //使用 MemoryStream 和 FileStream 的组合方式来处理大型文件
        public static void SaveLargeFileToFileSystem(string sourceFilePath, string targetFilePath, int bufferSize)
        {
            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            using var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, bufferSize)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
            }
        }


        //  文本写入流中
        //二进制写入流中
    }
}
