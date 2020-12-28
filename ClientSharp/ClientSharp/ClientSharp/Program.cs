using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientSharp
{
	class Program
    {
        static int myId;
        static Mutex hMutex = new Mutex();
        static int nPort = 12345;
        static IPAddress ipAddr = IPAddress.Parse("127.0.0.1");

        static void MyChannel(IPEndPoint endPoint, ref Message msg, ref MsgHeader h_msg, ref Socket client, Messages Type, int ID_ToClient, int ID_FromClient, string textMsg = "")
        {
            try
            {
                client.Connect(endPoint);
            }
            catch (SocketException ex)
            {
                Console.Write("Ошибка подключения!");
                Environment.Exit(0);
            }
            finally
            {
                msg.SendMessage(client, ID_ToClient, ID_FromClient, Type, textMsg);
                h_msg = msg.Receive(client);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }

        }

        static void GetDataFromServer()
        {
            while (true)
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(ipAddr, nPort);

                Message msg = new Message();
                MsgHeader h_msg = new MsgHeader();

                MyChannel(endPoint, ref msg, ref h_msg, ref client, Messages.M_GETDATA, 0, myId);

                string MsgText = "";

                if (h_msg.m_Type == Messages.M_TEXT)
                {
                    MsgText = msg.getMsgText();
                    hMutex.WaitOne();
                    Console.WriteLine("Сообщение от клиента: " + MsgText);
                    hMutex.ReleaseMutex();
                }
                Thread.Sleep(2000);
            }
        }

        static void ConnectToServer(IPEndPoint endPoint, ref Message m, ref MsgHeader h_msg, ref Socket client)
        {

            MyChannel(endPoint, ref m, ref h_msg, ref client, Messages.M_INIT, 0, 0);

            if (h_msg.m_Type == Messages.M_CONFIRM)
            {
                myId = h_msg.m_To;
                Console.WriteLine("Ваш ID = " + myId);
                Thread getDataThread = new Thread(GetDataFromServer);
                getDataThread.Start();
            }
            else
            {
                Console.WriteLine("Ошибка. Клиент не подключен." );
                return;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Выберите: 1 - подключиться к серверу. \n 0 - выйти.\n");
            int result = Convert.ToInt32(Console.ReadLine());
            if(result == 0) Environment.Exit(0);
            if(result == 1)
            {
                //Подключение к серверу
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(ipAddr,nPort);
          
                Message msg = new Message();
                MsgHeader h_msg = new MsgHeader();
                ConnectToServer(endPoint, ref msg, ref h_msg, ref client);

                while (true)
                {
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    endPoint = new IPEndPoint(ipAddr, nPort);

                    string textMsg;
                    int ID_ToClient;

                    Console.WriteLine("Отправить сообщение: 0 - всем или 1 - одному клиенту ?\n2 - чтобы выйти из приложения. ");
                    int s = Convert.ToInt32(Console.ReadLine());
                    switch (s)
                    {
                        case 1:
                            {
                                Console.WriteLine("Введите ID клиента:");
                                ID_ToClient = Convert.ToInt32(Console.ReadLine());

                                Console.WriteLine("Введите сообщение: ");
                                textMsg = Console.ReadLine().ToString();

                                MyChannel(endPoint, ref msg, ref h_msg, ref client, Messages.M_TEXT, ID_ToClient, myId, textMsg);
                                if (h_msg.m_Type == Messages.M_CONFIRM) Console.WriteLine("Сообщение отправлено!");
                                else Console.WriteLine("Сообщение не было отправлено!");

                                break;
                            }
                        case 0:
                            {
                                Console.WriteLine("Введите сообщение: ");
                                textMsg = Console.ReadLine().ToString();
                                MyChannel(endPoint, ref msg, ref h_msg, ref client, Messages.M_TEXT, (int)Members.M_ALL , myId, textMsg);

                                if (h_msg.m_Type == Messages.M_CONFIRM) Console.WriteLine("Сообщение отправлено!");
                                else Console.WriteLine("Сообщение не было отправлено!");

                                break;
                            }
                        case 2:
                            {
                                MyChannel(endPoint, ref msg, ref h_msg, ref client, Messages.M_EXIT, 0, myId);
                                if (h_msg.m_Type == Messages.M_CONFIRM) Console.WriteLine("Вы успешно вышли!");
                                else Console.WriteLine("Ошибка.");
                                Environment.Exit(0);
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("Ошибка ввода. Выход!");
                                Environment.Exit(0);
                                return;
                            }
                    }

                }
            }
            else
            {
                Console.WriteLine("Ошибка ввода. Выход!");
                return;
            }
        }
    }
}
