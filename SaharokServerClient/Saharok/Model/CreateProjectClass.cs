using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model
{
    public static class CreateProjectClass
    {
        private static void CreateFolder(string fullPath)
        {
            if (Directory.Exists(Path.GetDirectoryName(fullPath)))
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    throw new Exception("Папка уже существует");
                }
            }
            else
            {
                throw new Exception("Несуществующий каталог");
            }
        }

        private static void CreateFileProject(string path, string nameProject, string codeProject)
        {
            if (Directory.Exists(Path.GetDirectoryName(path)))
            {
                string fileName = Path.Combine(path, nameProject + ".srk");
                if (!File.Exists(fileName))
                {
                    using (StreamWriter stream = new StreamWriter(fileName, false, Encoding.Default))
                    {
                        stream.WriteLine(Path.GetFileName(path));
                        stream.WriteLine(codeProject);
                    }
                }
                else
                {
                    throw new Exception("Файл проекта уже существует");
                }
            }
            else
            {
                throw new Exception("Несуществующий каталог");
            }
        }

        public static void CreateProjectDirectory(string fullPath, string nameProject, string codeProject, FoldersConfigInfo foldersConfigInfo)
        {
            if (nameProject.Trim() == "")
                throw new Exception("Укажите имя проекта");
            if (Path.GetDirectoryName(fullPath).Trim() == "")
                throw new Exception("Укажите путь проекта");
            CreateFolder(fullPath);
            foldersConfigInfo.FolderPaths.ForEach(path => CreateFolder(Path.Combine(fullPath, path)));
            CreateFileProject(fullPath, nameProject, codeProject);
        }
    }
}
