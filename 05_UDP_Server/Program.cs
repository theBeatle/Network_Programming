using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _05_UDP_Server
{
    class Msg
    {
        public IPAddress Adress { get; set; }

        public int Count { get; set; }
    }
    class Program
    {
        private const int listenPort = 12000;

        private static void StartListener()
        {
            List<Msg> list = new List<Msg>();
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            //IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("10.7.110.45"), listenPort); //for exect IP listening

            Stopwatch sw = new Stopwatch();

            try
            {
                while (!done)
                {

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    for (int i = 0; i < list.Count; i++)
                    {
                        Console.WriteLine($" {list[i].Adress} - {list[i].Count} ");
                    }

                    Console.ResetColor();
                    Console.WriteLine("Waiting for msg");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                                       //list.ForEach(l => Console.WriteLine($" {l.Adress} - {l.Count} "));
                    

                    //Console.WriteLine($"Received msg from {groupEP} at {DateTime.Now}:\n {msg}\n");

                    if (list.Count > 0 &&  list.Count( s => s.Adress.ToString() == groupEP.Address.ToString()) > 0)
                        list.First(a => a.Adress.ToString() == groupEP.Address.ToString()).Count += 1;
                    else
                        list.Add(new Msg { Adress = groupEP.Address, Count = 1 });

                    
                    if (msg == "exit")
                    {
                        done = true;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }

        public static int Main()
        {
            StartListener();
            return 0;
        }
    }
}

