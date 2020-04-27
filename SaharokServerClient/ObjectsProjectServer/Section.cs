using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsProjectServer
{
    public class Section
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public TypeDocumentation TypeDocumentation { get; set; }
        public ObservableCollection<FileSection> Files { get; set; }

        //public Section(string path, string name, TypeDocumentation parent)
        //{
        //    Path = path;
        //    Name = name;
        //    TypeDocumentation = parent;
        //    LoadFilesWithRetries();
        //}

        

        //protected async void LoadFilesWithRetries()
        //{
        //    int @try = 0;
        //    do
        //    {
        //        try
        //        {
        //            LoadFiles();
        //            return;
        //        }
        //        catch (System.IO.IOException e)
        //        {

        //            if (@try++ < 10)
        //            {
        //                if (@try > 1)
        //                    await Task.Delay(500);
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        }
        //    } while (true);
        //}

        //protected void LoadFiles()
        //{
        //    Files = new ObservableCollection<FileSection>(Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly)
        //        .Where(line => FileFilter(line)).Select(line => new FileSection(line, System.IO.Path.GetFileName(line), this)));
        //}
        //private bool FileFilter(string fileName)
        //{
        //    List<string> ignoredExtensions = new List<string> { ".bak", ".tmp", ".sс$", ".dwl", ".cd~" };
        //    string extensionFile = System.IO.Path.GetExtension(fileName).ToLower();
        //    bool result = ignoredExtensions.All(arg => arg != extensionFile) && File.GetAttributes(fileName) != FileAttributes.Hidden && !fileName.Contains("~$");
        //    if (result)
        //        return true;
        //    else
        //        return false;
        //}
    }
}
