using Newtonsoft.Json;
using PropioApp.Clases;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Timer = System.Windows.Forms.Timer;

namespace PropioApp
{
    internal class PropioTrayForm : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _trayMenu;
        private readonly Timer _captureTimer;

       private const string ScreenshotPath = "Propio.png";

        private readonly string _tessdataPath = Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory, "tessdata"
       );

        private readonly string _machineName;

        private readonly string _logPath;
        public PropioTrayForm()
        {

            _machineName = Environment.MachineName;

            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PropioApp");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logPath = Path.Combine(logDirectory, "log.txt");
             


            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Capturar ahora", null, (s, e) => CaptureProcessAndLog(logDirectory));
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Salir", null, OnExitClick);

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // o tu propio .ico
                ContextMenuStrip = _trayMenu,
                Text = "Propio App",
                Visible = false
            };

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Load += (s, e) => { this.Visible = false; };

            _captureTimer = new Timer
            {
                Interval = 15_000 // 1 minuto. Ajusta a tu gusto
            };
            _captureTimer.Tick += (s, e) => CaptureProcessAndLog(logDirectory);
            _captureTimer.Start();

        }

        private void OnExitClick(object sender, EventArgs e)
        {
            _captureTimer.Stop();
            _captureTimer.Dispose();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        // Método principal: Captura, hace OCR y busca tokens
        private void CaptureProcessAndLog(string logDirectory)
        {
            try
            {
                           

                   File.AppendAllText("C:\\propioApp\\log3.txt",
                     $"{DateTime.Now}: Nuevo Log ProcessAndLog 2.0.0 \n");


                File.AppendAllText(_logPath,
                     $"{DateTime.Now}: ProcessAndLog \n");


                string vScreenshotPath =  Path.Combine(logDirectory, ScreenshotPath);

                // 1. Capturar pantalla
                CaptureScreen(vScreenshotPath);

                // 2. OCR
                string extractedText = DoOCR(vScreenshotPath);

                // 3. Buscar tokens ("interpreter id", "client id", etc.)
                CheckTokensAndCallApi(extractedText);

                // Ejemplo: mostrar notificación mínima
                _trayIcon.ShowBalloonTip(2000, "App Check", "PropioApp completed", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
               File.AppendAllText(_logPath,
                    $"{DateTime.Now}: Error: {ex.Message}\n {ex.StackTrace}");  
            }
        }

        // Captura la pantalla principal
        private void CaptureScreen(string filePath)
        {
            var bounds = Screen.PrimaryScreen.Bounds;
            using var bmp = new Bitmap(bounds.Width, bounds.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            bmp.Save(filePath, ImageFormat.Png);
        }

        // Realiza OCR con Tesseract
        private string DoOCR(string imagePath)
        {
            if (!Directory.Exists(_tessdataPath))
                throw new DirectoryNotFoundException("No se encontró la carpeta tessdata en: " + _tessdataPath);

            using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            return page.GetText();
        }


        private async void CheckTokensAndCallApi(string text)
        {

            if (!text.Contains("Call Status") &&
                !text.Contains("Client ID") &&
                !text.Contains("Greeting to Client") &&
                !text.Contains("Connected Participants") &&
                !text.Contains("Connected") &&
                !text.Contains("will be your")) return;

            var request = new RequestService();
            var vInterpreterID = string.Empty;
            var vInterpreterName = string.Empty;

            string clientIdPattern = @"Client ID:\s*(\d+)";
            string namePattern = @"My name is ([\w\s]+),";
            string interpreterIdPattern = @"interpreter ID\s*(\d+)";


            // Buscar coincidencias
            var clientIdMatch = Regex.Match(text, clientIdPattern);
            var nameMatch = Regex.Match(text, namePattern);
            var interpreterIdMatch = Regex.Match(text, interpreterIdPattern);

            string clientId = clientIdMatch.Success ? clientIdMatch.Groups[1].Value : "";
            vInterpreterName = nameMatch.Success ? nameMatch.Groups[1].Value : "";
            vInterpreterID = interpreterIdMatch.Success ? interpreterIdMatch.Groups[1].Value : "";

            foreach (var line in text.Split('\n'))
            {
                try
                {
                    if (line.Contains("Welcome"))
                    {
                        var details = ExtractInterpreterDetails(line);
                        vInterpreterID = details.Item1;
                        vInterpreterName = details.Item2;
                    }

                    if (line.Contains("Status"))
                    {
                        var status = ExtractStatus(line);
                        request = BuildRequest(request, vInterpreterID, vInterpreterName, status, _machineName);
                        saveRegister(request);

                    }

                    if (line.Contains("Client ID"))
                    {

                        request = BuildRequest(request, vInterpreterID, vInterpreterName, "onCall", _machineName, clientId);
                        saveRegister(request);

                    }



                    if (line.Contains("will be your") ||
                        line.Contains("Connected") ||
                        line.Contains("Connected Participants") ||
                        line.Contains("Greeting to Client"))
                    {

                        request = BuildRequest(request, vInterpreterID, vInterpreterName, "onCall", _machineName, clientId);
                        saveRegister(request);
                    }

                }
                catch (Exception ex)
                {
                    File.AppendAllText(_logPath,
                    $"{DateTime.Now}: Error: {ex.Message}\n {ex.StackTrace}");
                }
            }

        
        }


        private string EscapeJson(string val)
        {
            return val.Replace("\"", "\\\"");
        }

        // Ocultar la ventana
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Visible = false;
        }

       
        private static (string, string) ExtractInterpreterDetails(string line)
        {
            var lineArray = line.Split(',');
            var interpreterDetails = lineArray[1].Split('(');
            var interpreterId = interpreterDetails[1].Trim(')').Trim();
            //var match = Regex.Match(interpreterId, @"\d+(?=\))");
            //interpreterId = match.Success ? match.Value : string.Empty;
            var interpreterName = interpreterDetails[0].Trim();
            return (interpreterId, interpreterName);
        }


        private static string ExtractStatus(string line)
        {
            return line.Split(':')[1].Trim();
        }

        private static RequestService BuildRequest(RequestService request, string id, string name, string status, string pcName, string clientId = "")
        {
            return new RequestService
            {
                interpreterId = id,
                interpreterName = name,
                status = status,
                date = "GETDATE()",
                image = "0",
                pcName = pcName,
                clientId = clientId
            };
        }

        private static void saveRegister(RequestService request)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var uri = new Uri("http://18.214.97.156:10000/propio/api/collect/propio");
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                PostRequest(uri, content).Wait();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private static async Task<string> PostRequest(Uri uri, HttpContent content)
        {
            try
            {
                using var client = new HttpClient();

                var response = await client.PostAsync(uri, content);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
