using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _06_UDP_Client
{
    class Program
        {
            static void Main()
            {
                StartSender();
            }

            private static void StartSender()
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                    bool done = false;
                    while (!done)
                    {
                        // Oleh's IP 10.7.180.104
                        Console.WriteLine("Enter msg for Server:");
                        string mes = Console.ReadLine();
                        byte[] sendbuf = Encoding.ASCII.GetBytes(mes);
                    s.SendTo(sendbuf, remoteEP);
                        Console.WriteLine("Message sent to the broadcast address");
                        if (mes == "exit")
                        {
                            done = true;
                        }
                    }
                }
            }
        }
    }