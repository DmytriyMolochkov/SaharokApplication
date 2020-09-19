using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Saharok.Model.Client.Encryption;

namespace Saharok.Model.Client
{
    public class ClientObject : INotifyPropertyChanged
    {
        public int Number { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }
        private string HWID { get; set; }
        private readonly object _lock = new object();
        public TcpClient client { get; set; }
        NetworkStream Nstream;
        EchoStream Estream;
        MyAes myAes;

        CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        CancellationToken Token;


        public ClientObject(string host, int port, int numberServer)
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
                    if (Estream != null)
                        Estream.Flush();

                    myAes = new MyAes();
                    myAes.ImportParameters(Estream, Nstream);
                    myAes.EncryptToStream(objectsToProject, Nstream);
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
        public async void CheckConnection()
        {
            CancelTokenSource = new CancellationTokenSource();
            try
            {
                Token = CancelTokenSource.Token;
                Estream = new EchoStream(Token);
                await System.Threading.Tasks.Task.Run(() => Nstream.CopyTo(Estream));
            }
            catch (Exception) { }
            Disconnect();
            CancelTokenSource.Cancel();
            IsServerConnect = client.Connected;
            Thread.Sleep(1000);
            Connect(true);
        }

        public void CheckStream(CancellationToken token)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    long lastLength = Estream.Length;
                    Thread.Sleep(10000);
                    if (lastLength == Estream.Length && !token.IsCancellationRequested)
                    {
                        Disconnect();
                    }
                }
            });
        }

        public void Connect(bool isCheckConnection = false)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while ((!client?.Connected) ?? true)
                {
                    try
                    {
                        client.Connect(Host, Port); //подключение клиента
                        Nstream = client.GetStream(); // получаем поток
                        IsServerConnect = client.Connected;

                        if (Estream != null)
                            Estream.Flush();

                        if (isCheckConnection)
                            CheckConnection();
                        HWID = GetMachineGuid();
                        string userName = SystemInformation.UserName;
                        myAes = new MyAes();

                        myAes.ImportParameters(Estream, Nstream);
                        myAes.EncryptToStream(new string[] { HWID, userName }, Nstream);
                    }
                    catch (Exception ex)
                    {
                        Disconnect();
                        IsServerConnect = client.Connected;
                    }
                }
            });
        }

        public FilesToPDFSort ReceiveMessage()
        {
            object data;
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
                        data = myAes.DecryptFromStream(Estream);

                        if (data is InfoOfProcess)
                        {
                            InfoOfProcess.SetInstance((InfoOfProcess)data);

                        }
                        else if (data is FilesToPDFSort)
                        {
                            return (FilesToPDFSort)data;
                        }
                        else if (data is string)
                        {
                            throw new ServerDataException(data.ToString());
                        }
                        else
                        {
                            throw new ServerDataException("От сервера получена нечитаемая информация.       ");
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
                    //Disconnect();
                    throw new ServerException($"Превышено вермя ожидания сервера №{Number}.        ");
                }
                else throw new ServerException(ex.Message);
            }
            finally
            {
                cancelTokenSource.Cancel();
            }
        }

        public void Disconnect()
        {
            if (Nstream != null)
                Nstream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента

            client = new TcpClient();
        }

        private string GetMachineGuid()
        {
            string location = @"SOFTWARE\Microsoft\Cryptography";
            string name = "MachineGuid";

            using (RegistryKey localMachineX64View =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(
                            string.Format("Key Not Found: {0}", location));

                    object machineGuid = rk.GetValue(name);
                    if (machineGuid == null)
                        throw new IndexOutOfRangeException(
                            string.Format("Index Not Found: {0}", name));

                    return machineGuid.ToString();
                }
            }
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
