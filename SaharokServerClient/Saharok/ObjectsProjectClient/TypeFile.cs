using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saharok
{
    [Serializable]
    public enum MethodPDFFile
    {
        NoPDFMethod = 0,
        Kompas = 1,
        Word = 2,
        Excel = 3,
        PDF = 4,
        AutoCAD = 5,
        NanoCAD = 6,
        DontPDF = 10,
    }

    [Serializable]
    public enum MethodFormFile
    {
        PDF = 1,
        ZIP = 2
    }

    public static class TypeFile
    {
        static Dictionary<MethodPDFFile, string[]> MethodExtensionPairs { get; set; } = new Dictionary<MethodPDFFile, string[]> {
            { MethodPDFFile.PDF, ConfigurationManager.AppSettings["PDFExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.Word, ConfigurationManager.AppSettings["WordExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.Excel, ConfigurationManager.AppSettings["ExcelExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.Kompas, ConfigurationManager.AppSettings["KompasExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.AutoCAD, ConfigurationManager.AppSettings["AutoCADExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.NanoCAD, ConfigurationManager.AppSettings["NanoCADExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },
            { MethodPDFFile.DontPDF, ConfigurationManager.AppSettings["DontPDFExtensions"].Replace(" ", String.Empty).Split(',').Select(e => '.' + e).ToArray() },};

        public static MethodPDFFile ChooseMethodPDFFile(FileSection file)
        {
            return MethodExtensionPairs.FirstOrDefault(e => e.Value.Contains(Path.GetExtension(file.Path).ToLower())).Key;
        }
    }
}
