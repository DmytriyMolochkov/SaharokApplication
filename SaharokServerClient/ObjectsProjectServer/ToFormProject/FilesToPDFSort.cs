using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ObjectsProjectServer
{
    public class FilesToPDFSort : ISerializable
    {
        public List<FileToProject> FilesToPDFfromPDF = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromWord = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromExcel = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromKompas = new List<FileToProject>();
        public List<FileToProject> FilesToPDFfromAutoCad = new List<FileToProject>();

        public FilesToPDFSort(List<SectionToProject> sectionsToProject)
        {
            InfoOfProcess.TotalFormsFiles = sectionsToProject.SelectMany(section => section.FilesToPDF)
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            InfoOfProcess.TotalFormsSections = sectionsToProject.Count();

            FilesToPDFfromPDF = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToPDFfromWord = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToPDFfromExcel = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToPDFfromKompas = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToPDFfromAutoCad = sectionsToProject.SelectMany(section => section.FilesToPDF).Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();
        }
        public FilesToPDFSort(SectionToProject sectionToProject)
        {
            InfoOfProcess.TotalFormsFiles = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            InfoOfProcess.TotalFormsSections = 1;

            FilesToPDFfromPDF = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToPDFfromWord = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToPDFfromExcel = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToPDFfromKompas = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToPDFfromAutoCad = sectionToProject.FilesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();
        }
        public FilesToPDFSort(List<FileToProject> filesToPDF)
        {
            InfoOfProcess.TotalFormsFiles = filesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Count();

            FilesToPDFfromPDF = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.PDF).ToList();
            FilesToPDFfromWord = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Word).ToList();
            FilesToPDFfromExcel = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Excel).ToList();
            FilesToPDFfromKompas = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.Kompas).ToList();
            FilesToPDFfromAutoCad = filesToPDF.Where(file => file.MethodPDFFile == MethodPDFFile.AutoCad).ToList();
        }

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
