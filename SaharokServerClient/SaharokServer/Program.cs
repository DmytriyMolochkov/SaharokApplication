using System;
using SaharokServer;

namespace SaharokServer
{
    class Program
    {
        static ServerObject server = null; // сервер
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                //server.ListenAsync1();
                server.ListenAsync2();
                while (true)
                {
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Main(new string[0] { });
            }
        }
    }
}
