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
            //Token = CancelTokenSource.Token;
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

        //private bool cryticalError;
        //bool CryticalError
        //{
        //    get
        //    {
        //        lock (_lock)
        //        {
        //            return cryticalError;
        //        }
        //    }
        //    set
        //    {
        //        lock (_lock)
        //        {

        //            cryticalError = value;
        //            OnPropertyChanged(nameof(CryticalError));

        //        }
        //    }
        //}


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
                    do
                    {
                        Nstream.CopyTo(Estream);
                    }
                    while (Nstream.DataAvailable);
                }
                catch (Exception)
                {
                    IsServerConnect = client.Connected;
                    CancelTokenSource.Cancel();
                    Connect(true);
                }
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
                        object data = null;
                        data = formatter.Deserialize(Estream);
                        if (data.GetType().Name == "InfoOfProcess")
                            InfoOfProcess.SetInstance((InfoOfProcess)data);
                        else
                        {
                            return (FilesToPDFSort)data;
                        }
                    }
                }
                else
                {
                    throw new ServerException($"Соединение с сервером №{Number} потеряно.       ");
                }
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








    //public class ClientObject
    //{
    //    private const string host = "127.0.0.1";
    //    private const int port = 8888;
    //    private static readonly object _lock = new object();
    //    static TcpClient client = new TcpClient();
    //    static NetworkStream Nstream;
    //    static EchoStream Estream;

    //    private static bool isServer1Connect;
    //    public static bool IsServer1Connect
    //    {
    //        get
    //        {
    //            lock (_lock)
    //            {
    //                return isServer1Connect;
    //            }
    //        }
    //        set
    //        {
    //            lock (_lock)
    //            {
    //                isServer1Connect = value;
    //                OnPropertyChanged(nameof(IsServer1Connect));
    //            }
    //        }
    //    }

    //    // отправка сообщений
    //    public static void SendMessage(object objectsToProject)
    //    {
    //        try
    //        {
    //            if (client.Connected)
    //            {
    //                BinaryFormatter formatter = new BinaryFormatter();
    //                formatter.Serialize(Nstream, objectsToProject);
    //            }
    //            else
    //            {
    //                throw new Exception("Соединение с сервером потеряно.       ");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    // получение сообщений
    //    public static void CheckConnection()
    //    {
    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                Estream = new EchoStream();
    //                do
    //                {
    //                    Nstream.CopyTo(Estream);
    //                }
    //                while (Nstream.DataAvailable);
    //            }
    //            catch (Exception)
    //            {
    //                IsServer1Connect = client.Connected;
    //                Connect(true);
    //            }
    //        });
    //    }

    //    public static void CheckStream(CancellationToken token)
    //    {
    //        Task.Run(() =>
    //        {
    //            while (!token.IsCancellationRequested)
    //            {
    //                long lastLength = Estream.Length;
    //                Thread.Sleep(5000);
    //                if (lastLength == Estream.Length && !token.IsCancellationRequested)
    //                {
    //                    Estream.ReadTimeout = 1;
    //                    Estream.Write(new byte[1] { 0 }, 0, 1);
    //                }
    //            }
    //        });
    //    }

    //    public static void Connect(bool isCheckConnection = false)
    //    {
    //        Task.Run(() =>
    //        {
    //            while (!client.Connected)
    //            {
    //                try
    //                {
    //                    client.Connect(host, port); //подключение клиента
    //                    Nstream = client.GetStream(); // получаем поток
    //                    IsServer1Connect = client.Connected;
    //                    if (isCheckConnection)
    //                        CheckConnection();
    //                }
    //                catch (Exception ex)
    //                {
    //                    Disconnect();
    //                    lock (_lock)
    //                    {
    //                        client = new TcpClient();
    //                    }
    //                    Thread.Sleep(5000);
    //                }
    //            }
    //        });
    //    }

    //    public static FilesToPDFSort ReceiveMessage()
    //    {
    //        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    //        CancellationToken token = cancelTokenSource.Token;
    //        BinaryFormatter formatter = new BinaryFormatter();
    //        formatter.Binder = new Type1ToType2DeserializationBinder();
    //        try
    //        {
    //            if (client.Connected)
    //            {
    //                CheckStream(token);
    //                while (true)
    //                {
    //                    object data = null;
    //                    data = formatter.Deserialize(Estream);
    //                    if (data.GetType().Name == "InfoOfProcess")
    //                        InfoOfProcess.SetInstance((InfoOfProcess)data);
    //                    else
    //                    {
    //                        return (FilesToPDFSort)data;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                throw new Exception("Соединение с сервером потеряно.       ");
    //            }
    //        }
    //        catch (Exception ex)
    //        {

    //            Disconnect();
    //            Connect();
    //            if (ex.Message == "Конец потока обнаружен до завершения разбора.")
    //                throw new Exception("Превышено вермя ожидания сервера.        ");
    //            else throw ex;
    //        }
    //        finally
    //        {
    //            cancelTokenSource.Cancel(); ;
    //        }
    //    }

    //    public static void Disconnect()
    //    {
    //        if (Nstream != null)
    //            Nstream.Close();//отключение потока
    //        if (Estream != null)
    //            Estream.Close();//отключение потока
    //        if (client != null)
    //        {
    //            client.Close();//отключение клиента
    //            IsServer1Connect = client.Connected;
    //        }  
    //    }

    //    public static event PropertyChangedEventHandler PropertyChanged;

    //    private static void OnPropertyChanged(string propertyName = "")
    //    {
    //        PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    //    }
    //}
}
