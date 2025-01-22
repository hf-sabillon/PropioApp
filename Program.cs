using AutoUpdaterDotNET;

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


            // Configurar AutoUpdater
            AutoUpdater.ReportErrors = true;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/hf-sabillon/PropioApp/refs/heads/main/public/update.xml");

            // Continuar con el inicio de la aplicaci�n
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PropioTrayForm());
        }

        private static void AutoUpdater_ApplicationExitEvent()
        {
            // L�gica para reiniciar la aplicaci�n si es necesario
            Application.Exit();
        }

    }
}