using System;
using System.Collections;
using SaharokServer;
using System.Linq;
using SaharokServer.Server.Database;
using Microsoft.EntityFrameworkCore;
using ObjectsProjectServer;
using System.Threading;
using System.Configuration;

namespace SaharokServer
{
    class Program
    {
        static ManualResetEvent _event = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            try
            {
                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_user AS
                //                                SELECT u.ID AS ID,
                //                                u.HWID AS HWID,
                //                                u.NamePC AS NamePC,
                //                                u.FirstIP AS FirstIP,
                //                                u.LastIP AS LastIP,
                //                                u.FirstConnection AS FirstConnection,
                //                                u.LastConnection AS LastConnection,
                //                                u.Comment AS Comment,
                //                                u.IsOnlineServer1 AS IsOnlineServer1,
                //                                u.IsOnlineServer2 AS IsOnlineServer2,
                //                                u.AllowUsing AS AllowUsing
                //                                FROM user u
                //                                GROUP BY u.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_admin AS
                //                            SELECT a.ID AS ID,
                //                            a.Login AS Login,
                //                            a.Password AS Password,
                //                            a.LastIP AS LastIP,
                //                            a.LastSuccessfulLogin AS LastSuccessfulLogin,
                //                            a.LastNamePC AS LastNamePC,
                //                            a.LastHWID AS LastHWID,
                //                            a.IsOnlineServer1 AS IsOnlineServer1,
                //                            a.IsOnlineServer2 AS IsOnlineServer2
                //                            FROM admin a
                //                            GROUP BY a.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_session_user AS
                //                                SELECT s.ID AS SessionID,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                s.TimeOn AS TimeOn,
                //                                s.TimeOff AS TimeOff,
                //                                s.ConnectionTime AS ConnectionTime,
                //                                s.ServerNumber AS ServerNumber,
                //                                s.IsOnline AS IsOnline
                //                                FROM session_user s
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY s.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_session_admin AS
                //                                SELECT s.ID AS ID,
                //                                a.Login AS Login,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                s.TimeOn AS TimeOn,
                //                                s.TimeOff AS TimeOff,
                //                                s.ConnectionTime AS ConnectionTime,
                //                                s.ServerNumber AS ServerNumber,
                //                                s.IsOnline AS IsOnline,
                //                                s.IsSuccessfulLogin AS IIsSuccessfulLogin
                //                                FROM session_admin s
                //                                INNER JOIN admin a on a.ID = s.AdminID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY s.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_request_response AS
                //                                SELECT r.ID AS ID,
                //                                r.SessionID AS SessionID,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                r.NameProject AS NameProject,
                //                                r.CodeProject AS CodeProject,
                //                                r.PathProject AS PathProject,
                //                                r.Time AS Time,
                //                                r.ClientRequest AS ClientRequest,
                //                                r.ServerResponse AS ServerResponse,
                //                                s.ServerNumber AS ServerNumber
                //                                FROM request_response r
                //                                INNER JOIN session_user s on r.sessionID = s.ID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY r.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_error_user AS
                //                                SELECT e.ID AS ID,
                //                                e.SessionID AS SessionID,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                e.Time AS Time,
                //                                e.ErrorMessage AS ErrorMessage,
                //                                s.ServerNumber AS ServerNumber
                //                                FROM error_user e
                //                                INNER JOIN session_user s on e.sessionID = s.ID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY e.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_error_admin AS
                //                                SELECT e.ID AS ID,
                //                                e.SessionID AS SessionID,
                //                                a.Login AS Login,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                e.Time AS Time,
                //                                e.ErrorMessage AS ErrorMessage,
                //                                s.ServerNumber AS ServerNumber
                //                                FROM error_admin e
                //                                INNER JOIN session_admin s on e.sessionID = s.ID
                //                                INNER JOIN admin a on a.ID = s.AdminID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY e.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_error_response AS
                //                                SELECT e.ID AS ID,
                //                                e.RequestResponseID AS RequestResponseID,
                //                                r.SessionID AS SessionID,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                r.NameProject AS NameProject,
                //                                r.CodeProject AS CodeProject,
                //                                r.PathProject AS PathProject,
                //                                r.Time AS Time,
                //                                r.ClientRequest AS ClientRequest,
                //                                r.ServerResponse AS ServerResponse,
                //                                s.ServerNumber AS ServerNumber
                //                                FROM error_response e
                //                                INNER JOIN request_response r on e.RequestResponseID = r.ID
                //                                INNER JOIN session_user s on r.sessionID = s.ID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY e.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_error_server_object AS
                //                                SELECT e.ID AS ID,
                //                                e.Time AS RTime,
                //                                e.ErrorMessage AS ErrorMessage,
                //                                e.ServerNumber AS ServerNumber
                //                                FROM error_server_object e
                //                                GROUP BY e.ID");
                //    db.SaveChanges();
                //}

                //using (ApplicationContext db = new ApplicationContext())
                //{
                //    db.Database.ExecuteSqlRaw(@"CREATE OR REPLACE VIEW view_banned_response AS
                //                                SELECT e.ID AS ID,
                //                                e.RequestResponseID AS RequestResponseID,
                //                                r.SessionID AS SessionID,
                //                                u.NamePC AS NamePC,
                //                                u.NameUser AS NameUser,
                //                                r.NameProject AS NameProject,
                //                                r.CodeProject AS CodeProject,
                //                                r.PathProject AS PathProject,
                //                                r.Time AS Time,
                //                                r.ClientRequest AS ClientRequest,
                //                                r.ServerResponse AS ServerResponse
                //                                FROM banned_response e
                //                                INNER JOIN request_response r on e.RequestResponseID = r.ID
                //                                INNER JOIN session_user s on r.sessionID = s.ID
                //                                INNER JOIN user u on u.ID = s.UserID
                //                                GROUP BY e.ID");
                //    db.SaveChanges();
                //}
                var serversConfig = (ServersConfigSection)ConfigurationManager.GetSection("ProjectServers");
                foreach(ServerElement sc in serversConfig.Servers)
                {
                    ServerObject server = new ServerObject(sc.ServerNumber, sc.UserPort, sc.AdminPort, sc.AllowUsingNewClient, sc.DBname);
                    using (ApplicationContext db = new ApplicationContext(server.DBname)) // проверка на ошибочно не обновлённые данные о состоянии сессий БД
                    {
                        var sessionUserOnline = db.SessionUser
                            .Where(s => s.IsOnline == true && s.ServerNumber == server.ServerNumber)
                            .ForEachImmediate(s => s.IsOnline = false);

                        var sessionAdminOnline = db.SessionAdmin
                            .Where(s => s.IsOnline == true || s.IsSuccessfulLogin == true && s.ServerNumber == server.ServerNumber)
                            .ForEachImmediate(s =>
                            {
                                s.IsOnline = false;
                                s.IsSuccessfulLogin = false;
                            });
                        db.SessionUser.UpdateRange(sessionUserOnline);
                        db.SessionAdmin.UpdateRange(sessionAdminOnline);
                        db.SaveChanges();
                    }
                    server.ListenClientAsync();
                    server.ListenAdminAsync();
                }
                //ServerObject server = new ServerObject(Convert.ToInt32(ConfigurationManager.AppSettings["ServerNumber"]), Convert.ToInt32(ConfigurationManager.AppSettings["UserPort"]), Convert.ToInt32(ConfigurationManager.AppSettings["AdminPort"]), Convert.ToBoolean(ConfigurationManager.AppSettings["AllowUsingNewClient"]), (string)(ConfigurationManager.AppSettings["DBname"]));
                //using (ApplicationContext db = new ApplicationContext(server.DBname)) // проверка на ошибочно не обновлённые данные о состоянии сессий БД
                //{
                //    var sessionUserOnline = db.SessionUser
                //        .Where(s => s.IsOnline == true && s.ServerNumber == server.ServerNumber)
                //        .ForEachImmediate(s => s.IsOnline = false);

                //    var sessionAdminOnline = db.SessionAdmin
                //        .Where(s => s.IsOnline == true || s.IsSuccessfulLogin == true && s.ServerNumber == server.ServerNumber)
                //        .ForEachImmediate(s =>
                //        {
                //            s.IsOnline = false;
                //            s.IsSuccessfulLogin = false;
                //        });
                //    db.SessionUser.UpdateRange(sessionUserOnline);
                //    db.SessionAdmin.UpdateRange(sessionAdminOnline);
                //    db.SaveChanges();
                //}


                //server.ListenClientAsync();
                //server.ListenAdminAsync();
                _event.WaitOne();
            }
            catch (Exception ex)
            {
                Main(new string[0] { });
            }
        }
    }
}
