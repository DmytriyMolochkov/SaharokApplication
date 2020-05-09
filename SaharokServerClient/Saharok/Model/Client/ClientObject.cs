using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObjectsProjectClient;
using Saharok.ViewModel;

namespace Saharok.Model.Client
{
    public class ClientObject : INotifyPropertyChanged
    {
        int Number;
        private string Host;
        private int Port;
        private readonly object _lock = new object();
        TcpClient client;
        NetworkStream Nstream;
        EchoStream Estream;

        CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        CancellationToken Token;


        public ClientObject(string host , int port, int numberServer)
        {
            client = new TcpClient();
            Host = host;
            Port = port;
            Number = numberServer;
        }


        private bool isServerConnect;
        public bool IsServerConnect
        {
            get
            {
                lock (_lock)
                {
                    return isServerConnect;
                }
            }
            set
            {
                lock (_lock)
                {

                    isServerConnect = value;
                    OnPropertyChanged(nameof(IsServerConnect));

                }
            }
        }

        // отправка сообщений
        public void SendMessage(object objectsToProject)
        {
            try
            {
                if (client.Connected)
                {
                    if(Estream != null)
                    {
                        Estream.Flush();
                    }
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(Nstream, objectsToProject);
                }
                else
                {
                    throw new ServerException($"Соединение с сервером №{Number} потеряно.       ");
                }
            }
            catch (Exception ex)
            {
                throw new ServerException(ex.Message);
            }
        }

        // получение сообщений
        public void CheckConnection()
        {
            Task.Run(() =>
            {
                CancelTokenSource = new CancellationTokenSource();
                try
                {
                    
                    Token = CancelTokenSource.Token;
                    Estream = new EchoStream(Token);
                    Nstream.CopyTo(Estream);
                }
                catch (Exception) { }
                Disconnect();
                IsServerConnect = client.Connected;
                CancelTokenSource.Cancel();
                Connect(true);
            });
        }

        public void CheckStream(CancellationToken token)
        {
            Task.Run(() =>
            {
            while (!token.IsCancellationRequested)
            {
                long lastLength = Estream.Length;
                Thread.Sleep(5000);
                if (lastLength == Estream.Length && !token.IsCancellationRequested)
                    {
                        Disconnect();
                    }
                }
            });
        }

        public void Connect(bool isCheckConnection = false)
        {
            Task.Run(() =>
            {
                while (!client.Connected)
                {
                    try
                    {
                        client.Connect(Host, Port); //подключение клиента
                        Nstream = client.GetStream(); // получаем поток
                        IsServerConnect = client.Connected;
                        if (isCheckConnection)
                            CheckConnection();
                    }
                    catch (Exception ex)
                    {
                        Disconnect();
                        IsServerConnect = client.Connected;
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        public FilesToPDFSort ReceiveMessage()
        {
            object data = null;
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new Type1ToType2DeserializationBinder();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            try
            {
                if (client.Connected)
                {
                    CancellationToken token = cancelTokenSource.Token;
                    CheckStream(token);
                    while (true)
                    {
                        data = null;
                        data = formatter.Deserialize(Estream);
                        if (data.GetType().Name == "InfoOfProcess")
                        {
                            InfoOfProcess.SetInstance((InfoOfProcess)data);
                        }
                        else if (data.GetType().Name == typeof(FilesToPDFSort).Name)
                        {
                            return (FilesToPDFSort)data;
                        }
                        else if (data.GetType().Name == typeof(string).Name)
                        {
                            throw new ServerDataException(data.ToString());
                        }
                        else
                        {
                            throw new ServerDataException("От сервера получена нечитаемая информация.");
                        }
                    }
                }
                else
                {
                    throw new ServerException($"Соединение с сервером №{Number} потеряно.       ");
                }
            }
            catch (ServerDataException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Операция была отменена.")
                {
                    Disconnect();
                    throw new ServerException($"Превышено вермя ожидания сервера №{Number}.        ");
                }
                else throw new ServerException(ex.Message);
            }
            finally
            {
                cancelTokenSource.Cancel(); ;
            }
        }

        public void Disconnect()
        {
            if (Nstream != null)
                Nstream.Close();//отключение потока
            if (client != null)
            {
                client.Close();//отключение клиента
            }
            client = new TcpClient();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServerException : Exception
    {
        public ServerException(string message)
            : base(message)
        { }
    }

    public class ServerDataException : Exception
    {
        public ServerDataException(string message)
            : base(message)
        { }
    }
}
