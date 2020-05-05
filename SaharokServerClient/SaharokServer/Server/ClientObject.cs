using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using ObjectsProjectServer;

namespace SaharokServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Nstream { get; private set; }
        string userName;
        public TcpClient client { get; set; }
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
            try
            {
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
                    //try
                    //{
                    Console.WriteLine("Я сплю");
                    Thread.Sleep(20000);
                    Console.WriteLine("Я опять готов");
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Binder = new Type1ToType2DeserializationBinder();

                    object data = null;
                    do
                    {
                        data = formatter.Deserialize(Nstream);
                    }
                    while (Nstream.DataAvailable);

                    FilesToPDFSort filesToPDFSort = null;
                    if (data is IFilesToProjectContainer)
                    {
                        filesToPDFSort = ((IFilesToProjectContainer)data).GetFilesToPDFSort();
                    }
                    else
                    {
                        throw new Exception($"Полученный сервером класс не поддерживает интерфейс: {typeof(IFilesToProjectContainer).Name}.");
                    }
                    formatter.Serialize(Nstream, InfoOfProcess.GetInstance());
                    formatter.Serialize(Nstream, filesToPDFSort);
                    //message = String.Format("{0}: {1}", userName, message);
                    //Console.WriteLine(message);
                    //server.BroadcastMessage(message, this.Id);
                    //}
                    //catch (Exception ex)
                    //{
                    //    //message = String.Format("{0}: покинул чат", userName);
                    //    //Console.WriteLine(message);
                    //    //server.BroadcastMessage(message, this.Id);
                    //    //break;
                    //    Console.WriteLine(ex.Message);
                    //}
                    //Thread.Sleep(10000);
                    //Console.WriteLine("Я опять готов");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        public void GetMessage()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new Type1ToType2DeserializationBinder();

            object data = null;
            do
            {
                data = formatter.Deserialize(Nstream);
            }
            while (Nstream.DataAvailable);

            FilesToPDFSort filesToPDFSort = null;
            if (data is IFilesToProjectContainer)
            {
                filesToPDFSort = ((IFilesToProjectContainer)data).GetFilesToPDFSort();
            }
            else
            {
                throw new Exception($"Полученный сервером класс не поддерживает интерфейс: {typeof(IFilesToProjectContainer).Name}.");
            }
            formatter.Serialize(Nstream, InfoOfProcess.GetInstance());
            formatter.Serialize(Nstream, filesToPDFSort);
            Console.WriteLine();
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
