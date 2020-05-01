using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsProjectServer
{
    public static class InfoOfProcess
    {
        private static int totalFormsFiles;
        public static int TotalFormsFiles
        {
            get => totalFormsFiles;
            set => totalFormsFiles = value;
        }
        private static int totalFormsSections;
        public static int TotalFormsSections
        {
            get => totalFormsSections;
            set => totalFormsSections = value;
        }

        private static int completeFormsFiles;
        public static int CompleteFormsFiles
        {
            get => completeFormsFiles;
            set => completeFormsFiles = value;
        }

        private static int completeFormsSections;
        public static int CompleteFormsSections
        {
            get => completeFormsSections;
            set => completeFormsSections = value;
        }
    }
}
