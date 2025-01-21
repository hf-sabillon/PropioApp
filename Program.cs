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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();



            // Configurar AutoUpdater
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.Start("https://tu-servidor.com/actualizaciones/update.xml");

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