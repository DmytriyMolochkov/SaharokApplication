using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using ObjectsProjectServer;
using SaharokServer.Encryption;
using SaharokServer.Server.Database;

namespace SaharokServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Nstream { get; private set; }

        public TcpClient TcpClient { get; set; }
        public ServerObject Server { get; set; } // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject, bool isAdmin = false)
        {
            Id = Guid.NewGuid().ToString();
            TcpClient = tcpClient;
            Server = serverObject;
            if (isAdmin)
            {
                serverObject.AddConnectionAdmin(this);
            }
            else
            {
                serverObject.AddConnectionClient(this);
            }
        }

        public void ProcessUser()
        {
            User user = null; // для логов
            SessionUser session = new SessionUser(); // для логов
            try
            {
                Nstream = TcpClient.GetStream();
                user = new User(TcpClient, Nstream, Server.AllowUsingNewClient);
                Logs.TryExecute(() => user = Logs.Connection(user, session, Server));

                while (true)
                {
                    object data = null;
                    MyAes myAes = new MyAes();

                    if (Nstream != null)
                        Nstream.Flush();

                    myAes.GenerateParameters(); // генерируем симметричный ключ для общения с юзером
                    myAes.ExportParameters(Nstream);
                    data = myAes.DecryptFromStream(Nstream); // ожидаем пока придёт проект для обработки
                    RequestResponse requestResponse = new RequestResponse();
                    Logs.TryExecute(() => session = Logs.ClientRequest(session, requestResponse, data, Server));

                    if (!CheckUser(user)) // проверяем бан юзера
                    {
                        string message = "Отказано в доступе. Пожалуйста, свяжитесь с разработчиком по почте saharok.application@gmail.com если это произошло по ошибке или вы заинтересованы в продукте.";
                        myAes.EncryptToStream(message, Nstream);
                        Logs.TryExecute(() => Logs.ServerBannedResponse(requestResponse, message, Server));
                        continue;
                    }

                    FilesToPDFSort filesToPDFSort = null;
                    // отправляем ответ юзеру
                    if (data is IFilesToProjectContainer)
                    {
                        filesToPDFSort = ((IFilesToProjectContainer)data).GetFilesToPDFSort();
                        myAes.EncryptToStream(InfoOfProcess.GetInstance(), Nstream);
                        myAes.EncryptToStream(filesToPDFSort, Nstream);
                        Logs.TryExecute(() => Logs.ServerResponse(requestResponse, filesToPDFSort, Server));
                    }
                    else
                    {
                        string message = $"Полученный сервером объект не поддерживает интерфейс: {typeof(IFilesToProjectContainer).Name}.";
                        myAes.EncryptToStream(message, Nstream);
                        Logs.TryExecute(() => Logs.ServerErrorResponse(requestResponse, message, Server));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (!(ex is IOException || ex is SerializationException))
                {
                    Logs.TryExecute(() => Logs.ErrorUser(session, ex, Server));
                }
                Logs.TryExecute(() => Logs.Disconnection(session, Server));
                Server.RemoveConnectionClient(this.Id);
                Close();
            }
        }

        public void ProcessAdmin()
        {
            Admin admin = null; // для логов
            SessionAdmin session = new SessionAdmin(); // для логов
            try
            {
                Nstream = TcpClient.GetStream();
                MyAes myAes = new MyAes();
                admin = new Admin(TcpClient, Nstream, myAes);
                myAes.EncryptToStream(Server.ServerNumber, Nstream);
                Logs.TryExecute(() => admin = Logs.Connection(admin, session, Server));
                ListenerCommand listenerCommand = new ListenerCommand(this);
                listenerCommand.Start(session, myAes, Server);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (!(ex is IOException || ex is SerializationException))
                {
                    Logs.TryExecute(() => Logs.ErrorAdmin(session, ex, Server));
                }
                Logs.TryExecute(() => Logs.Disconnection(session, Server));
                Server.RemoveConnectionAdmin(this.Id);
                Close();
            }
        }

        public void LogingAdmin(Admin admin,SessionAdmin session, MyAes myAes, string[] args)
        {
            if (!session.IsSuccessfulLogin)
            {
                if (admin.Login == args[0] && admin.Password == args[1] && admin.Login != null && admin.Password != null)
                {
                    myAes.EncryptToStream(new string[] { true.ToString(),
                        $"Вход на сервер №{Server.ServerNumber} выполнен успешно.{Environment.NewLine}" +
                        $"Последний вход был произведён: {admin.LastSuccessfulLogin.ToString()} со следующими данными:{Environment.NewLine}" +
                        $"IP: {admin.LastIP}{Environment.NewLine}" +
                        $"Name PC: {admin.LastNamePC}{Environment.NewLine}" +
                        $"HWID: {admin.LastHWID}"
                    }, Nstream);
                    Logs.TryExecute(() => Logs.SuccessfulLogin(admin, session, Server));
                }
                else myAes.EncryptToStream(new string[] { false.ToString(), $"Неудачный вход на сервер {Server.ServerNumber}. Неверный логин или пароль." }, Nstream);

            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы уже авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        public void DisconnectClients(SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int UsersCount = Server.clients.Count();
                Server.DisconnectClients();
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Клиенты отключены на сервере №{Server.ServerNumber}.{Environment.NewLine}" +
                    $"Кол-во отключённых клиентов: {UsersCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        public void ListenClient(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                if(!(bool)typeof(TcpListener).GetProperty("Active", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Server.tcpListenerClient))
                {
                    Server.ListenClientAsync();
                    myAes.EncryptToStream(new string[] { true.ToString(), $"Началось прослушивание клиентов сервере №{Server.ServerNumber}." }, Nstream);
                }
                else myAes.EncryptToStream(new string[] { false.ToString(), $"Клиенты на сервере №{Server.ServerNumber} уже прослушиваются." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        public void DisconnectAdmins(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int AdminsCount = Server.admins.Count() - 1;
                Server.DisconnectAdmins(Id);
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Администраторы отключены на сервере №{Server.ServerNumber}, кроме вас.{Environment.NewLine}" +
                    $"Кол-во отключённых администраторов: {AdminsCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        public void ClientsCount(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int ClientsCount = Server.clients.Count();
                myAes.EncryptToStream(new string[] { true.ToString(), $"Кол-во клиентов на сервере №{Server.ServerNumber}: {ClientsCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        public void AdminsCount(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int AdminsCount = Server.admins.Count();
                int AdminsLoginCount;
                using (ApplicationContext db = new ApplicationContext(Server.DBname))
                {
                    AdminsLoginCount = db.SessionAdmin.Where(s => s.IsSuccessfulLogin && s.IsOnline && s.ServerNumber == Server.ServerNumber).Count();
                }
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Кол-во администраторов на сервере №{Server.ServerNumber}: {AdminsCount}шт.{Environment.NewLine}" +
                    $"Кол-во авторизованных администраторов: {AdminsLoginCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{Server.ServerNumber}." }, Nstream);
        }

        private bool CheckUser(User user)
        {
            using (ApplicationContext db = new ApplicationContext(Server.DBname)) // обновляем состояние юзера из базы
            {
                user = db.User.Find(user.ID);
            }
            return user.AllowUsing;
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Nstream != null)
                Nstream.Close();
            if (TcpClient != null)
                TcpClient.Close();
        }
    }

    abstract class CommandBase
    {
        public ClientObject clientObject;
        public CommandBase(ClientObject clientObject)
        {
            this.clientObject = clientObject;
        }
        public abstract bool TryParse(string[] args);
        public abstract void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args);
    }

    class LgCommand : CommandBase
    {
        public LgCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "lg";
        }
        public override void Execute(Admin admin,SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.LogingAdmin(admin, session, myAes, args.Skip(1).ToArray());
        }
    }

    class DcCommand : CommandBase
    {
        public DcCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "dc";
        }
        public override void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.DisconnectClients(session, myAes, args.Skip(1).ToArray());
        }
    }

    class LcCommand : CommandBase
    {
        public LcCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "lc";
        }
        public override void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.ListenClient(admin, session, myAes, args.Skip(1).ToArray());
        }
    }

    class DaCommand : CommandBase
    {
        public DaCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "da";
        }
        public override void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.DisconnectAdmins(admin, session, myAes, args.Skip(1).ToArray());
        }
    }

    class ccCommand : CommandBase
    {
        public ccCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "cc";
        }
        public override void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.ClientsCount(admin, session, myAes, args.Skip(1).ToArray());
        }
    }

    class acCommand : CommandBase
    {
        public acCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "ac";
        }
        public override void Execute(Admin admin, SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.AdminsCount(admin, session, myAes, args.Skip(1).ToArray());
        }
    }

    class ListenerCommand
    {
        public ClientObject clientObject;
        private List<CommandBase> commands;
        public ListenerCommand(ClientObject clientObject)
        {
            this.clientObject = clientObject;
            commands = new List<CommandBase>
            {
                new LgCommand(clientObject),
                new DcCommand(clientObject),
                new LcCommand(clientObject),
                new DaCommand(clientObject),
                new ccCommand(clientObject),
                new acCommand(clientObject),
            };
        }

        public void Start(SessionAdmin session, MyAes myAes, ServerObject server)
        {
            while (true)
            {
                try
                {
                    Execute(session, myAes, (string[])myAes.DecryptFromStream(clientObject.Nstream));
                }
                catch (Exception ex)
                {
                    myAes.EncryptToStream(new string[] { false.ToString(), ex.Message }, clientObject.Nstream);
                    Logs.TryExecute(() => Logs.ErrorAdmin(session, ex, server));
                }
            }
        }
        void Execute(SessionAdmin session, MyAes myAes, string[] args)
        {
            foreach (var com in commands)
                if (com.TryParse(args))
                {
                    using (ApplicationContext db = new ApplicationContext(clientObject.Server.DBname)) // достаём состояние админа из базы
                    {
                        int sessionID = session.ID;
                        int i = 0;
                        do
                        {
                            session = db.SessionAdmin.Find(sessionID);
                            i++;
                        } while (session == null && i < 100);
                        if (session == null)
                            return;
                        Admin admin = session.Admin;
                        com.Execute(admin, session, myAes, args);
                        return;
                    }
                }
            throw new Exception("Syntax error: unknown command");
        }
    }
}