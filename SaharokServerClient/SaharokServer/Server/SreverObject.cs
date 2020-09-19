using Microsoft.EntityFrameworkCore;
using SaharokServer.Server.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaharokServer
{
    public class ServerObject
    {
        public TcpListener tcpListenerClient; // сервер для прослушивания
        public TcpListener tcpListenerAdmin; // сервер для прослушивания
        public List<ClientObject> clients = new List<ClientObject>(); // все подключения
        public List<ClientObject> admins = new List<ClientObject>(); // все подключения
        private object _lock = new Object();
        public int ServerNumber { get; set; }
        public int UserPort { get; set; }
        public int AdminPort { get; set; }
        public bool AllowUsingNewClient { get; set; }
        public string DBname { get; set; }

        public ServerObject(int serverNumber, int userPort, int adminPort, bool allowUsingNewClient, string dbName)
        {
            ServerNumber = serverNumber;
            UserPort = userPort;
            AdminPort = adminPort;
            AllowUsingNewClient = allowUsingNewClient;
            DBname = dbName;
        }
        protected internal void AddConnectionClient(ClientObject clientObject)
        {
            lock (_lock)
            {
                clients.Add(clientObject);
            }
        }

        protected internal void AddConnectionAdmin(ClientObject clientObject)
        {
            lock (_lock)
            {
                admins.Add(clientObject);
            }
        }

        protected internal void RemoveConnectionClient(string id)
        {
            lock (_lock)
            {
                // получаем по id закрытое подключение
                ClientObject client = clients.FirstOrDefault(c => c.Id == id);
                // и удаляем его из списка подключений
                if (client != null)
                    clients.Remove(client);
            }
        }

        protected internal void RemoveConnectionAdmin(string id)
        {
            lock (_lock)
            {
                // получаем по id закрытое подключение
                ClientObject admin = admins.FirstOrDefault(c => c.Id == id);
                // и удаляем его из списка подключений
                if (admin != null)
                    admins.Remove(admin);
            }
        }

        // прослушивание входящих подключений клентов
        protected internal async Task ListenClientAsync()
        {
            try
            {
                tcpListenerClient = new TcpListener(IPAddress.Any, UserPort);
                tcpListenerClient.Start();
                while (true)
                {
                    TcpClient tcpClient = await Task.Run(() => tcpListenerClient.AcceptTcpClientAsync());
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.ProcessUser));
                    clientThread.Start();
                    Console.WriteLine("Подлкючился клиент");
                }
            }
            catch (Exception ex)
            {
                Logs.TryExecute(()=>Logs.ErrorServerObject(ex, this));
                if (!(ex is ObjectDisposedException))
                {
                    DisconnectClients();
                    Thread.Sleep(60000);
                    ListenClientAsync();
                }
            }
        }

        // прослушивание входящих подключений клентов
        protected internal async Task ListenAdminAsync()
        {
            try
            {
                tcpListenerAdmin = new TcpListener(IPAddress.Any, AdminPort);
                tcpListenerAdmin.Start();
                while (true)
                {
                    TcpClient tcpClient = await Task.Run(() => tcpListenerAdmin.AcceptTcpClientAsync());
                    ClientObject clientObject = new ClientObject(tcpClient, this, true);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.ProcessAdmin));
                    clientThread.Start();
                    Console.WriteLine("Подлкючился админ");
                }
            }
            catch (Exception ex)
            {
                Logs.TryExecute(()=>Logs.ErrorServerObject(ex, this));
                if (!(ex is ObjectDisposedException))
                {
                    DisconnectAdmins();
                    Thread.Sleep(60000);
                }
                if (ex.Message != "Cannot access a disposed object.\r\nObject name: 'System.Net.Sockets.Socket'.")
                    ListenAdminAsync();
            }
        }

        // отключение всех клиентов
        protected internal void DisconnectClients()
        {
            tcpListenerClient?.Stop(); //остановка сервера для юзеров

            for (int i = 0; i < clients?.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
        }

        protected internal void DisconnectAdmins()
        {
            tcpListenerAdmin?.Stop(); //остановка сервера для админов

            for (int i = 0; i < admins?.Count; i++)
            {
                admins[i].Close(); //отключение админа
            }
        }

        protected internal void DisconnectAdmins(string myID)
        {
            for (int i = 0; i < admins?.Count; i++)
            {
                if (admins[i].Id != myID)
                    admins[i].Close(); //отключение админа
            }
        }
    }
}
