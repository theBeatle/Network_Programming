using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _04_AsyncSockets_Client
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
    class Program
    {
        //порт для сокета
        private const int port = 11000;

        //об"єкти синхронізації для кожного етапа 
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static String response = String.Empty;

        private static void StartClient()
        {
            try
            {
                //створення точки підключення для сокета
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[2];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                var ipV4 = ipHostInfo.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToList(); // drop away IP of V6 protocol
                foreach (var item in ipV4)
                {
                    Console.WriteLine(item);
                }

                //створення на основі точки підключення самого сокета
                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    //початок процедури з"єднання з сервером
                    client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);

                    //тут завмирає основний потік доки не прийде команда з колбека
                    connectDone.WaitOne();

                    //виклик ф-ції відправки повідомлення на сервер
                    Send(client, "This ilkjlkjkljs a te;'l';l;l;'st<EOF> second");
                    //тут завмирає основний потік доки не прийде команда з колбека
                    sendDone.WaitOne();

                    // отримання відповіді від сервера
                    Receive(client);
                    //тут завмирає основний потік доки не прийде команда з колбека
                    receiveDone.WaitOne();


                    Console.WriteLine($"Response received : {response}");

                    //закриття сокета на передачу і прийом
                    client.Shutdown(SocketShutdown.Both);
                    //звільнення ресурсів сокета
                    client.Close();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //колбек етапу під"єднання
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //з параметру колбека витягуємо сам сокет на якому працюємо
                Socket client = (Socket)ar.AsyncState;
                //завершуєм процедуру під"єднання
                client.EndConnect(ar);

                Console.WriteLine($"Socket connected to {client.RemoteEndPoint}");
                //відправляєм команду в основний потік про можливість продовження роботи
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //ф-ція отримання даних
        private static void Receive(Socket client)
        {
            try
            {
                //запихуємо ссилку на сокет в клас і передаєм його як параметр
                //в наступний етап
                StateObject state = new StateObject
                {
                    workSocket = client
                };
                //початок етапу отримання відповіді від сервера
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //колбек етапу прийому
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //з параметру колбека витягуємо сам сокет на якому працюємо
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // завершуєм етап отримання відповіді від сервера
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //отримує і вичитуємо повідомлення 
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    //вичитуємо все з відповіді
                    client.BeginReceive(state.buffer, 0,
                        StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                //якщо в буфері немає повідомлень починаєм етап закриття сокета
                else
                {


                    if (state.sb.Length > 1)
                    {
                        //відповідь від сервера
                        response = state.sb.ToString();
                    }
                    //відправляєм в мейн команду про продовження його роботи
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //ф-ція відправки повідомлення на сервер
        private static void Send(Socket client, String data)
        {

            byte[] byteData = Encoding.ASCII.GetBytes(data);

            //початок етапу відправки
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                //отримуємо сокет з параметра
                Socket client = (Socket)ar.AsyncState;
                //кінець етапу відправки
                int bytesSent = client.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to server.");
                //розморожуємо головний потік
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }
    }
}
