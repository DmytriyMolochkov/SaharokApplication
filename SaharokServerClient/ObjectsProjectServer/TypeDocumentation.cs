using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectsProjectServer
{
    public class TypeDocumentation
    {
        public string Name { get; set; }

        public string Path { get; set; }
        public Project Project { get; set; }
        public ObservableCollection<Section> Sections { get; set; }

        //public TypeDocumentation(string path, string name, Project parent)
        //{
        //    Path = path;
        //    Name = name;
        //    Project = parent;
        //    LoadSectionsWithRetries();
        //}

        //public Section GetSection(string path)
        //{
        //    return Sections.Where(s => path.StartsWith(s.Path)).FirstOrDefault();
        //}
        //protected async void LoadSectionsWithRetries()
        //{
        //    int @try = 0;
        //    do
        //    {
        //        try
        //        {
        //            LoadSections();
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

        //protected void LoadSections()
        //{
        //    Sections = new ObservableCollection<Section>(Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly)
        //        .Select(line => new Section(line, System.IO.Path.GetFileName(line), this)));
        //}
    }
}
