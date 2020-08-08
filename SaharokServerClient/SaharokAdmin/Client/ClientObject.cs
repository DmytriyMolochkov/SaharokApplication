using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using SaharokAdmin.Client.Encryption;
using System.Runtime.InteropServices;

namespace SaharokAdmin.Client
{
    public class ClientObject : INotifyPropertyChanged
    {
        public int ServerNumber { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }
        private string HWID { get; set; }

        public TcpClient client { get; set; }
        NetworkStream Nstream { get; set; }
        EchoStream Estream { get; set; }
        MyAes myAes { get; set; }

        CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();
        CancellationToken Token { get; set; }

        private readonly object _lock = new object();

        public ClientObject(string host, int port, int defaultServerNumber)
        {
            client = new TcpClient();
            Host = host;
            Port = port;
            ServerNumber = defaultServerNumber;
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

        // получение сообщений
        public async void CheckConnection()
        {
            CancelTokenSource = new CancellationTokenSource();
            try
            {
                Token = CancelTokenSource.Token;
                Estream = new EchoStream(Token);
                await Task.Run(() => Nstream.CopyTo(Estream));
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

                        if (Estream != null)
                            Estream.Flush();

                        if (isCheckConnection)
                            CheckConnection();
                        HWID = GetMachineGuid();
                        string userName = SystemInformation.UserName;

                        myAes = new MyAes();
                        myAes.ImportParameters(Estream, Nstream);
                        myAes.EncryptToStream(new string[] { HWID, userName }, Nstream);
                        ServerNumber = (int)myAes.DecryptFromStream(Estream);
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

        public string Login(string login, string password)
        {
            myAes.EncryptToStream(new string[] { "lg", login, password }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
        }

        public string DisconnectionClients()
        {
            myAes.EncryptToStream(new string[] { "dc" }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
        }

        public string ListenClient()
        {
            myAes.EncryptToStream(new string[] { "lc" }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
        }

        public string DisconnectionAdmins()
        {
            myAes.EncryptToStream(new string[] { "da" }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
        }

        public string ClientsCount()
        {
            myAes.EncryptToStream(new string[] { "cc" }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
        }

        public string AdminsCount()
        {
            myAes.EncryptToStream(new string[] { "ac" }, Nstream);
            string[] data = (string[])myAes.DecryptFromStream(Estream);
            if (Convert.ToBoolean(data[0]))
                return data[1];
            else
                return data[1];
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
