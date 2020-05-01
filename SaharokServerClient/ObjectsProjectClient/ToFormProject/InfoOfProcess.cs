using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectsProjectClient;

namespace ObjectsToFormProjectClient
{
    public static class InfoOfProcess
    {
        private static int totalFormsFiles;
        public static int TotalFormsFiles
        {
            get => totalFormsFiles;
            set
            {
                totalFormsFiles = value;
                OnPropertyChanged(nameof(TotalFormsFiles));
            }
        }

        private static int totalFormsSections;
        public static int TotalFormsSections
        {
            get => totalFormsSections;
            set
            {
                totalFormsSections = value;
                OnPropertyChanged(nameof(TotalFormsSections));
            }
        }

        private static int completeFormsFiles;
        public static int CompleteFormsFiles
        {
            get => completeFormsFiles;
            set
            {
                completeFormsFiles = value;
                OnPropertyChanged(nameof(CompleteFormsFiles));
            }
        }

        private static int completeFormsSections;
        public static int CompleteFormsSections
        {
            get => completeFormsSections;
            set
            {
                completeFormsSections = value;
                OnPropertyChanged(nameof(CompleteFormsSections));
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
