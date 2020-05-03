using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ObjectsProjectClient;

namespace Saharok.Model.Client
{
    public class ClientObject
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream Nstream;


        // отправка сообщений
        public static void SendMessage(Project project)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(Nstream, project);
        }
        public static void SendMessage(TypeDocumentation typeDocumentation)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(Nstream, typeDocumentation);
        }
        public static void SendMessage(Section section)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(Nstream, section);
        }
        public static void SendMessage(FileSection file)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(Nstream, file);
        }
        // получение сообщений
        public static FilesToPDFSort ReceiveMessage()
        {
            while (true)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new Type1ToType2DeserializationBinder();

                object data = null;
                Console.WriteLine("Начинаю десериализацию");
                do
                {
                    data = formatter.Deserialize(Nstream);
                }
                while (Nstream.DataAvailable);
                if (data.GetType().Name == "InfoOfProcess")
                    InfoOfProcess.SetInstance((InfoOfProcess)data);
                else
                    return (FilesToPDFSort)data;
            }
        }

        public static void Connect()
        {
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                Nstream = client.GetStream(); // получаем поток
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //finally
            //{
            //    Disconnect();
            //}
        }

        static void Disconnect()
        {
            if (Nstream != null)
                Nstream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
}
