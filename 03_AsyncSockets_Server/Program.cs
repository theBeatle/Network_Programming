using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _03_AsyncSockets_Server
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder(); //Single!!! container for Msg from client
    }

    class Program
    {

        public static ManualResetEvent allDone = new ManualResetEvent(false); //
        // object for syncronization;
        static void Main(string[] args)
        {
            StartListener();

        }

        public static void StartListener()
        {
            //1 Config socket
            byte[] bytes = new byte[1024];
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPHostEntry ipHostInfo2 = Dns.GetHostEntry(Dns.GetHostName());

            //IPAddress ip;
            var ipV4 = ipHostInfo2.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToList(); // drop away IP of V6 protocol

            foreach (var item in ipV4)
            {
                Console.WriteLine(item);
            }

            //get current IP
            IPAddress localIpAddress = ipV4[0]; // local ip
            int localPort = 11000;
            //EndPoint created
            IPEndPoint localEndPoint = new IPEndPoint(localIpAddress, localPort);

            //Create socket and setup
            Socket listener = new Socket(
                AddressFamily.InterNetwork, //IPv4
                SocketType.Stream, // type of socket
                ProtocolType.Tcp //on which protocol
                );

            try
            {
                listener.Bind(localEndPoint); // connect socket to certain localEndPoint
                listener.Listen(100); // length of message's queue

                while (true)
                {
                    allDone.Reset(); // start object to syncronize

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // 1param - callback after Accept, 2param - socket original

                    //main thread waits for a Sync from AcceptCallback
                    allDone.WaitOne();
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" {ex}");
            }
            Console.WriteLine("Press Enter to continue...");
            Console.Read();

        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set(); // inform object od Sync. to continue main thread

            //Socket listener = (ar.AsyncState as Socket); //get access to original socket
            //Socket handler = listener.EndAccept(ar);
            Socket listener = (ar.AsyncState as Socket).EndAccept(ar);

            //Helper object to pass several parameters to thread
            StateObject state = new StateObject { workSocket = listener };

            listener.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            //start receiving msg from client
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            Socket listener = (ar.AsyncState as StateObject).workSocket;
            //string tmpContent = "";
            string tmpContent = String.Empty;
            int bytesReceived = listener.EndReceive(ar);
            //get bytes count from income stream

            if (bytesReceived > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesReceived));
                tmpContent = state.sb.ToString();
                if (tmpContent.IndexOf("<EOF>") > -1)// if Msg not empty and Receiving not ended
                {
                    //We got flag of Ending Message
                    Console.WriteLine($"Read {tmpContent.Length} from Socket \n Data : {tmpContent} ");
                    Send(listener, tmpContent);
                }
                else
                {
                    // Msg contains several parts
                    listener.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
            }
        }
        private static void Send(Socket listener, string tmpContent)
        {
            byte[] bytes = Encoding.ASCII.GetBytes($"{tmpContent} Server time: {DateTime.Now}");
            listener.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), listener);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            Socket listener = null;
            try
            {
                //get original socket
                listener = (ar.AsyncState as Socket);
                int bytesSent = listener.EndSend(ar);
                Console.WriteLine($"Bytes sent to client {bytesSent}");

                //close socket to both ends
                listener.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception has caught {ex}");
            }
            finally
            {
                listener.Close(); // free resources
            }
        }
    }
}

