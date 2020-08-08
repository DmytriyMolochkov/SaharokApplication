using Microsoft.EntityFrameworkCore;
using ObjectsProjectServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace SaharokServer.Server.Database
{
    public static class Logs
    {
        private static object _lock = new Object();
        public static void Connection(ref User user, ref SessionUser session)
        {

            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
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
                        if (ServerObject.ServerNumber == 1)
                        {
                            existingUser.IsOnlineServer1 = true;
                        }
                        else if (ServerObject.ServerNumber == 2)
                        {
                            existingUser.IsOnlineServer2 = true;
                        }
                        session = new SessionUser(existingUser);
                        db.SessionUser.Add(session);
                        user = existingUser;
                    }
                    else
                    {
                        user.FirstIP = user.LastIP;
                        user.FirstConnection = user.LastConnection;
                        if (ServerObject.ServerNumber == 1)
                        {
                            user.IsOnlineServer1 = true;
                        }
                        else if (ServerObject.ServerNumber == 2)
                        {
                            user.IsOnlineServer2 = true;
                        }
                        session = new SessionUser(user);
                        db.User.Add(user);
                        db.SessionUser.Add(session);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void Connection(ref Admin admin, ref SessionAdmin session)
        {

            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Admin existingAdmin = db.Admin.Find(1);
                    if(existingAdmin != null)
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
                    session = new SessionAdmin(admin, existingUser);
                    db.SessionAdmin.Add(session);
                    db.SaveChanges();
                }
            }
        }

        public static void SuccessfulLogin(ref Admin admin, ref SessionAdmin session)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                session.Login(ref admin);
                db.SessionAdmin.Update(session);
                //db.Admin.Update(admin);
                db.SaveChanges();
            }
        }

        public static void Disconnection(ref User user, ref SessionUser session)
        {
            lock (_lock)
            {

                using (ApplicationContext db = new ApplicationContext())
                {
                    if (user != null && session != null)
                    {
                        user = db.User.Find(user.ID);
                        int sessionID = session.ID;
                        int i = 0;
                        do
                        {
                            session = db.SessionUser.Find(sessionID);
                            i++;
                        } while (session == null && i < 100);
                        if (session == null)
                            return;
                        session.Disconnect(ref user);
                        db.SessionUser.Update(session);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void Disconnection(ref Admin admin, ref SessionAdmin session)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    if (admin != null && session != null)
                    {
                        admin = db.Admin.Find(admin.ID);
                        int sessionID = session.ID;
                        int i = 0;
                        do
                        {
                            session = db.SessionAdmin.Find(sessionID);
                            i++;
                        } while (session == null && i < 100);
                            if (session == null)
                            return;
                        session.Disconnect(ref admin);
                        db.SessionAdmin.Update(session);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void ClientRequest(ref SessionUser session, ref RequestResponse request, object clientData)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    session = db.SessionUser.Find(session.ID);
                    request = new RequestResponse(session);
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
                }
            }
        }

        public static void ServerResponse(ref RequestResponse response, object serverData)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
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

        public static void ServerBannedResponse(ref RequestResponse response, string message)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    response.ServerResponse = message;
                    BannedResponse bannedResponse = new BannedResponse(response);
                    response.BannedResponse = bannedResponse;
                    db.Update(response);
                    db.SaveChanges();
                }
            }
        }

        public static void ServerErrorResponse(ref RequestResponse response, string message)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    response.ServerResponse = message;
                    ErrorResponse errorResponse = new ErrorResponse(response);
                    response.ErrorResponse = errorResponse;
                    db.Update(response);
                    db.SaveChanges();
                }
            }
        }

        public static void ErrorUser(ref SessionUser session, Exception ex)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    if (session != null)
                    {
                        int sessionID = session.ID;
                        int i = 0;
                        do
                        {
                            session = db.SessionUser.Find(sessionID);
                            i++;
                        } while (session == null && i < 100);
                            if (session == null)
                            return;
                        ErrorUser errorUser = new ErrorUser(session, ex);
                        db.ErrorUser.Add(errorUser);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void ErrorAdmin(ref SessionAdmin session, Exception ex)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    if (session != null)
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
                        ErrorAdmin errorAdmin = new ErrorAdmin(session, ex);
                        db.ErrorAdmin.Add(errorAdmin);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void ErrorServerObject(Exception ex)
        {
            lock (_lock)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    ErrorServerObject errorServerObject = new ErrorServerObject(ex);
                    db.ErrorServerObject.Add(errorServerObject);
                    db.SaveChanges();
                }
            }
        }
    }
}