using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Saharok.View;
using Saharok.ViewModel;
using Saharok.Model;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using System.IO;
using Saharok.Model.Client;

namespace Saharok
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindowView window { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            window = new MainWindowView();
            if (e.Args.Length == 1)
            {
                string filename = e.Args[0].ToString();
                ((MainWindowViewModel)window.DataContext).LoadProject(e.Args[0]);
            }
            window.Show();

            if (e.Args.Length == 0)
            {
                new MyFileAssociation();
            }
            ClientObject.Connect();
        }
    }
}
