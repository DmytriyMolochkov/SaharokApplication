using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model
{
    public static class CreateProjectClass
    {
        private static void CreateFolder(string pathFolder, string nameFolder)
        {
            if (Directory.Exists(pathFolder))
            {
                if (!Directory.Exists(System.IO.Path.Combine(pathFolder, nameFolder)))
                {
                    Directory.CreateDirectory(System.IO.Path.Combine(pathFolder, nameFolder));
                }
                else
                {
                    if (nameFolder.Trim() == string.Empty)
                    {
                        throw new Exception("Укажите имя папки");
                    }
                    else
                    {
                        throw new Exception("Папка уже существует");
                    }
                }
            }
            else
            {
                throw new Exception("Несуществующий каталог");
            }
        }

        private static void CreateFileProject(string pathProject, string nameProject, string codeProject)
        {
            if (Directory.Exists(pathProject))
            {
                string fileName = System.IO.Path.Combine(pathProject, nameProject + ".srk");
                if (!File.Exists(fileName))
                {
                    using (StreamWriter stream = new StreamWriter(fileName, false, Encoding.Default))
                    {
                        stream.WriteLine(nameProject);
                        stream.WriteLine(codeProject);
                    }
                }
                else
                {
                    if (nameProject.Trim() == string.Empty)
                    {
                        throw new Exception("Укажите имя проекта");
                    }
                    else
                    {
                        throw new Exception("Файл проекта уже существует");
                    }
                }
            }
            else
            {
                throw new Exception("Несуществующий каталог");
            }
        }

        public static void CreateProjectPath(string pathFolderProject, string nameProject, string codeProject)
        {
            try
            {
                CreateFolder(pathFolderProject, nameProject);
                string pathProject = System.IO.Path.Combine(pathFolderProject, nameProject);
                CreateFolder(pathProject, "PDF постранично");
                CreateFolder(pathProject, "Готовый проект");
                CreateFolder(pathProject, "Исходные данные");
                CreateFolder(pathProject, "Редактируемые файлы");
                CreateFolder(System.IO.Path.Combine(pathProject, "Готовый проект"), "На сервер");
                CreateFolder(System.IO.Path.Combine(pathProject, "Готовый проект"), "На отправку");
                CreateFileProject(pathProject, nameProject, codeProject);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
