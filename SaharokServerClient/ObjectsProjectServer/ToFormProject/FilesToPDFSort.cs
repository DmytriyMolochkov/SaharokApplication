using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        public List<FileToProject> FilesToProjectfromAutoCAD = new List<FileToProject>();
        public List<FileToProject> FilesToProjectfromNanoCAD = new List<FileToProject>();

        private List<FileToProject> FilesWithExtensionError = new List<FileToProject>();
        private List<FileToProject> FilesWithNameError = new List<FileToProject>();

        private List<SectionToProject> AllSectionsToProject = new List<SectionToProject>();

        public FilesToPDFSort(List<SectionToProject> sectionsToProject)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = sectionsToProject.SelectMany(section => section.FilesToProject)
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            infoOfProcess.TotalFormsSections = sectionsToProject.Count();

            SortByApllications(sectionsToProject.SelectMany(section => section.FilesToProject));

            AllSectionsToProject = sectionsToProject;
        }
        public FilesToPDFSort(SectionToProject sectionToProject)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = sectionToProject.FilesToProject.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).ToList().Count();
            infoOfProcess.TotalFormsSections = 1;

            SortByApllications(sectionToProject.FilesToProject);

            AllSectionsToProject.Add(sectionToProject);
        }
        public FilesToPDFSort(List<FileToProject> filesToPDF)
        {
            InfoOfProcess infoOfProcess = InfoOfProcess.GetInstance();
            infoOfProcess.TotalFormsFiles = filesToPDF.Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF).Count();
            infoOfProcess.TotalFormsSections = 0;

            SortByApllications(filesToPDF);

            AllSectionsToProject = null;
        }

        private void SortByApllications(IEnumerable<FileToProject> filesToPDF)
        {
            FilesWithNameError = GetFilesWhithNameError(filesToPDF);
            filesToPDF.ForEachImmediate(file =>
            {
                switch (file.MethodPDFFile)
                {
                    case MethodPDFFile.Kompas:
                        {
                            FilesToProjectfromKompas.Add(file);
                            break;
                        }
                    case MethodPDFFile.Word:
                        {
                            FilesToProjectfromWord.Add(file);
                            break;
                        }
                    case MethodPDFFile.PDF:
                        {
                            FilesToProjectfromPDF.Add(file);
                            break;
                        }
                    case MethodPDFFile.Excel:
                        {
                            FilesToProjectfromExcel.Add(file);
                            break;
                        }
                    case MethodPDFFile.AutoCAD:
                        {
                            FilesToProjectfromAutoCAD.Add(file);
                            break;
                        }
                    case MethodPDFFile.NanoCAD:
                        {
                            FilesToProjectfromNanoCAD.Add(file);
                            break;
                        }
                    case MethodPDFFile.DontPDF:
                        {
                            break;
                        }
                    case MethodPDFFile.NoPDFMethod:
                        {
                            FilesWithExtensionError.Add(file);
                            break;
                        }
                    default:
                        {
                            throw new Exception($"Невозможное исключение, неизветсный метод формирования PDF: {file.MethodPDFFile}");
                        }
                }
            });
        }

        private List<FileToProject> GetFilesWhithNameError(IEnumerable<FileToProject> filesToPDF)
        {
            return filesToPDF
                .Where(file => file.MethodPDFFile != MethodPDFFile.DontPDF)
                .GroupBy(file => file.OutputFileName).Where(group => group.ToList().Count > 1)
                .SelectMany(group => group).ToList();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected FilesToPDFSort(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
