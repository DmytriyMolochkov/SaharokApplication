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
                while (true)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Binder = new Type1ToType2DeserializationBinder();

                    object data = null;
                    data = formatter.Deserialize(Nstream);

                    FilesToPDFSort filesToPDFSort = null;
                    if (data is IFilesToProjectContainer)
                    {
                        filesToPDFSort = ((IFilesToProjectContainer)data).GetFilesToPDFSort();
                        formatter.Serialize(Nstream, InfoOfProcess.GetInstance());
                        formatter.Serialize(Nstream, filesToPDFSort);
                    }
                    else
                    {
                        formatter.Serialize(Nstream, $"Полученный сервером объект не поддерживает интерфейс: {typeof(IFilesToProjectContainer).Name}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                server.RemoveConnection(this.Id);
                Close();
            }
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
