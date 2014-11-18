using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

using System.Text;
using System.Collections;

using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using ICSharpCode.SharpZipLib.BZip2;

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

/// <summary>
/// 压缩文件
/// </summary>
namespace SenserModels
{
    public class ZipCompresser
    {
        public void Zip(string fileToZip, string zipedFile, int compressionLevel, int blockSize)
        {
            //如果文件没有找到，则报错
            if (!System.IO.File.Exists(fileToZip))
            {
                throw new System.IO.FileNotFoundException("指定的文件 " + fileToZip + " 没有被找到. 压缩终止");
            }

            System.IO.FileStream streamToZip = new System.IO.FileStream(fileToZip, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.FileStream zipedFileStream = System.IO.File.Create(zipedFile);
            ZipOutputStream zipedOutputStream = new ZipOutputStream(zipedFileStream);
            ZipEntry zipEntry = new ZipEntry(zipedFile);
            zipedOutputStream.PutNextEntry(zipEntry);
            zipedOutputStream.SetLevel(compressionLevel);
            byte[] buffer = new byte[blockSize];
            System.Int32 size = streamToZip.Read(buffer, 0, buffer.Length);
            zipedOutputStream.Write(buffer, 0, size);
            try
            {
                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipedOutputStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }

            catch (System.Exception ex)
            {
                throw ex;
            }

            zipedOutputStream.Finish();
            zipedOutputStream.Close();
            streamToZip.Close();
        }

        public void Zip(string[] args)
        {
            string[] filenames = Directory.GetFiles(args[0]);

            Crc32 crc = new Crc32();
            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(args[1]));

            zipOutputStream.SetLevel(6); // 0 - store only to 9 - means best compression

            foreach (string file in filenames)
            {
                //打开压缩文件
                FileStream fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                ZipEntry entry = new ZipEntry(file);

                entry.DateTime = DateTime.Now;

                // set Size and the crc, because the information
                // about the size and crc should be stored in the header
                // if it is not set it is automatically written in the footer.
                // (in this case size == crc == -1 in the header)
                // Some ZIP programs have problems with zip files that don't store
                // the size and crc in the header.
                entry.Size = fs.Length;
                fs.Close();

                crc.Reset();
                crc.Update(buffer);

                entry.Crc = crc.Value;

                zipOutputStream.PutNextEntry(entry);

                zipOutputStream.Write(buffer, 0, buffer.Length);

            }

            zipOutputStream.Finish();
            zipOutputStream.Close();
        }

        public void UnZip(string[] args)
        {
            ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(args[0]));

            ZipEntry theEntry;
            while ((theEntry = zipInputStream.GetNextEntry()) != null)
            {

                string directoryName = Path.GetDirectoryName(args[1]);
                string fileName = Path.GetFileName(theEntry.Name);

                //生成解压目录
                Directory.CreateDirectory(directoryName);

                if (fileName != String.Empty)
                {
                    //解压文件到指定的目录
                    FileStream streamWriter = File.Create(args[1] + theEntry.Name);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = zipInputStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                }
            }
            zipInputStream.Close();
        }
    }
}




