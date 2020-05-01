using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ObjectsProjectServer;
using ObjectsToFormProjectServer;

namespace SaharokServer.Server
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Nstream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server; // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            //try
            //{
                Nstream = client.GetStream();
                // получаем имя пользователя
                //string message = GetMessage();
                //userName = message;

                //message = userName + " вошел в чат";
                //// посылаем сообщение о входе в чат всем подключенным пользователям
                //server.BroadcastMessage(message, this.Id);
                //Console.WriteLine(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        GetMessage();
                        //message = String.Format("{0}: {1}", userName, message);
                        //Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                    }
                    catch (Exception ex)
                    {
                        //message = String.Format("{0}: покинул чат", userName);
                        //Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                        //break;
                        Console.WriteLine(ex.Message);
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //    //Console.WriteLine(ex.Message);
            //}
            //finally
            //{
            //    // в случае выхода из цикла закрываем ресурсы
            //    server.RemoveConnection(this.Id);
            //    Close();
            //}
        }

        // чтение входящего сообщения и преобразование в строку
        private void GetMessage()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new Type1ToType2DeserializationBinder();

            //Project section = null;
            object infoClient = null;
            Console.WriteLine("Начинаю десериализацию");
            do
            {
                infoClient = /*(Project)*/formatter.Deserialize(Nstream);
            }
            while (Nstream.DataAvailable);

            Type qwe = infoClient.GetType();
            Console.WriteLine("Объект десериализован");
            infoClient.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Nstream != null)
                Nstream.Close();
            if (client != null)
                client.Close();
        }
    }
}
