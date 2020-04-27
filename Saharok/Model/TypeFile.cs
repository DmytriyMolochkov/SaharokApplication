using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectsProject;

namespace Saharok.Model
{
    //public enum MethodPDFFile
    //{
    //    Kompas = 0,
    //    Word = 1,
    //    Excel = 2,
    //    PDF = 3,
    //    AutoCad = 4,
    //    DontPDF = 10,
    //    NoPDFMethod = 11
    //}

    public enum MethodFormFile
    {
        PDF = 1,
        ZIP = 2
    }

    //public static class TypeFile
    //{
    //    public static MethodPDFFile ChooseMethodPDFFile(FileSection file)
    //    {
    //        switch ((Path.GetExtension(file.Path)).ToLower())
    //        {
    //            case ".pdf":
    //                {
    //                    return MethodPDFFile.PDF;
    //                }
    //            case ".docm":
    //            case ".doc":
    //            case ".docx":
    //                {
    //                    return MethodPDFFile.Word;
    //                }
    //            case ".xlsm":
    //            case ".xlsx":
    //            case ".xls":
    //                {
    //                    return MethodPDFFile.Excel;
    //                }
    //            case ".cdw":
    //                {
    //                    return MethodPDFFile.Kompas;
    //                }
    //            case ".dwg":
    //                {
    //                    //return MethodPDFFile.DontPDF;
    //                    return MethodPDFFile.AutoCad;
    //                }
    //            case ".png":
    //            case ".bmp":
    //            case ".jpeg":
    //            case ".jpg":
    //            case ".jfif":
    //                {
    //                    return MethodPDFFile.DontPDF;
    //                }
    //            default:
    //                {
    //                    return MethodPDFFile.NoPDFMethod;
    //                }
    //        }
    //    } 
    //}
}
