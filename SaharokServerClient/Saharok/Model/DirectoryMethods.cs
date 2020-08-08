using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model
{
    public static class DirectoryMethods
    {
        public static void ClearFolder(string pathFolder)
        {
            try
            {
                if (Directory.Exists(pathFolder))
                {
                    Directory.Delete(pathFolder, true);
                    Directory.CreateDirectory(pathFolder);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
