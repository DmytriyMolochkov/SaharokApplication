//using Kompas6API5;
//using KAPITypes;
//using KompasAPI7;
//using Kompas6Constants;

using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model
{
    public static class ConnectKompas
    {
        //Запускает Компас
        public static dynamic CreateKompas()
        {
            dynamic kompas = CreateApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
        }

        public static dynamic CreateKompas(bool invisiable)
        {
            if (invisiable)
            {
                dynamic kompas = CreateApplicationObject("KSINVISIBLE.Application.5");
                if (kompas != null) return kompas;
                throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
            }
            else
            {
                dynamic kompas = CreateApplicationObject("KOMPAS.Application.5");
                if (kompas != null) return kompas;
                throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
            }
        }

        //Получает экземпляр запущенного компаса
        public static dynamic GetKompas()
        {
            dynamic kompas = GetApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема подключения к Kompas!");
        }

        private static dynamic CreateApplicationObject(string progId)
        {
            try
            {
                dynamic obj = Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                return obj;
            }
            catch
            {
                return null;
            }
        }

        private static dynamic GetApplicationObject(string progId)
        {
            dynamic obj = null;
            try
            {
                obj = Marshal.GetActiveObject(progId);
                return obj;
            }
            catch
            {
                obj = CreateApplicationObject(progId);
                return obj;
            }
        }
    }
}