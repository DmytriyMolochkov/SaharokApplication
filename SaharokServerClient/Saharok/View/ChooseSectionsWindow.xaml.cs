using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Saharok.View
{
    /// <summary>
    /// Логика взаимодействия для CreateNewProjectWindow.xaml
    /// </summary>
    public partial class ChooseSectionsWindow : Window
    {
        public ChooseSectionsWindow()
        {
            InitializeComponent();
        }
        private void ChooseSectionsPaths_PreviewDragEnter(object sender, DragEventArgs e)
        {
            bool isCorrect = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] directoryNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (string directoryName in directoryNames)
                {
                    if (!Directory.Exists(directoryName))
                    {
                        isCorrect = false;
                        break;
                    }
                    if(((TextBox)sender).Text.Contains(directoryName))
                    {
                        isCorrect = false;
                        break;
                    }
                }
            }
            if (isCorrect == true)
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void ChooseSectionsPaths_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] directoryNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            foreach (string directoryName in directoryNames)
                ((TextBox)sender).Text += directoryName + Environment.NewLine;
            ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
            e.Handled = true;
        }
    }
}
