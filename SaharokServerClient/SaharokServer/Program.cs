using System;
using SaharokServer.Server;

namespace SaharokServer
{
    class Program
    {
        static ServerObject server = null; // сервер
        static void Main(string[] args)
        {
            //try
            //{
                server = new ServerObject();
                server.ListenAsync();
                while (true)
                {
                    Console.ReadLine();
                }
            //}
            //catch (Exception ex)
            //{
            //    //Console.WriteLine(ex.Message);
            //    throw ex;
            //}
        }
    }
}
