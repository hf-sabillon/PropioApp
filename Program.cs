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



                // Continuar con el inicio de la aplicaci�n
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
            // L�gica para reiniciar la aplicaci�n si es necesario
            Application.Exit();
        }


        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
           

            if (args.Error != null)
            { 
          
                return;
            }

            if (args.IsUpdateAvailable)
            {
                // Registrar que hay una nueva versi�n disponible

            }
            else
            {
                // Registrar que la aplicaci�n est� actualizada
              
            }
        }

    }
}