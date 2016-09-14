using System;
using System.Configuration;
using System.Text;
using System.Net;
using System.IO;
using Ionic.Zlib;


namespace IncapsulaLogDownloader
{
    class Program
    {
        private static readonly string apiId = ConfigurationManager.AppSettings["APIid"];
        private static readonly string apiKey = ConfigurationManager.AppSettings["APIkey"];
        private static readonly string lastDownloadedFilePath = ConfigurationManager.AppSettings["LastKnownDownloadedFilePath"];

        static void Main(string[] args)
        {
            var lastDownloadedFileId = File.ReadAllText(lastDownloadedFilePath);
            var nextFileToDownload = "441_" + (int.Parse(lastDownloadedFileId.Substring(4, 6)) + 1) + ".log";
            DowloadFile(nextFileToDownload);
        }

        private static void DowloadFile(string fileToDownload)
        {
            var targeturl = "https://logs1.incapsula.com//441_525574/" + fileToDownload;
            var request = (HttpWebRequest)WebRequest.Create(targeturl);
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiId + ":" + apiKey));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var receiveStream = response.GetResponseStream();
                var ms = new MemoryStream();
                CopyStream(receiveStream, ms);
                var delimiter = GetBytes("|==|\n");
                var newBytes = new byte[5];
                var j = 0;
                for (var i = 0; i < delimiter.Length; i = i + 2)
                {
                    newBytes[j] = delimiter[i];
                    j++;
                }
                var byteArray = ms.ToArray();
                var zipStart = FindBytes(byteArray, newBytes);
                var newFileStream = new byte[byteArray.Length - zipStart - 5];
                var k = 0;
                for (var i = zipStart + 5; i < byteArray.Length; i++)
                {
                    newFileStream[k] = byteArray[i];
                    k++;
                }
                var unzippedContent = ZlibStream.UncompressString(newFileStream);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }


        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

    }
}



