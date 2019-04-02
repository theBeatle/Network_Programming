using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _07_FTP_client
{
    class Program
        {
            static void Main(string[] args)
            {
                //Console.ReadLine();
                // create FtpWebRequest
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.7.180.101:21/regex.pdf");
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential("test_user", "1234567890");
                //request.EnableSsl = true; // если используется ssl

                // get answer as FtpWebResponse
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                // get stream and save as file 
                Stream responseStream = response.GetResponseStream();
                FileStream fs = new FileStream("new_regex.pdf", FileMode.Create);
                byte[] buffer = new byte[64];
                int size = 0;

                while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, size);
                }
                fs.Close();
                response.Close();

                Console.WriteLine("Saving complete");
                Console.Read();
            }
        }
    }
