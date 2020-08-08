using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
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
        private ServerObject Server { get; set; } // объект сервера

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
            SessionUser session = null; // для логов
            try
            {
                Nstream = TcpClient.GetStream();
                user = new User(TcpClient, Nstream);
                Logs.Connection(ref user, ref session);

                while (true)
                {
                    object data = null;
                    MyAes myAes = new MyAes();
                    RequestResponse requestResponse = null;

                    if (Nstream != null)
                        Nstream.Flush();

                    myAes.GenerateParameters(); // генерируем симметричный ключ для общения с юзером
                    myAes.ExportParameters(Nstream);
                    data = myAes.DecryptFromStream(Nstream); // ожидаем пока придёт проект для обработки
                    Logs.ClientRequest(ref session, ref requestResponse, data);

                    if (!CheckUser(ref user)) // проверяем бан юзера
                    {
                        string message = "Мы ходим вокруг опунции, опунции в пять часов утра.";
                        myAes.EncryptToStream(message, Nstream);
                        Logs.ServerBannedResponse(ref requestResponse, message);
                        continue;
                    }

                    FilesToPDFSort filesToPDFSort = null;
                    // отправляем ответ юзеру
                    if (data is IFilesToProjectContainer)
                    {
                        filesToPDFSort = ((IFilesToProjectContainer)data).GetFilesToPDFSort();
                        myAes.EncryptToStream(InfoOfProcess.GetInstance(), Nstream);
                        myAes.EncryptToStream(filesToPDFSort, Nstream);
                        Logs.ServerResponse(ref requestResponse, filesToPDFSort);
                    }
                    else
                    {
                        string message = $"Полученный сервером объект не поддерживает интерфейс: {typeof(IFilesToProjectContainer).Name}.";
                        myAes.EncryptToStream(message, Nstream);
                        Logs.ServerErrorResponse(ref requestResponse, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (!(ex is IOException))
                {
                    Logs.ErrorUser(ref session, ex);
                }
                Logs.Disconnection(ref user, ref session);
                Server.RemoveConnectionClient(this.Id);
                Close();
            }
        }

        public void ProcessAdmin()
        {
            Admin admin = null; // для логов
            SessionAdmin session = null; // для логов
            try
            {
                Nstream = TcpClient.GetStream();
                MyAes myAes = new MyAes();
                admin = new Admin(TcpClient, Nstream, myAes);
                myAes.EncryptToStream(ServerObject.ServerNumber, Nstream);
                Logs.Connection(ref admin, ref session);
                ListenerCommand listenerCommand = new ListenerCommand(this);
                listenerCommand.Start(ref admin, ref session, myAes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (!(ex is IOException))
                {
                    Logs.ErrorAdmin(ref session, ex);
                }
                Logs.Disconnection(ref admin, ref session);
                Server.RemoveConnectionAdmin(this.Id);
                Close();
            }
        }

        public void LogingAdmin(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (!session.IsSuccessfulLogin)
            {
                if (admin.Login == args[0] && admin.Password == args[1] && admin.Login != null && admin.Password != null)
                {
                    myAes.EncryptToStream(new string[] { true.ToString(),
                        $"Вход на сервер №{ServerObject.ServerNumber} выполнен успешно.{Environment.NewLine}" +
                        $"Последний вход был произведён: {admin.LastSuccessfulLogin.ToString()} со следующими данными:{Environment.NewLine}" +
                        $"IP: {admin.LastIP}{Environment.NewLine}" +
                        $"Name PC: {admin.LastNamePC}{Environment.NewLine}" +
                        $"HWID: {admin.LastHWID}"
                    }, Nstream);
                    Logs.SuccessfulLogin(ref admin, ref session);
                }
                else myAes.EncryptToStream(new string[] { false.ToString(), $"Неудачный вход на сервер {ServerObject.ServerNumber}. Неверный логин или пароль." }, Nstream);

            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы уже авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        public void DisconnectClients(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int UsersCount = Server.clients.Count();
                Server.DisconnectClients();
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Клиенты отключены на сервере №{ServerObject.ServerNumber}.{Environment.NewLine}" +
                    $"Кол-во отключённых клиентов: {UsersCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        public void ListenClient(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                Server.ListenClientAsync();
                myAes.EncryptToStream(new string[] { true.ToString(), $"Началось прослушивание клиентов сервере №{ServerObject.ServerNumber}." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        public void DisconnectAdmins(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int AdminsCount = Server.admins.Count() - 1;
                Server.DisconnectAdmins(Id);
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Администраторы отключены на сервере №{ServerObject.ServerNumber}, кроме вас.{Environment.NewLine}" +
                    $"Кол-во отключённых администраторов: {AdminsCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        public void ClientsCount(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int ClientsCount = Server.clients.Count();
                myAes.EncryptToStream(new string[] { true.ToString(), $"Кол-во клиентов на сервере №{ServerObject.ServerNumber}: {ClientsCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        public void AdminsCount(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            if (session.IsSuccessfulLogin)
            {
                int AdminsCount = Server.admins.Count();
                int AdminsLoginCount;
                using (ApplicationContext db = new ApplicationContext())
                {
                    AdminsLoginCount = db.SessionAdmin.Where(s => s.IsSuccessfulLogin && s.IsOnline && s.ServerNumber == ServerObject.ServerNumber).Count();
                }
                myAes.EncryptToStream(new string[] { true.ToString(),
                    $"Кол-во администраторов на сервере №{ServerObject.ServerNumber}: {AdminsCount}шт.{Environment.NewLine}" +
                    $"Кол-во авторизованных администраторов: {AdminsLoginCount}шт." }, Nstream);
            }
            else myAes.EncryptToStream(new string[] { false.ToString(), $"Вы не авторизованы на сервере №{ServerObject.ServerNumber}." }, Nstream);
        }

        //private bool CheckError(Exception ex)
        //{
        //    switch (ex.Message)
        //    {
        //        case ("Unable to read data from the transport connection: Удаленный хост принудительно разорвал существующее подключение.."):
        //            {
        //                return false;
        //            }
        //        case ("Unable to write data to the transport connection: Удаленный хост принудительно разорвал существующее подключение.."):
        //            {
        //                return false;
        //            }
        //        case ("Unable to write data to the transport connection: Программа на вашем хост-компьютере разорвала установленное подключение.."):
        //            {
        //                return false;
        //            }
        //        case ("Unable to read data from the transport connection: Операция блокирования прервана вызовом WSACancelBlockingCall.."):
        //            {
        //                return false;
        //            }
        //        case ("End of Stream encountered before parsing was completed."):
        //            {
        //                return false;
        //            }
        //        case ("Stream was not writable."):
        //            {
        //                return false;
        //            }
        //        //case ("An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure()' to the 'UseMySql' call."):
        //        //    {
        //        //        return false;
        //        //    }
        //        default:
        //            {
        //                return true;
        //            }
        //    }
        //}

        private bool CheckUser(ref User user)
        {
            using (ApplicationContext db = new ApplicationContext()) // обновляем состояние юзера из базы
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
        public abstract void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args);
    }

    class LgCommand : CommandBase
    {
        public LgCommand(ClientObject clientObject) : base(clientObject)
        { }

        public override bool TryParse(string[] args)
        {
            return args[0].ToLower() == "lg";
        }
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.LogingAdmin(ref admin, ref session, myAes, args.Skip(1).ToArray());
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
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.DisconnectClients(ref admin, ref session, myAes, args.Skip(1).ToArray());
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
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.ListenClient(ref admin, ref session, myAes, args.Skip(1).ToArray());
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
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.DisconnectAdmins(ref admin, ref session, myAes, args.Skip(1).ToArray());
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
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.ClientsCount(ref admin, ref session, myAes, args.Skip(1).ToArray());
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
        public override void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            clientObject.AdminsCount(ref admin, ref session, myAes, args.Skip(1).ToArray());
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

        public void Start(ref Admin admin, ref SessionAdmin session, MyAes myAes)
        {
            while (true)
            {
                try
                {

                    Execute(ref admin, ref session, myAes, (string[])myAes.DecryptFromStream(clientObject.Nstream));

                }
                catch (Exception ex)
                {
                    myAes.EncryptToStream(new string[] { false.ToString(), ex.Message }, clientObject.Nstream);
                    Logs.ErrorAdmin(ref session, ex);
                }
            }
        }
        void Execute(ref Admin admin, ref SessionAdmin session, MyAes myAes, string[] args)
        {
            foreach (var com in commands)
                if (com.TryParse(args))
                {
                    using (ApplicationContext db = new ApplicationContext()) // достаём состояние админа из базы
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
                        int adminID = admin.ID;
                        i = 0;
                        do
                        {
                            admin = db.Admin.Find(adminID);
                            i++;
                        } while (admin == null && i < 100);
                        if (admin == null)
                            return;
                        com.Execute(ref admin, ref session, myAes, args);
                        return;
                    }
                }
            throw new Exception("Syntax error: unknown command");
        }
    }
}