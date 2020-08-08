using SaharokAdmin.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.Threading;

namespace SaharokAdmin
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ClientObject ClientObject1 = new ClientObject("127.0.0.1", 8909, 1); /*109.68.215.3*/
                ClientObject ClientObject2 = new ClientObject("127.0.0.1", 8908, 2); /*5.23.54.220*/
                ClientObject1.Connect(true);
                ClientObject2.Connect(true);

                Thread.Sleep(4000);
                Connector connector = new Connector(new ClientObject[] { ClientObject1, ClientObject2 });
                Console.WriteLine("Войдите на сервера: ");
                var lgOptions = new LgOptions();
                lgOptions.servers = new List<int>();
                connector.Loging(lgOptions);

                while (true)
                {
                    Parser.Default.ParseArguments<LgOptions, DcOptions, LcOptions, DaOptions, CcOptions, AcOptions, VsOptions, VcsOptions, ChdsOptions, VdsOptions>(Console.ReadLine().Split(' '))
                        .MapResult(
                     (LgOptions options) => connector.Loging(options),
                     (DcOptions options) => connector.DisconnectionClients(options),
                     (LcOptions options) => connector.ListenClients(options),
                     (DaOptions options) => connector.DisconnectionAdmins(options),
                     (CcOptions options) => connector.ClientsCount(options),
                     (AcOptions options) => connector.AdminsCount(options),
                     (VsOptions options) => connector.ViewAllServers(options),
                     (VcsOptions options) => connector.ViewAllConnectedServers(options),
                     (ChdsOptions options) => connector.ChangeDefaultServers(options),
                     (VdsOptions options) => connector.ViewDefaultServers(options),
                     errs => 1);
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.Handling(ex);
                Main(new string[] { });
            }
        }
    }

    public class Connector
    {
        private List<ClientObject> Servers { get; set; }
        private List<int> ServersDefault { get; set; } = new List<int>();
        private List<int> ServersToExecute { get; set; } = new List<int>();

        public Connector(ClientObject[] servers)
        {
            Servers = servers.ToList();
        }

        public int Loging(LgOptions options)
        {
            if (options.login == null)
            {
                Console.Write("Логин: ");
                options.login = Console.ReadLine();
            }
            if (options.password == null)
            {
                Console.Write("Пароль: ");
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        options.password += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && options.password.Length > 0)
                        {
                            options.password = options.password.Substring(0, (options.password.Length - 1));
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            foreach (var s in options.password)
                                Console.Write("\b \b");
                            Console.WriteLine();
                            break;
                        }
                    }
                } while (true);
            }
            return ExecudeCommand(new LgCommand(options));
        }

        public int DisconnectionClients(DcOptions options)
        {
            return ExecudeCommand(new DcCommand(options));
        }

        public int ListenClients(LcOptions options)
        {
            return ExecudeCommand(new LcCommand(options));
        }

        public int DisconnectionAdmins(DaOptions options)
        {
            return ExecudeCommand(new DaCommand(options));
        }

        public int ClientsCount(CcOptions options)
        {
            return ExecudeCommand(new CcCommand(options));
        }

        public int AdminsCount(AcOptions options)
        {
            return ExecudeCommand(new AcCommand(options));
        }

        public int ViewAllServers(VsOptions options)
        {
            try
            {
                Console.WriteLine($"Все сервера: {String.Join(", ", Servers.Select(s => s.ServerNumber))}");
                return 0;
            }
            catch (Exception ex)
            {
                ErrorHandling.Handling(ex);
                return 1;
            }
        }

        public int ViewAllConnectedServers(VcsOptions options)
        {
            try
            {
                Console.WriteLine($"Все подключённые сервера: {String.Join(", ", Servers.Where(s => s.IsServerConnect).Select(s => s.ServerNumber))}");
                return 0;
            }
            catch (Exception ex)
            {
                ErrorHandling.Handling(ex);
                return 1;
            }
        }

        public int ChangeDefaultServers(ChdsOptions options)
        {
            try
            {
                if (options.servers.Count() == 0)
                {
                    ServersToExecute = Servers.Select(s => s.ServerNumber).ToList();
                    ServersDefault = ServersToExecute;
                }
                else
                {
                    CheckServersNumbers(options.servers);
                    ServersDefault = options.servers.ToList();
                }
                Console.WriteLine($"Сервера по умолнчанию были изменены на: {String.Join(", ", ServersDefault)}");

                return 0;
            }
            catch (Exception ex)
            {
                ErrorHandling.Handling(ex);
                return 1;
            }
        }

        public int ViewDefaultServers(VdsOptions options)
        {
            try
            {
                if (ServersDefault.Count() == 0)
                {
                    ServersToExecute = Servers.Select(s => s.ServerNumber).ToList();
                    Console.WriteLine($"Сервера по умолчанию: {String.Join(", ", ServersToExecute)}");
                }
                else
                    Console.WriteLine($"Сервера по умолчанию: {String.Join(", ", ServersDefault)}");
                return 0;
            }
            catch (Exception ex)
            {
                ErrorHandling.Handling(ex);
                return 1;
            }
        }

        private int ExecudeCommand(CommandBase command)
        {
            if (command.options.servers.Count() == 0)
            {
                if (ServersDefault.Count() == 0)
                {
                    ServersToExecute = Servers.Select(s => s.ServerNumber).ToList();
                    command.options.servers = ServersToExecute;
                }
                else
                    command.options.servers = ServersDefault;
            }
            else CheckServersNumbers(command.options.servers);

            List<Task> tasks = new List<Task>();
            List<string> response = new List<string>();

            Servers
                .Where(s => command.options.servers.Contains(s.ServerNumber))
                .ForEachImmediate(server =>
                {
                    tasks.Add(Task.Run(() =>
                    {
                        if (server.client.Connected)
                        {
                            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                            CancellationToken token = cancelTokenSource.Token;
                            try
                            {
                                server.CheckStream(token); // асинхронная проверка есть ли передача данных в течении 5 секунд, иначе exception
                                response.Add(command.Execute(server));
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message == "Операция была отменена.")
                                    throw new ServerException($"Превышено вермя ожидания сервера №{server.ServerNumber}.");
                                else
                                    throw new ServerException(ex.Message);
                            }
                            finally
                            {
                                cancelTokenSource.Cancel(); // прекращение проверки
                            }
                        }
                        else
                            response.Add($"Соединение с сервером №{server.ServerNumber} потеряно.");
                    }));
                });
            try
            {
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine(String.Join(Environment.NewLine, response));
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Join(Environment.NewLine, response));
                ErrorHandling.Handling(ex);
                return 1;
            }
        }

        private void CheckServersNumbers(IEnumerable<int> servers)
        {
            var errorNumersServer = servers
                   .Where(serverNumber =>
                       !Servers
                           .Select(s => s.ServerNumber)
                           .Contains(serverNumber)
                           );
            if (errorNumersServer.Count() > 0)
                throw new Exception($"Неизвестные номера серверов: {String.Join(", ", errorNumersServer)}");
        }
    }

    abstract class CommandBase
    {
        public Options options;
        public CommandBase(Options options)
        {
            this.options = options;
        }
        public abstract string Execute(ClientObject clientObject);

    }

    class LgCommand : CommandBase
    {
        public LgCommand(LgOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.Login(((LgOptions)options).login, ((LgOptions)options).password);
        }
    }

    class DcCommand : CommandBase
    {
        public DcCommand(DcOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.DisconnectionClients();
        }
    }

    class LcCommand : CommandBase
    {
        public LcCommand(LcOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.ListenClient();
        }
    }

    class DaCommand : CommandBase
    {
        public DaCommand(DaOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.DisconnectionAdmins();
        }
    }

    class CcCommand : CommandBase
    {
        public CcCommand(CcOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.ClientsCount();
        }
    }

    class AcCommand : CommandBase
    {
        public AcCommand(AcOptions options) : base(options)
        { }

        public override string Execute(ClientObject clientObject)
        {
            return clientObject.AdminsCount();
        }
    }

    public class Options
    {
        [Option('s', "servers", Required = false, HelpText = "Server numbers for command execution.")]
        public IEnumerable<int> servers { get; set; }
    }

    [Verb("lg", HelpText = "Login to the server.")]
    public class LgOptions : Options
    {
        [Option('l', "login", Required = false, HelpText = "Login for login to the server.")]
        public string login { get; set; }

        [Option('p', "password", Required = false, HelpText = "Password for login to the server.")]
        public string password { get; set; }
    }

    [Verb("dc", HelpText = "Disconnection clients.")]
    public class DcOptions : Options
    { }

    [Verb("lc", HelpText = "Start listen clients.")]
    public class LcOptions : Options
    { }

    [Verb("da", HelpText = "Disconnection admins other than you.")]
    public class DaOptions : Options
    { }

    [Verb("cc", HelpText = "Clients count.")]
    public class CcOptions : Options
    { }

    [Verb("ac", HelpText = "Admins count.")]
    public class AcOptions : Options
    { }

    [Verb("vs", HelpText = "View all servers.")]
    public class VsOptions
    { }

    [Verb("vcs", HelpText = "View all connected servers.")]
    public class VcsOptions
    { }

    [Verb("chds", HelpText = "Change default servers.")]
    public class ChdsOptions : Options
    { }

    [Verb("vds", HelpText = "View default servers.")]
    public class VdsOptions
    { }


}
