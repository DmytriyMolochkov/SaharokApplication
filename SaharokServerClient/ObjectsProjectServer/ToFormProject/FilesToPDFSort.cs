using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ObjectsProjectServer
{
    [Serializable]
    public class FilesToPDFSort : ISerializable
    {
        public List<FileToProject> FilesToProjectfromPDF = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromWord = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromExcel = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromKompas = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromAutoCad = new List<FileToProject>();

        private List<FileToProject> AllFilesToProject { get; set; } = new List<FileToProject>();
        private List<SectionToProject> AllSectionsToProject { get; set; } = new List<SectionToProject>();

        public FilesToPDFSort(List<SectionToProject> sectionsToProject)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = sectionsToProject.SelectMany(section => section.FilesToProject)
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            infoOfProcess.TotalFormsSections = sectionsToProject.Count();

            FilesToProjectfromPDF = sectionsToProject.SelectMany(section => section.FilesToProject).Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToProjectfromWord = sectionsToProject.SelectMany(section => section.FilesToProject).Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToProjectfromExcel = sectionsToProject.SelectMany(section => section.FilesToProject).Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToProjectfromKompas = sectionsToProject.SelectMany(section => section.FilesToProject).Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToProjectfromAutoCad = sectionsToProject.SelectMany(section => section.FilesToProject).Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();

            AllFilesToProject = sectionsToProject.SelectMany(section => section.FilesToProject).ToList();
            AllSectionsToProject = sectionsToProject;
        }
        public FilesToPDFSort(SectionToProject sectionToProject)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            infoOfProcess.TotalFormsSections = 1;

            FilesToProjectfromPDF = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToProjectfromWord = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToProjectfromExcel = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToProjectfromKompas = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToProjectfromAutoCad = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();

            AllFilesToProject = sectionToProject.FilesToProject;
            AllSectionsToProject.Add(sectionToProject);
        }
        public FilesToPDFSort(List<FileToProject> filesToPDF)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = filesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Count();

            FilesToProjectfromPDF = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToProjectfromWord = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToProjectfromExcel = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToProjectfromKompas = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToProjectfromAutoCad = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();

            AllFilesToProject = filesToPDF;
            AllSectionsToProject = null;
        }

        //private List<FileToProject> GetAllFilesToProject()
        //{
        //    List<FileToProject> filesToProject = new List<FileToProject>();
        //    filesToProject.AddRange(this.FilesToProjectfromAutoCad);
        //    filesToProject.AddRange(this.FilesToProjectfromExcel);
        //    filesToProject.AddRange(this.FilesToProjectfromKompas);
        //    filesToProject.AddRange(this.FilesToProjectfromPDF);
        //    filesToProject.AddRange(this.FilesToProjectfromWord);
        //    return filesToProject;
        //}

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FilesToPDFSort(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
