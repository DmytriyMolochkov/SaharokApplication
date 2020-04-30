using Kompas6API5;
using KAPITypes;
using KompasAPI7;
using Kompas6Constants;

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
        public static KompasObject CreateKompas()
        {
            KompasObject kompas = (KompasObject)CreateApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
        }

        public static KompasObject CreateKompas(bool invisiable)
        {
            if (invisiable)
            {
                KompasObject kompas = (KompasObject)CreateApplicationObject("KSINVISIBLE.Application.5");
                if (kompas != null) return kompas;
                throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
            }
            else
            {
                KompasObject kompas = (KompasObject)CreateApplicationObject("KOMPAS.Application.5");
                if (kompas != null) return kompas;
                throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
            }
        }

        //Получает экземпляр запущенного компаса
        public static KompasObject GetKompas()
        {
            KompasObject kompas = (KompasObject)GetApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема подключения к Kompas!");
        }

        private static object CreateApplicationObject(string progId)
        {
            try
            {
                object obj = Activator.CreateInstance(Type.GetTypeFromProgID(progId) /*Type.GetTypeFromCLSID(new Guid("FBE002A6-1E06-4703-AEC5-9AD8A10FA1FA"))*/);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        private static object GetApplicationObject(string progId)
        {
            object obj = null;
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