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
using System.Threading;

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

            string[] args = String.Join(" ", e.Args).Split('~');
            if (args[0] != "0")
            {
                ((MainWindowViewModel)window.DataContext).LoadProject(args[0]);
            }
            if (args[1] == "-1")
            {
                MessageBox.Show("Запустите Saharok.exe от имени Администратора для настройки ассоциаций Windows.", "Ошибка доступа к реестру");
            }

            window.Topmost = true;
            window.Show();
            //window.Activate();
            window.Topmost = false;

            window.Closed += (object _sender, EventArgs _e) => ((MainWindowViewModel)window.DataContext).AbortProcess();
        }
    }
}
