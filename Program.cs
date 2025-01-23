using AutoUpdaterDotNET;
using Microsoft.VisualBasic.Logging;

namespace PropioApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //"
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.Mandatory = false;
            AutoUpdater.ReportErrors = false;
            AutoUpdater.DownloadPath = Application.StartupPath;

            var currentDirectory = new DirectoryInfo(Application.StartupPath);
            if (currentDirectory.Parent != null)
            {
                AutoUpdater.InstallationPath = currentDirectory.Parent.FullName;
            }

            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            try
            {
                AutoUpdater.Start("https://raw.githubusercontent.com/hf-sabillon/distribution/refs/heads/main/main/version.xml");



                // Continuar con el inicio de la aplicación
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PropioTrayForm());
            }
            catch (Exception ex)
            {
               
            }
            finally
            {               
                

            }

        }

        private static void AutoUpdater_ApplicationExitEvent()
        {
            // Lógica para reiniciar la aplicación si es necesario
            Application.Exit();
        }


        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            File.AppendAllText("C:\\propioApp\\log1.txt",
             $"{DateTime.Now}: {args.Error} \n {args.IsUpdateAvailable}");

            if (args.Error != null)
            { 
                File.AppendAllText("C:\\propioApp\\log.txt",
               $"{DateTime.Now}: Error al comprobar actualizaciones. \n");
                return;
            }

            if (args.IsUpdateAvailable)
            {
                // Registrar que hay una nueva versión disponible

                File.AppendAllText("C:\\propioApp\\log.txt",
                $"{DateTime.Now}: Nueva versión disponible: {args.CurrentVersion} -> {args.InstalledVersion}");
                // AutoUpdater.NET descargará y aplicará automáticamente la actualización
                if (AutoUpdater.DownloadUpdate(args))
                {
                    Application.Exit();
                }
            }
            else
            {
                // Registrar que la aplicación está actualizada
              
                File.AppendAllText("C:\\propioApp\\log.txt",
                $"{DateTime.Now}: La aplicación está actualizada. \n");
            }
        }

    }
}