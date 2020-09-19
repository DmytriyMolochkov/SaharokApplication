using Microsoft.EntityFrameworkCore;
using ObjectsProjectServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;

namespace SaharokServer.Server.Database
{
    public static class Logs
    {
        private static object _lock = new Object();
        public static User Connection(User user, SessionUser session, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    User existingUser = null;
                    foreach (var u in db.User)
                    {
                        if (u.HWID == user.HWID)
                        {
                            existingUser = u;
                            break;
                        }
                    }

                    if (existingUser != null)
                    {
                        existingUser.LastIP = user.LastIP;
                        existingUser.LastConnection = user.LastConnection;
                        if (server.ServerNumber % 2 > 0)
                            existingUser.IsOnlineServer1 = true;
                        else
                            existingUser.IsOnlineServer2 = true;

                        session.Start(existingUser, server.ServerNumber);
                        db.SessionUser.Add(session);
                        user = existingUser;
                    }
                    else
                    {
                        user.FirstIP = user.LastIP;
                        user.FirstConnection = user.LastConnection;
                        if (server.ServerNumber % 2 > 0)
                            user.IsOnlineServer1 = true;
                        else
                            user.IsOnlineServer2 = true;

                        session.Start(user, server.ServerNumber);
                        db.User.Add(user);
                        db.SessionUser.Add(session);
                    }
                    db.SaveChanges();
                    return user;
                }
            }
        }

        public static Admin Connection(Admin admin, SessionAdmin session, ServerObject server)
        {

            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    Admin existingAdmin = db.Admin.Find(1);
                    if (existingAdmin != null)
                    {
                        existingAdmin.NowHWID = admin.NowHWID;
                        existingAdmin.NowIP = admin.NowIP;
                        existingAdmin.NowNamePC = admin.NowNamePC;
                        admin = existingAdmin;
                    }
                    else
                    {
                        db.Admin.Add(admin);
                    }
                    User existingUser = null;
                    foreach (var u in db.User)
                    {
                        if (u.HWID == admin.NowHWID)
                        {
                            existingUser = u;
                            break;
                        }
                    }
                    if (existingUser == null)
                    {
                        User newUser = new User();
                        newUser.HWID = admin.NowHWID;
                        newUser.NamePC = admin.NowNamePC;
                        newUser.FirstIP = admin.NowIP;
                        newUser.FirstConnection = DateTime.Now;
                        newUser.AllowUsing = true;
                        db.User.Add(newUser);
                        existingUser = newUser;
                    }
                    session.Start(admin, existingUser, server.ServerNumber);
                    db.SessionAdmin.Add(session);
                    db.SaveChanges();
                    return admin;
                }
            }
        }

        public static void SuccessfulLogin(Admin admin, SessionAdmin session, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    session.Login(admin);
                    db.SessionAdmin.Update(session);
                    db.SaveChanges();
                }
            }
        }

        public static void Disconnection(SessionUser session, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    if (session != null)
                    {
                        int sessionID = session.ID;
                        int count = 0;
                        do
                        {
                            session = db.SessionUser.Find(sessionID);
                            count++;
                        } while (session == null && count < 100);
                        if (session == null)
                            return;
                        session.Disconnect();
                        db.SessionUser.Update(session);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void Disconnection(SessionAdmin session, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    if (session != null)
                    {
                        int sessionID = session.ID;
                        int count = 0;
                        do
                        {
                            session = db.SessionAdmin.Find(sessionID);
                            count++;
                        } while (session == null && count < 100);
                        if (session == null)
                            return;
                        session.Disconnect();
                        db.SessionAdmin.Update(session);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static SessionUser ClientRequest(SessionUser session, RequestResponse request, object clientData, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    session = db.SessionUser.Find(session.ID);
                    request.SessionUser = session;
                    request.Time = DateTime.Now;
                    request.ClientRequest = JsonConvert.SerializeObject(clientData, Formatting.Indented, new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All
                    });

                    if (clientData is IFilesToProjectContainer)
                    {
                        request.NameProject = ((IFilesToProjectContainer)clientData).GetNameProject();
                        request.CodeProject = ((IFilesToProjectContainer)clientData).GetCodeProject();
                        request.PathProject = ((IFilesToProjectContainer)clientData).GetPathProject();
                    }

                    db.RequestResponse.Add(request);
                    db.SaveChanges();
                    return session;
                }
            }
        }

        public static void ServerResponse(RequestResponse response, object serverData, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    response.ServerResponse = JsonConvert.SerializeObject(serverData, Formatting.Indented, new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All
                    });
                    db.RequestResponse.Update(response);
                    db.SaveChanges();
                }
            }
        }

        public static void ServerBannedResponse(RequestResponse response, string message, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    response.ServerResponse = message;
                    BannedResponse bannedResponse = new BannedResponse(response);
                    response.BannedResponse = bannedResponse;
                    db.Update(response);
                    db.SaveChanges();
                }
            }
        }

        public static void ServerErrorResponse(RequestResponse response, string message, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    response.ServerResponse = message;
                    ErrorResponse errorResponse = new ErrorResponse(response);
                    response.ErrorResponse = errorResponse;
                    db.Update(response);
                    db.SaveChanges();
                }
            }
        }

        public static void ErrorUser(SessionUser session, Exception exception, ServerObject server)
        {
            lock (_lock)
            {

                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    if (session != null)
                    {
                        int sessionID = session.ID;
                        int count = 0;
                        do
                        {
                            session = db.SessionUser.Find(sessionID);
                            count++;
                        } while (session == null && count < 100);
                        if (session == null)
                            return;
                        ErrorUser errorUser = new ErrorUser(session, exception);
                        db.ErrorUser.Add(errorUser);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void ErrorAdmin(SessionAdmin session, Exception exception, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    if (session != null)
                    {
                        int sessionID = session.ID;
                        int count = 0;
                        do
                        {
                            session = db.SessionAdmin.Find(sessionID);
                            count++;
                        } while (session == null && count < 100);
                        if (session == null)
                            return;
                        ErrorAdmin errorAdmin = new ErrorAdmin(session, exception);
                        db.ErrorAdmin.Add(errorAdmin);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void ErrorServerObject(Exception exception, ServerObject server)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext(server.DBname))
                {
                    ErrorServerObject errorServerObject = new ErrorServerObject(exception, server.ServerNumber);
                    db.ErrorServerObject.Add(errorServerObject);
                    db.SaveChanges();
                }
            }
        }

        public static void TryExecute(Action action)
        {
            int i = 0;
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {

                    if (++i == 20)
                        throw ex;
                    Thread.Sleep(200);
                }
            }
        }
    }
}