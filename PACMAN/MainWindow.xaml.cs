using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
//using CorrosionModels;
using System.IO;
using Microsoft.Win32;
using SharpDX.Collections;
using Telerik.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Telerik.Windows.Controls.ChartView;
using System.Windows.Threading;
using System.Threading;
using SharpDX.Direct3D10;
using System.Diagnostics;
using System.Configuration;
using System.Net.Mail;
using System.Xml;
using Microsoft.Extensions.Logging;
using Telerik.Windows.Controls.Legend;
using Telerik.Windows.Controls.Map;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections;
using System.Runtime.Remoting.Contexts;
using System.Windows.Markup;
//using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
//using SharpDX.Direct2D1;

namespace PACMAN
{
    public partial class MainWindow : Window
    {
        private CorrosionModelEntities db = new CorrosionModelEntities();

        public static int savedindex { get; set; }

        public List<string> errorMessages = new List<string>();

        private CorrosionData cd = new CorrosionData();

        private List<string> baseNamesAndTimeStamps = new List<string>();

        public List<CorrosionDataPoint> dropletPoints { get; set; }
        public List<CorrosionDataPoint> temperaturePoints { get; set; }
        public List<CorrosionDataPoint> realativeHumidityPoints { get; set; }
        public List<CorrosionDataPoint> precipitationPoints { get; set; }
        public List<CorrosionDataPoint> surfacePollutantPoints { get; set; }
        public List<CorrosionDataPoint> timeOfWetnessPoints { get; set; }
        public List<CorrosionDataPoint> pitPoints { get; set; }
        public List<CorrosionDataPoint> steelLossPoints { get; set; }

        public List<CorrosionDataPoint> steelCorr1Points { get; set; }

        public List<CorrosionDataPoint> steelCorr2Points { get; set; }

        string errorFilePath = "";//ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile1";
        string errorFilePath1 = "";//ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile1";
        string errorFilePath2 = "";//ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile2";
        string errorFilePath3 = "";//ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile3";
        const int sizeLimitInBytes = 500 * 1024 * 1024; //500 * 1024 * 1024 is a limit of 500 MB

        public MainWindow()
        {
            StyleManager.ApplicationTheme = new VisualStudio2013Theme();
            if (ConfigurationManager.AppSettings["ErrorFilePath"] == "%Documents%")
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                errorFilePath = documentsPath + "\\ErrorFile1";
                errorFilePath1 = documentsPath + "\\ErrorFile1";
                errorFilePath2 = documentsPath + "\\ErrorFile2";
                errorFilePath3 = documentsPath + "\\ErrorFile3";
            }
            else
            {
                errorFilePath = ConfigurationManager.AppSettings["ErrorFilePath"] + "\\ErrorFile1";
                errorFilePath1 = ConfigurationManager.AppSettings["ErrorFilePath"] + "\\ErrorFile1";
                errorFilePath2 = ConfigurationManager.AppSettings["ErrorFilePath"] + "\\ErrorFile2";
                errorFilePath3 = ConfigurationManager.AppSettings["ErrorFilePath"] + "\\ErrorFile3";
            }
            InitializeComponent();
            
            ToggleLoadingVisability(false);
            DropletsChart.HorizontalAxis.Title = "Days";
            DropletsChart.VerticalAxis.Title = "Average Number of Droplets";
            TemperatureChart.HorizontalAxis.Title = "Date";
            TemperatureChart.VerticalAxis.Title = "Temperature (C)";
            RelativeHumidityChart.HorizontalAxis.Title = "Date";
            RelativeHumidityChart.VerticalAxis.Title = "RH(%)";
            PrecipitationChart.HorizontalAxis.Title = "Date";
            PrecipitationChart.VerticalAxis.Title = "Precipitation (inches)";
            SurfacePollutantsChart.HorizontalAxis.Title = "Date";
            SurfacePollutantsChart.VerticalAxis.Title = "Surface Pollutant density (mg/m)";
            TimeOfWetnessChart.HorizontalAxis.Title = "Date";
            TimeOfWetnessChart.VerticalAxis.Title = "TOW current (mA)";
            PitDistributionChart.HorizontalAxis.Title = "Pit Diameter (\u03BCm)";
            PitDistributionChart.VerticalAxis.Title = "Pit Count";
            SteelWeightLossChart.HorizontalAxis.Title = "Time(days)";
            SteelWeightLossChart.VerticalAxis.Title = "mm";
        }

        private void downloadDroplets_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Day, Average Number of Droplets");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in dropletPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.Y.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadRelativeHumidity_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Date, RH(%)");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in realativeHumidityPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.DateString.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadTemperature_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Date, Temperature (C)");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in temperaturePoints)
                {
                    string x = c.Value.ToString();
                    string y = c.DateString.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadPrecipitation_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Date, Precipitation (inches)");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in precipitationPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.DateString.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadSurfacePollutants_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Date, Surface Pollutant Density (mg/m)");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in surfacePollutantPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.DateString.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadTimeOfWetness_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Date, TOW current (mA)");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in timeOfWetnessPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.DateString.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadPit_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Pit Diameter (\u03BCm), Pit Count");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in pitPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.Series.ToString();
                    string content = x + "," + y;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadSteelWeightLoss_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("Time (days), mm");
                sb.Append("\r\n");
                foreach (CorrosionDataPoint c in steelLossPoints)
                {
                    string x = c.Value.ToString();
                    string y = c.Series.ToString();
                    string content = y + "," + x;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void downloadSteelCorr_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                string folderPath = saveFileDialog.FileName;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("CS1 Time (days), CS1 " + "\u03BC" + "m, CS2 Time (days), CS2 " + "\u03BC" + "m");
                sb.Append("\r\n");
                int i = 0;
                foreach (CorrosionDataPoint c in steelCorr1Points)
                {
                    CorrosionDataPoint d = steelCorr2Points[i];

                    string x = c.Value.ToString();
                    string y = c.Series.ToString();
                    string a = d.Value.ToString();
                    string b = d.Series.ToString();

                    string content = y + "," + x + "," + a + "," + b;
                    sb.Append(content);

                    sb.Append("\r\n");
                }
                string data = sb.ToString();
                System.IO.File.WriteAllText(folderPath, data);
            }
        }

        private void RibbonHomeClick(object sender, RoutedEventArgs e)
        {
            DropletsPane.Visibility = Visibility.Collapsed;
            RelativeHumidityPane.Visibility = Visibility.Collapsed;
            TemperaturePane.Visibility = Visibility.Collapsed;
            PrecipitationPane.Visibility = Visibility.Collapsed;
            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
            PitDistributionPane.Visibility = Visibility.Collapsed;
            SteelWeightLossPane.Visibility = Visibility.Collapsed;
            FleetAnalysisPane.Visibility = Visibility.Collapsed;
            SteelCorrPane.Visibility = Visibility.Collapsed;
        }

        private void RibbonManualDownloadClick(object sender, RoutedEventArgs e)
        {
            List<t_Sensor_Data> entryList = new List<t_Sensor_Data>();
            List<t_ErrorList> errorList = new List<t_ErrorList>();
            //string errorFilePath = "|DataDirectory|\\ErrorLog.txt";
            string errorFilePath1 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile1";
            string errorFilePath2 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile2";
            string errorFilePath3 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile3";
            DateTime now = new DateTime();
            using (var client = new ImapClient(new ProtocolLogger("imap.log")))
            {
                try
                {
                    client.Connect(ConfigurationManager.AppSettings["Domain"], 993, SecureSocketOptions.SslOnConnect);

                    string pass = ConfigurationManager.AppSettings["Password"];

                    client.Authenticate(ConfigurationManager.AppSettings["User"], pass);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    IMailFolder parentFolder = client.GetFolder(client.PersonalNamespaces[0]);
                    var archive = parentFolder.Create("Archive", true);
                    inbox.Open(FolderAccess.ReadWrite);

                    //var A = inbox.GetFolder("A");


                    //Console.WriteLine("Total messages: {0}", inbox.Count);
                    //Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    List<DateTime> dateList = new List<DateTime>();

                    using (var context = new CorrosionModelEntities())
                    {
                        int appLocation = Convert.ToInt32(ConfigurationManager.AppSettings["Location"]);
                        dateList = context
                        .t_Sensor_Data
                        .Where(u => u.Location == appLocation)
                        .Select(u => u.Timestamp)
                        .ToList();
                    }
                    var items = inbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);
                    foreach (var item in items)
                    {
                        var message = inbox.GetMessage(item.UniqueId);


                        Console.WriteLine("Subject: {0}", message.Subject);
                        if (message.Subject.Contains("Samples"))
                        {

                            try
                            {

                                string[] splitsubject = message.Subject.Split(
                                new string[] { " " },
                                StringSplitOptions.None
                                );

                                int year = Convert.ToInt32(splitsubject[1].Substring(0, 4));
                                int month = Convert.ToInt32(splitsubject[1].Substring(4, 2));
                                int day = Convert.ToInt32(splitsubject[1].Substring(6, 2));
                                if (year > 2025 || (year == 2025 && ((month == 6 && day >= 12) || (month > 6))))
                                {
                                    var body = message.TextBody;

                                    string editedBody = body.Replace("\r", "");

                                    string[] lines = editedBody.Split(
                                    new string[] { "\n" },
                                    StringSplitOptions.None
                                    );

                                    foreach (string line in lines)
                                    {
                                        string[] rawValues = line.Split(
                                        new string[] { "," },
                                        StringSplitOptions.None
                                        );

                                        if (rawValues.Count() == 12)
                                        {
                                            try
                                            {
                                                RawInputData rid = new RawInputData();
                                                rid.col1 = rawValues[0];
                                                rid.col2 = rawValues[1];
                                                rid.col3 = rawValues[2];
                                                rid.col4 = rawValues[3];
                                                rid.col5 = rawValues[4];
                                                rid.col6 = rawValues[5];
                                                rid.col7 = rawValues[6];
                                                rid.col8 = rawValues[7];
                                                rid.col9 = rawValues[8];
                                                rid.col10 = rawValues[9];
                                                rid.col11 = rawValues[10];
                                                rid.col12 = rawValues[11];

                                                t_Sensor_Data sensorData = new t_Sensor_Data();

                                                sensorData.Location = Convert.ToInt32(ConfigurationManager.AppSettings["Location"]);

                                                int dataYear = 2000 + Convert.ToInt32(rid.col1.Substring(0, 2));
                                                string dataMonth = rid.col1.Substring(2, 2);
                                                string dataDay = rid.col1.Substring(4, 2);
                                                string dataHour = rid.col1.Substring(6, 2);
                                                string dataMinite = rid.col1.Substring(8, 2);
                                                DateTime myDate = DateTime.ParseExact(dataYear + "-" + dataMonth + "-" + dataDay + " " + dataHour + ":" + dataMinite + ":00", "yyyy-MM-dd HH:mm:ss",
                                                   System.Globalization.CultureInfo.InvariantCulture);
                                                sensorData.Timestamp = myDate;

                                                if (dateList.Contains(myDate))
                                                {
                                                    break;
                                                }

                                                double shtTemp = Convert.ToDouble(rid.col2) / 10;
                                                sensorData.SHTTemp = shtTemp;

                                                double shtRH = Convert.ToDouble(rid.col3) / 10;
                                                sensorData.SHTRH = shtRH;

                                                double corr1 = -0.00943 + 0.000001074 * Convert.ToDouble(rid.col4);
                                                sensorData.Corr1 = corr1;

                                                double corr2 = -0.00943 + 0.000001074 * Convert.ToDouble(rid.col5);
                                                sensorData.Corr2 = corr2;

                                                double TOW = -0.000332656 + Convert.ToDouble(rid.col6) * 0.000000171386;
                                                sensorData.TOW = TOW;

                                                string temp1HexValue = Convert.ToInt32(rid.col7).ToString("X");
                                                double temp1val1 = Convert.ToInt32(temp1HexValue.Substring(0, 1), 16) * 16;
                                                double temp1val2 = Convert.ToInt32(temp1HexValue.Substring(1, 1), 16);
                                                double temp1val3 = Convert.ToInt32(temp1HexValue.Substring(2, 1), 16) / 16;
                                                double temp1 = temp1val1 + temp1val2 + temp1val3;
                                                sensorData.TC1 = temp1;

                                                string temp2HexValue = Convert.ToInt32(rid.col8).ToString("X");
                                                double temp2val1 = Convert.ToInt32(temp2HexValue.Substring(0, 1), 16) * 16;
                                                double temp2val2 = Convert.ToInt32(temp2HexValue.Substring(1, 1), 16);
                                                double temp2val3 = Convert.ToInt32(temp2HexValue.Substring(2, 1), 16) / 16;
                                                double temp2 = temp2val1 + temp2val2 + temp2val3;
                                                sensorData.TC2 = temp2;

                                                string temp3HexValue = Convert.ToInt32(rid.col9).ToString("X");
                                                double temp3val1 = Convert.ToInt32(temp3HexValue.Substring(0, 1), 16) * 16;
                                                double temp3val2 = Convert.ToInt32(temp3HexValue.Substring(1, 1), 16);
                                                double temp3val3 = Convert.ToInt32(temp3HexValue.Substring(2, 1), 16) / 16;
                                                double temp3 = temp3val1 + temp3val2 + temp3val3;
                                                sensorData.TC2Cold = temp3;

                                                double refResister = -0.00943 + 0.000001074 * Convert.ToDouble(rid.col10);
                                                sensorData.ResistanceReference = refResister;

                                                double BatteryV = -0.09008 + Convert.ToDouble(rid.col11) * 0.00381;
                                                sensorData.Battery = BatteryV;

                                                bool Rain = Convert.ToBoolean(Convert.ToInt32(rid.col12));
                                                sensorData.Rain = Rain;
                                                entryList.Add(sensorData);

                                                dateList.Add(myDate);

                                                //client.Inbox.AddFlags(new MailKit.UniqueId[] { item.UniqueId }, MessageFlags.Deleted, true);
                                            }
                                            catch
                                            {
                                                t_ErrorList error = new t_ErrorList();
                                                error.MESSAGEUID = item.UniqueId.ToString();
                                                error.ERRORMESSAGE = "Error converting data from message";
                                                error.ERRORDATE = DateTime.Now;
                                                error.EMAILTITLE = message.Subject;
                                                //errorList.Add(error);
                                                long length = 0;
                                                try
                                                {
                                                    length = new System.IO.FileInfo(errorFilePath).Length;
                                                    if (length > sizeLimitInBytes)
                                                    {
                                                        if (errorFilePath.Contains("ErrorFile1"))
                                                        {
                                                            errorFilePath = errorFilePath2;
                                                        }
                                                        else if (errorFilePath.Contains("ErrorFile2"))
                                                        {
                                                            errorFilePath = errorFilePath3;
                                                        }
                                                        else if (errorFilePath.Contains("ErrorFile3"))
                                                        {
                                                            errorFilePath = errorFilePath1;
                                                        }
                                                        else
                                                        {
                                                            errorFilePath = errorFilePath1;
                                                        }
                                                    }
                                                }
                                                catch (Exception exc)
                                                {

                                                }


                                                using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                                                {
                                                    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                                                    {
                                                        builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                                                        
                                                    });

                                                    ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();
                                                    now = DateTime.Now;
                                                    using (logger.BeginScope("[scope is enabled]"))
                                                    {
                                                        logger.LogInformation(now + ": Error Parsing Data in Message with subject " + message.Subject);
                                                    }
                                                }


                                            }
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                using (var context = new CorrosionModelEntities())
                                {
                                    t_ErrorList error = new t_ErrorList();
                                    error.MESSAGEUID = item.UniqueId.ToString();
                                    error.ERRORMESSAGE = "Error converting data from message";
                                    error.ERRORDATE = DateTime.Now;
                                    error.EMAILTITLE = message.Subject;

                                    long length = 0;
                                    try
                                    {
                                        length = new System.IO.FileInfo(errorFilePath).Length;
                                        if (length > sizeLimitInBytes)
                                        {
                                            if (errorFilePath.Contains("ErrorFile1"))
                                            {
                                                errorFilePath = errorFilePath2;
                                            }
                                            else if (errorFilePath.Contains("ErrorFile2"))
                                            {
                                                errorFilePath = errorFilePath3;
                                            }
                                            else if (errorFilePath.Contains("ErrorFile3"))
                                            {
                                                errorFilePath = errorFilePath1;
                                            }
                                            else
                                            {
                                                errorFilePath = errorFilePath1;
                                            }
                                        }
                                    }
                                    catch (Exception exc)
                                    {

                                    }

                                    //errorList.Add(error);

                                    using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                                    {
                                        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                                        {
                                            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                                        });

                                        ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();
                                        now = DateTime.Now;
                                        using (logger.BeginScope("[scope is enabled]"))
                                        {
                                            logger.LogInformation(now + ": Error Opening Message with subject " + message.Subject);
                                        }
                                    }
                                }
                            }
                            inbox.MoveTo(item.UniqueId, archive);
                        }
                    }
                    //client.Inbox.Expunge();
                    client.Disconnect(true);
                }
                catch
                {
                    long length = 0;
                    try
                    {
                        length = new System.IO.FileInfo(errorFilePath).Length;
                        if (length > sizeLimitInBytes)
                        {
                            if (errorFilePath.Contains("ErrorFile1"))
                            {
                                errorFilePath = errorFilePath2;
                            }
                            else if (errorFilePath.Contains("ErrorFile2"))
                            {
                                errorFilePath = errorFilePath3;
                            }
                            else if (errorFilePath.Contains("ErrorFile3"))
                            {
                                errorFilePath = errorFilePath1;
                            }
                            else
                            {
                                errorFilePath = errorFilePath1;
                            }
                        }
                    }
                    catch (Exception exc)
                    {

                    }

                    using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                    {
                        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                        {
                            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                        });

                        ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();
                        now = DateTime.Now;
                        using (logger.BeginScope("[scope is enabled]"))
                        {
                            logger.LogInformation(now + ": Error Connecting to email server");
                        }
                    }
                }
            }
            using (var context = new CorrosionModelEntities())
            {
                foreach (var sensorData in entryList)
                {
                    context.t_Sensor_Data.Add(sensorData);
                }
                foreach (var errors in errorList)
                {
                    //context.t_ErrorList.Add(errors);
                }
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    long length = 0;
                    try
                    {
                        length = new System.IO.FileInfo(errorFilePath).Length;
                        if (length > sizeLimitInBytes)
                        {
                            if (errorFilePath.Contains("ErrorFile1"))
                            {
                                errorFilePath = errorFilePath2;
                            }
                            else if (errorFilePath.Contains("ErrorFile2"))
                            {
                                errorFilePath = errorFilePath3;
                            }
                            else if (errorFilePath.Contains("ErrorFile3"))
                            {
                                errorFilePath = errorFilePath1;
                            }
                            else
                            {
                                errorFilePath = errorFilePath1;
                            }
                        }
                    }
                    catch (Exception exc)
                    {

                    }

                    using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                    {
                        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                        {
                            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                        });

                        ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();

                        now = DateTime.Now;
                        using (logger.BeginScope("[scope is enabled]"))
                        {
                            logger.LogInformation(now + ": Error Saving to Database");
                        }
                    }
                }
            }
        }

        private void RibbonAddLocationClick(object sender, RoutedEventArgs e)
        {
            errorMessages.Clear();
            DropletsPane.Visibility = Visibility.Collapsed;
            RelativeHumidityPane.Visibility = Visibility.Collapsed;
            TemperaturePane.Visibility = Visibility.Collapsed;
            PrecipitationPane.Visibility = Visibility.Collapsed;
            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
            PitDistributionPane.Visibility = Visibility.Collapsed;
            SteelWeightLossPane.Visibility = Visibility.Collapsed;
            FleetAnalysisPane.Visibility = Visibility.Collapsed;
            SteelCorrPane.Visibility = Visibility.Collapsed;
            DescriptionPane.IsActive = true;

            AddLocation addLocationDlg = new AddLocation();
            addLocationDlg.ShowDialog();
            if (addLocationDlg.canceled == false)
            {
                try
                {
                    List<t_Location> locations = new List<t_Location>();
                    using (var context = new CorrosionModelEntities())
                    {
                        locations = context.t_Location.ToList();
                    }

                    int newIndex = 0;
                    foreach (t_Location location in locations)
                    {
                        if (location.Id > newIndex)
                        {
                            newIndex = location.Id;
                        }
                    }
                    newIndex++;

                    t_Location newLocation = new t_Location();
                    newLocation.Id = newIndex;
                    newLocation.Name = addLocationDlg.LocationName;
                    using (var context = new CorrosionModelEntities())
                    {
                        context.t_Location.Add(newLocation);
                        context.SaveChanges();
                    }

                    foreach (PollutionModel pm in addLocationDlg.gridResults)
                    {
                        t_Salt_Composition salt = new t_Salt_Composition();
                        t_Base_Properties baseProp = new t_Base_Properties();

                        baseProp.Salt_dep = pm.Deposit;
                        baseProp.Conc = pm.RainConc;
                        baseProp.pH = pm.RainDep;
                        baseProp.Month = pm.Month;
                        baseProp.Location = newIndex;

                        salt.NH4NO3 = pm.AmmNit;
                        salt.NH42SO4 = pm.AmmSul;
                        salt.NaCl = pm.NaCl;
                        salt.MgC12 = pm.MgCl2;
                        salt.Na2S04 = pm.Na2S04;
                        salt.CaC12 = pm.CaC12;
                        salt.KCI = pm.KCI;
                        salt.MgSO4 = pm.MgSO4;
                        salt.K2SO4 = pm.K2SO4;
                        salt.CaSO4 = pm.CaSO4;
                        salt.Total = pm.total;
                        salt.HSMass = pm.HSMass;
                        salt.HNMass = pm.HNMass;
                        salt.MinSalts = pm.Sea;
                        salt.Month = pm.Month;
                        salt.Location = newIndex;

                        using (var context = new CorrosionModelEntities())
                        {
                            context.t_Base_Properties.Add(baseProp);
                            context.t_Salt_Composition.Add(salt);
                            context.SaveChanges();
                        }
                    }
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                    {
                        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                        {
                            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                        });

                        ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();

                        DateTime now = DateTime.Now;
                        using (logger.BeginScope("[scope is enabled]"))
                        {
                            logger.LogInformation(now + ": Error Connecting to Database: " + ex.Message);
                        }
                    }

                    MessageBoxResult result = MessageBox.Show("Error Connecting to Database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception error)
                {
                    using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                    {
                        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                        {
                            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                        });

                        ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();

                        DateTime now = DateTime.Now;
                        using (logger.BeginScope("[scope is enabled]"))
                        {
                            logger.LogInformation(now + ": General Error Occured when updating database: " + error.Message);
                        }
                    }

                    MessageBoxResult result = MessageBox.Show("General Error Occured when updating database: " + error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void RibbonCloseClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void RibbonTimespanClick(object sender, RoutedEventArgs e)
        {
            errorMessages.Clear();
            DropletsPane.Visibility = Visibility.Collapsed;
            RelativeHumidityPane.Visibility = Visibility.Collapsed;
            TemperaturePane.Visibility = Visibility.Collapsed;
            PrecipitationPane.Visibility = Visibility.Collapsed;
            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
            PitDistributionPane.Visibility = Visibility.Collapsed;
            SteelWeightLossPane.Visibility = Visibility.Collapsed;
            FleetAnalysisPane.Visibility = Visibility.Collapsed;
            SteelCorrPane.Visibility = Visibility.Collapsed;
            DescriptionPane.IsActive = true;

            //IncrementLoadingBar("Begining Analysis", 10);

            List<t_Location> locations = new List<t_Location>();
            try
            {
                using (var context = new CorrosionModelEntities())
                {
                    locations = context.t_Location.ToList();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                {
                    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                    });

                    ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();

                    DateTime now = DateTime.Now;
                    using (logger.BeginScope("[scope is enabled]"))
                    {
                        logger.LogInformation(now + ": Error Connecting to Database: " + ex.Message);
                    }
                }

                MessageBoxResult result = MessageBox.Show("Error Connecting to Database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception error)
            {
                using (StreamWriter logFileWriter = new StreamWriter(errorFilePath, append: true))
                {
                    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
                    });

                    ILogger<MainWindow> logger = loggerFactory.CreateLogger<MainWindow>();

                    DateTime now = DateTime.Now;
                    using (logger.BeginScope("[scope is enabled]"))
                    {
                        logger.LogInformation(now + ": General Error Occured when connecting to database: " + error.Message);
                    }
                }

                MessageBoxResult result = MessageBox.Show("General Error Occured when connecting to database: " + error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            var timespanAnalysisWindow = new TimespanAnalysis(locations);
            timespanAnalysisWindow.ShowDialog();
            if (timespanAnalysisWindow.canceled == false)
            {
                try
                {
                    ToggleLoadingVisability(true);

                    IncrementLoadingBar("Begining Analysis", 10);


                    cd = singleAnalysisFromDate(timespanAnalysisWindow.entireRange, timespanAnalysisWindow.start, timespanAnalysisWindow.end, timespanAnalysisWindow.location);

                    if (errorMessages.Count == 0)
                    {

                        int totDays = (timespanAnalysisWindow.end - timespanAnalysisWindow.start).Days;
                        bool showMonths = (totDays > 60);

                        DropletsChart.Series.Clear();
                        CategoricalSeries series = new LineSeries();
                        series.ItemsSource = cd.getDropletsPoints(true);
                        dropletPoints = cd.getDropletsPoints(false);
                        if (showMonths)
                        {
                            CategoricalAxis dateTimeAxis = new CategoricalAxis();
                            dateTimeAxis.SmartLabelsMode = Telerik.Charting.AxisSmartLabelsMode.SmartStep;
                            dateTimeAxis.MajorTickInterval = 1;
                            DropletsChart.HorizontalAxis = dateTimeAxis;
                            DropletsChart.HorizontalAxis.Title = "Wetness Period";
                        }
                        else
                        {
                            CategoricalAxis dateTimeAxis = new CategoricalAxis();
                            dateTimeAxis.SmartLabelsMode = Telerik.Charting.AxisSmartLabelsMode.SmartStep;
                            dateTimeAxis.MajorTickInterval = 1;
                            DropletsChart.HorizontalAxis = dateTimeAxis;
                            DropletsChart.HorizontalAxis.Title = "Wetness Period";
                        }
                        series.CategoryBinding = new PropertyNameDataPointBinding("Y");
                        series.ValueBinding = new PropertyNameDataPointBinding("Value");
                        DropletsChart.Series.Add(series);

                        TemperatureChart.Series.Clear();
                        CategoricalSeries series2 = new LineSeries();
                        series2.ItemsSource = cd.getTemperaturePoints();
                        temperaturePoints = cd.getTemperaturePoints();
                        if (showMonths)
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 3;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                            dateTimeAxis.LabelFormat = "MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            TemperatureChart.HorizontalAxis = dateTimeAxis;
                            TemperatureChart.HorizontalAxis.Title = "Date";
                        }
                        else
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd";
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            TemperatureChart.HorizontalAxis = dateTimeAxis;
                            TemperatureChart.HorizontalAxis.Title = "Days";
                        }
                        series2.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series2.ValueBinding = new PropertyNameDataPointBinding("Value");
                        TemperatureChart.Series.Add(series2);

                        RelativeHumidityChart.Series.Clear();
                        CategoricalSeries series3 = new LineSeries();
                        series3.ItemsSource = cd.getRelativeHumidityPoints();
                        realativeHumidityPoints = cd.getRelativeHumidityPoints();
                        if (showMonths)
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 3;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                            dateTimeAxis.LabelFormat = "MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            RelativeHumidityChart.HorizontalAxis = dateTimeAxis;
                            RelativeHumidityChart.HorizontalAxis.Title = "Date";
                        }
                        else
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            RelativeHumidityChart.HorizontalAxis = dateTimeAxis;
                            RelativeHumidityChart.HorizontalAxis.Title = "Date";
                        }
                        series3.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series3.ValueBinding = new PropertyNameDataPointBinding("Value");
                        RelativeHumidityChart.Series.Add(series3);

                        PrecipitationChart.Series.Clear();
                        CategoricalSeries series4 = new LineSeries();
                        series4.ItemsSource = cd.getPrecipitationPoints();
                        precipitationPoints = cd.getPrecipitationPoints();
                        if (showMonths)
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 3;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                            dateTimeAxis.LabelFormat = "MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            PrecipitationChart.HorizontalAxis = dateTimeAxis;
                            PrecipitationChart.HorizontalAxis.Title = "Date";
                        }
                        else
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            PrecipitationChart.HorizontalAxis = dateTimeAxis;
                            PrecipitationChart.HorizontalAxis.Title = "Days";
                        }
                        series4.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series4.ValueBinding = new PropertyNameDataPointBinding("Value");
                        PrecipitationChart.Series.Add(series4);

                        SurfacePollutantsChart.Series.Clear();
                        CategoricalSeries series5 = new LineSeries();
                        series5.ItemsSource = cd.getSurfacePollutantsPoints();
                        surfacePollutantPoints = cd.getSurfacePollutantsPoints();
                        if (showMonths)
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 3;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                            dateTimeAxis.LabelFormat = "MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            SurfacePollutantsChart.HorizontalAxis = dateTimeAxis;
                            SurfacePollutantsChart.HorizontalAxis.Title = "Date";
                        }
                        else
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd";
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            SurfacePollutantsChart.HorizontalAxis = dateTimeAxis;
                            SurfacePollutantsChart.HorizontalAxis.Title = "Days";
                        }
                        series5.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series5.ValueBinding = new PropertyNameDataPointBinding("Value");
                        SurfacePollutantsChart.Series.Add(series5);

                        TimeOfWetnessChart.Series.Clear();
                        CategoricalSeries series6 = new LineSeries();
                        series6.ItemsSource = cd.getTimeofWetnessPoints();
                        timeOfWetnessPoints = cd.getTimeofWetnessPoints();
                        if (showMonths)
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd MMM yyyy";
                            dateTimeAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            TimeOfWetnessChart.HorizontalAxis = dateTimeAxis;
                            TimeOfWetnessChart.HorizontalAxis.Title = "Date";
                        }
                        else
                        {
                            DateTimeContinuousAxis dateTimeAxis = new DateTimeContinuousAxis();
                            dateTimeAxis.MajorStep = 1;
                            dateTimeAxis.MajorStepUnit = Telerik.Charting.TimeInterval.Day;
                            dateTimeAxis.LabelFormat = "dd";
                            dateTimeAxis.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                            TimeOfWetnessChart.HorizontalAxis = dateTimeAxis;
                            TimeOfWetnessChart.HorizontalAxis.Title = "Days";
                        }
                        series6.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series6.ValueBinding = new PropertyNameDataPointBinding("Value");
                        TimeOfWetnessChart.Series.Add(series6);

                        PitDistributionChart.Series.Clear();
                        CategoricalSeries series7 = new BarSeries();
                        series7.ItemsSource = cd.getPitPoints();
                        pitPoints = cd.getPitPoints();
                        series7.CategoryBinding = new PropertyNameDataPointBinding("Value");
                        series7.ValueBinding = new PropertyNameDataPointBinding("Series");
                        PitDistributionChart.Series.Add(series7);

                        SteelWeightLossChart.Series.Clear();
                        ScatterPointSeries series8 = new ScatterPointSeries();
                        series8.ItemsSource = cd.getSteelLossPoints();
                        steelLossPoints = cd.getSteelLossPoints();
                        series8.XValueBinding = new PropertyNameDataPointBinding() { PropertyName = "Series" };
                        series8.YValueBinding = new PropertyNameDataPointBinding() { PropertyName = "Value" };
                        series8.Name = "Model";
                        series8.DisplayName = "Model";
                        LegendItem newItem1 = new LegendItem();
                        newItem1.Title = "Model";
                        newItem1.MarkerFill = new System.Windows.Media.SolidColorBrush(Colors.Green);
                        SteelWeightLossLegend.Items = new LegendItemCollection();
                        SteelWeightLossLegend.Items.Add(newItem1);
                        SteelWeightLossChart.Series.Add(series8);

                        SteelCorrChart.Series.Clear();
                        SteelCorrChart.VerticalAxis.Title = "\u03BC" + "m";
                        ScatterPointSeries series9 = new ScatterPointSeries();
                        series9.ItemsSource = cd.Corr1Values;
                        steelCorr1Points = cd.Corr1Values;
                        series9.XValueBinding = new PropertyNameDataPointBinding("Series");
                        series9.YValueBinding = new PropertyNameDataPointBinding("Value");
                        series9.Name = "Sensor1";
                        series9.DisplayName = "Sensor1";
                        LegendItem newItem2 = new LegendItem();
                        newItem2.Title = "Sensor1";
                        newItem2.MarkerFill = new SolidColorBrush(Colors.Green);
                        SteelCorrLegend.Items = new LegendItemCollection();
                        SteelCorrLegend.Items.Add(newItem2);
                        SteelCorrChart.Series.Add(series9);

                        ScatterPointSeries series10 = new ScatterPointSeries();
                        series10.ItemsSource = cd.Corr2Values;
                        steelCorr2Points = cd.Corr2Values;
                        series10.XValueBinding = new PropertyNameDataPointBinding("Series");
                        series10.YValueBinding = new PropertyNameDataPointBinding("Value");
                        series10.Name = "Sensor2";
                        series10.DisplayName = "Sensor2";
                        LegendItem newItem3 = new LegendItem();
                        newItem3.Title = "Sensor2";
                        newItem3.MarkerFill = new SolidColorBrush(Colors.Blue);
                        SteelCorrLegend.Items.Add(newItem3);
                        SteelCorrChart.Series.Add(series10);

                        ParameterTree.Items.Clear();
                        ParameterTree.Items.Add(new RadTreeViewItem() { Header = "Timespan Analysis", Items = { new RadTreeViewItem() { Header = "Timespan", Items = { new RadTreeViewItem() { Header = "Start Date: " + timespanAnalysisWindow.start.ToShortDateString() }, new RadTreeViewItem() { Header = "End Date: " + timespanAnalysisWindow.end.ToShortDateString() } } }, new RadTreeViewItem() { Header = "Location", Items = { new RadTreeViewItem() { Header = timespanAnalysisWindow.locationString } } } } });


                        FleetAnalysisPane.Visibility = Visibility.Hidden;
                        DropletsPane.Visibility = Visibility.Visible;
                        RelativeHumidityPane.Visibility = Visibility.Visible;
                        TemperaturePane.Visibility = Visibility.Visible;
                        PrecipitationPane.Visibility = Visibility.Visible;
                        SurfacePollutantsPane.Visibility = Visibility.Visible;
                        TimeOfWetnessPane.Visibility = Visibility.Visible;
                        PitDistributionPane.Visibility = Visibility.Visible;
                        SteelWeightLossPane.Visibility = Visibility.Visible;
                        SteelCorrPane.Visibility = Visibility.Visible;
                        FleetAnalysisPane.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in errorMessages)
                        {
                            sb.Append(s);
                            sb.Append("\n");
                        }
                        MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    if(errorMessages.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in errorMessages)
                        {
                            sb.Append(s);
                            sb.Append("\n");
                        }
                        MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Error encountered during analysis", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                ToggleLoadingVisability(false);
            }
        }




        private void RibbonSingleClick(object sender, RoutedEventArgs e)
        {
            errorMessages.Clear();
            DropletsPane.Visibility = Visibility.Collapsed;
            RelativeHumidityPane.Visibility = Visibility.Collapsed;
            TemperaturePane.Visibility = Visibility.Collapsed;
            PrecipitationPane.Visibility = Visibility.Collapsed;
            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
            PitDistributionPane.Visibility = Visibility.Collapsed;
            SteelWeightLossPane.Visibility = Visibility.Collapsed;
            FleetAnalysisPane.Visibility = Visibility.Collapsed;
            SteelCorrPane.Visibility = Visibility.Collapsed;
            DescriptionPane.IsActive = true;

            List<t_Location> locations = new List<t_Location>();
            using (var context = new CorrosionModelEntities())
            {
                locations = context.t_Location.ToList();
            }

            var SingleAnalysisWindow = new SingleAnalysis(locations);
            SingleAnalysisWindow.ShowDialog();
            if(SingleAnalysisWindow.canceled == false)
            {
                try
                {
                    ToggleLoadingVisability(true);

                    IncrementLoadingBar("Running Analysis", 30);


                    cd = singleAnalysisFromFileCombined(SingleAnalysisWindow.selectedFileName, SingleAnalysisWindow.locationValue);

                    if (errorMessages.Count == 0)
                    {

                        DropletsChart.Series.Clear();
                        CategoricalSeries series = new LineSeries();
                        series.ItemsSource = cd.getDropletsPoints(true);
                        dropletPoints = cd.getDropletsPoints(false);
                        CategoricalAxis dateTimeAxis = new CategoricalAxis();
                        dateTimeAxis.SmartLabelsMode = Telerik.Charting.AxisSmartLabelsMode.SmartStep;
                        dateTimeAxis.MajorTickInterval = 50;
                        DropletsChart.HorizontalAxis = dateTimeAxis;
                        series.CategoryBinding = new PropertyNameDataPointBinding("Y");
                        series.ValueBinding = new PropertyNameDataPointBinding("Value");
                        DropletsChart.Series.Add(series);

                        TemperatureChart.Series.Clear();
                        CategoricalSeries series2 = new LineSeries();
                        series2.ItemsSource = cd.getTemperaturePoints();
                        temperaturePoints = cd.getTemperaturePoints();
                        DateTimeContinuousAxis dateTimeAxis2 = new DateTimeContinuousAxis();
                        dateTimeAxis2.MajorStep = 3;
                        dateTimeAxis2.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                        dateTimeAxis2.LabelFormat = "MMM yyyy";
                        dateTimeAxis2.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                        dateTimeAxis2.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                        TemperatureChart.HorizontalAxis = dateTimeAxis;
                        TemperatureChart.HorizontalAxis.Title = "Date";
                        series2.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series2.ValueBinding = new PropertyNameDataPointBinding("Value");
                        TemperatureChart.Series.Add(series2);

                        RelativeHumidityChart.Series.Clear();
                        CategoricalSeries series3 = new LineSeries();
                        series3.ItemsSource = cd.getRelativeHumidityPoints();
                        realativeHumidityPoints = cd.getRelativeHumidityPoints();
                        DateTimeContinuousAxis dateTimeAxis3 = new DateTimeContinuousAxis();
                        dateTimeAxis3.MajorStep = 3;
                        dateTimeAxis3.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                        dateTimeAxis3.LabelFormat = "MMM yyyy";
                        dateTimeAxis3.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                        dateTimeAxis3.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                        RelativeHumidityChart.HorizontalAxis = dateTimeAxis;
                        RelativeHumidityChart.HorizontalAxis.Title = "Date";
                        series3.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series3.ValueBinding = new PropertyNameDataPointBinding("Value");
                        RelativeHumidityChart.Series.Add(series3);

                        PrecipitationChart.Series.Clear();
                        CategoricalSeries series4 = new LineSeries();
                        series4.ItemsSource = cd.getPrecipitationPoints();
                        precipitationPoints = cd.getPrecipitationPoints();
                        DateTimeContinuousAxis dateTimeAxis4 = new DateTimeContinuousAxis();
                        dateTimeAxis4.MajorStep = 3;
                        dateTimeAxis4.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                        dateTimeAxis4.LabelFormat = "MMM yyyy";
                        dateTimeAxis4.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                        dateTimeAxis4.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                        PrecipitationChart.HorizontalAxis = dateTimeAxis;
                        PrecipitationChart.HorizontalAxis.Title = "Date";
                        series4.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series4.ValueBinding = new PropertyNameDataPointBinding("Value");
                        PrecipitationChart.Series.Add(series4);

                        SurfacePollutantsChart.Series.Clear();
                        CategoricalSeries series5 = new LineSeries();
                        series5.ItemsSource = cd.getSurfacePollutantsPoints();
                        surfacePollutantPoints = cd.getSurfacePollutantsPoints();
                        DateTimeContinuousAxis dateTimeAxis5 = new DateTimeContinuousAxis();
                        dateTimeAxis5.MajorStep = 3;
                        dateTimeAxis5.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                        dateTimeAxis5.LabelFormat = "MMM yyyy";
                        dateTimeAxis5.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                        dateTimeAxis5.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                        SurfacePollutantsChart.HorizontalAxis = dateTimeAxis;
                        SurfacePollutantsChart.HorizontalAxis.Title = "Date";
                        series5.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series5.ValueBinding = new PropertyNameDataPointBinding("Value");
                        SurfacePollutantsChart.Series.Add(series5);

                        TimeOfWetnessChart.Series.Clear();
                        CategoricalSeries series6 = new LineSeries();
                        series6.ItemsSource = cd.getTimeofWetnessPoints();
                        timeOfWetnessPoints = cd.getTimeofWetnessPoints();
                        DateTimeContinuousAxis dateTimeAxis6 = new DateTimeContinuousAxis();
                        dateTimeAxis6.MajorStep = 3;
                        dateTimeAxis6.MajorStepUnit = Telerik.Charting.TimeInterval.Month;
                        dateTimeAxis6.LabelFormat = "MMM yyyy";
                        dateTimeAxis6.LabelFitMode = Telerik.Charting.AxisLabelFitMode.MultiLine;
                        dateTimeAxis6.PlotMode = Telerik.Charting.AxisPlotMode.OnTicks;
                        TimeOfWetnessChart.HorizontalAxis = dateTimeAxis;
                        TimeOfWetnessChart.HorizontalAxis.Title = "Date";
                        series6.CategoryBinding = new PropertyNameDataPointBinding("Date");
                        series6.ValueBinding = new PropertyNameDataPointBinding("Value");
                        TimeOfWetnessChart.Series.Add(series6);

                        PitDistributionChart.Series.Clear();
                        CategoricalSeries series7 = new BarSeries();
                        series7.ItemsSource = cd.getPitPoints();
                        pitPoints = cd.getPitPoints();
                        series7.CategoryBinding = new PropertyNameDataPointBinding("Value");
                        series7.ValueBinding = new PropertyNameDataPointBinding("Series");
                        PitDistributionChart.Series.Add(series7);

                        SteelWeightLossChart.Series.Clear();
                        ScatterPointSeries series8 = new ScatterPointSeries();
                        series8.ItemsSource = cd.getSteelLossPoints();
                        steelLossPoints = cd.getSteelLossPoints();
                        series8.XValueBinding = new PropertyNameDataPointBinding("Series");
                        series8.YValueBinding = new PropertyNameDataPointBinding("Value");
                        SteelWeightLossChart.Series.Add(series8);

                        ParameterTree.Items.Clear();
                        ParameterTree.Items.Add(new RadTreeViewItem() { Header = "Single Analysis", Items = { new RadTreeViewItem() { Header = "File", Items = { new RadTreeViewItem() { Header = SingleAnalysisWindow.selectedFileName } } }, new RadTreeViewItem() { Header = "Location", Items = { new RadTreeViewItem() { Header = SingleAnalysisWindow.locationString } } } } });


                        FleetAnalysisPane.Visibility = Visibility.Hidden;
                        DropletsPane.Visibility = Visibility.Visible;
                        RelativeHumidityPane.Visibility = Visibility.Visible;
                        TemperaturePane.Visibility = Visibility.Visible;
                        PrecipitationPane.Visibility = Visibility.Visible;
                        SurfacePollutantsPane.Visibility = Visibility.Visible;
                        TimeOfWetnessPane.Visibility = Visibility.Visible;
                        PitDistributionPane.Visibility = Visibility.Visible;
                        SteelWeightLossPane.Visibility = Visibility.Visible;
                        SteelCorrPane.Visibility = Visibility.Collapsed;
                        FleetAnalysisPane.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in errorMessages)
                        {
                            sb.Append(s);
                            sb.Append("\n");
                        }
                        MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    if (errorMessages.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in errorMessages)
                        {
                            sb.Append(s);
                            sb.Append("\n");
                        }
                        MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Error encountered during analysis", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                ToggleLoadingVisability(false);
            }
        }

        private void RibbonLongTermClick(object sender, RoutedEventArgs e)
        {

            DropletsPane.Visibility = Visibility.Collapsed;
            RelativeHumidityPane.Visibility = Visibility.Collapsed;
            TemperaturePane.Visibility = Visibility.Collapsed;
            PrecipitationPane.Visibility = Visibility.Collapsed;
            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
            PitDistributionPane.Visibility = Visibility.Collapsed;
            SteelWeightLossPane.Visibility = Visibility.Collapsed;
            FleetAnalysisPane.Visibility = Visibility.Collapsed;
            SteelCorrPane.Visibility = Visibility.Collapsed;
            DescriptionPane.IsActive = true;

            errorMessages.Clear();
            try
            {
                List<string> allTails = GetAllTails();
                var LongTermWindow = new LongTermAnalysis(allTails);
                LongTermWindow.ShowDialog();

                if (!LongTermWindow.canceled)
                {
                    if (!LongTermWindow.fleetAnalysis)
                    {
                        ToggleLoadingVisability(true);
                        string selectedTail = LongTermWindow.selectedTail.ToString();
                        DateTime StartDate = (DateTime)LongTermWindow.DiagnosticStart;
                        DateTime EndDate = (DateTime)LongTermWindow.DiagnosticEnd;

                        IncrementLoadingBar("Running Analysis", 30);

                        cd = multiLocationAnalysis(true, selectedTail, StartDate, EndDate);

                        IncrementLoadingBar("Generating Charts", 80);

                        if (errorMessages.Count == 0)
                        {

                            DropletsChart.Series.Clear();
                            CategoricalSeries series = new LineSeries();
                            series.ItemsSource = cd.getDropletsPoints(true);
                            dropletPoints = cd.getDropletsPoints(false);
                            series.CategoryBinding = new PropertyNameDataPointBinding("Y");
                            series.ValueBinding = new PropertyNameDataPointBinding("Value");
                            DropletsChart.Series.Add(series);

                            TemperatureChart.Series.Clear();
                            CategoricalSeries series2 = new LineSeries();
                            series2.ItemsSource = cd.getTemperaturePoints();
                            temperaturePoints = cd.getTemperaturePoints();
                            series2.CategoryBinding = new PropertyNameDataPointBinding("Date");
                            series2.ValueBinding = new PropertyNameDataPointBinding("Value");
                            TemperatureChart.Series.Add(series2);

                            RelativeHumidityChart.Series.Clear();
                            CategoricalSeries series3 = new LineSeries();
                            series3.ItemsSource = cd.getRelativeHumidityPoints();
                            realativeHumidityPoints = cd.getRelativeHumidityPoints();
                            series3.CategoryBinding = new PropertyNameDataPointBinding("Date");
                            series3.ValueBinding = new PropertyNameDataPointBinding("Value");
                            RelativeHumidityChart.Series.Add(series3);

                            PrecipitationChart.Series.Clear();
                            CategoricalSeries series4 = new LineSeries();
                            series4.ItemsSource = cd.getPrecipitationPoints();
                            precipitationPoints = cd.getPrecipitationPoints();
                            series4.CategoryBinding = new PropertyNameDataPointBinding("Date");
                            series4.ValueBinding = new PropertyNameDataPointBinding("Value");
                            PrecipitationChart.Series.Add(series4);

                            SurfacePollutantsChart.Series.Clear();
                            CategoricalSeries series5 = new LineSeries();
                            series5.ItemsSource = cd.getSurfacePollutantsPoints();
                            surfacePollutantPoints = cd.getSurfacePollutantsPoints();
                            series5.CategoryBinding = new PropertyNameDataPointBinding("Date");
                            series5.ValueBinding = new PropertyNameDataPointBinding("Value");
                            SurfacePollutantsChart.Series.Add(series5);

                            TimeOfWetnessChart.Series.Clear();
                            CategoricalSeries series6 = new LineSeries();
                            series6.ItemsSource = cd.getTimeofWetnessPoints();
                            timeOfWetnessPoints = cd.getTimeofWetnessPoints();
                            series6.CategoryBinding = new PropertyNameDataPointBinding("Date");
                            series6.ValueBinding = new PropertyNameDataPointBinding("Value");
                            TimeOfWetnessChart.Series.Add(series6);

                            PitDistributionChart.Series.Clear();
                            CategoricalSeries series7 = new BarSeries();
                            series7.ItemsSource = cd.getPitPoints();
                            pitPoints = cd.getPitPoints();
                            series7.CategoryBinding = new PropertyNameDataPointBinding("Value");
                            series7.ValueBinding = new PropertyNameDataPointBinding("Series");
                            PitDistributionChart.Series.Add(series7);

                            SteelWeightLossChart.Series.Clear();
                            ScatterPointSeries series8 = new ScatterPointSeries();
                            series8.ItemsSource = cd.getSteelLossPoints();
                            steelLossPoints = cd.getSteelLossPoints();
                            series8.XValueBinding = new PropertyNameDataPointBinding("Series");
                            series8.YValueBinding = new PropertyNameDataPointBinding("Value");
                            SteelWeightLossChart.Series.Add(series8);

                            ParameterTree.Items.Clear();
                            RadTreeViewItem materialItem = new RadTreeViewItem() { Header = "Materials" };
                            foreach (string item in LongTermWindow.materials)
                            {
                                materialItem.Items.Add(new RadTreeViewItem() { Header = item });
                            }

                            RadTreeViewItem dateItem = new RadTreeViewItem() { Header = "Diagnostic Analysis" };
                            foreach (string item in baseNamesAndTimeStamps)
                            {
                                dateItem.Items.Add(new RadTreeViewItem() { Header = item });
                            }

                            ParameterTree.Items.Add(new RadTreeViewItem() { Header = "Long Term Analysis", Items = { new RadTreeViewItem() { Header = "MDS", Items = { new RadTreeViewItem() { Header = LongTermWindow.MDS } } }, new RadTreeViewItem() { Header = "Scenario Type", Items = { new RadTreeViewItem() { Header = LongTermWindow.scenarioType } } }, new RadTreeViewItem() { Header = "Selected Tail", Items = { new RadTreeViewItem() { Header = LongTermWindow.selectedTail } } }, materialItem, dateItem } });

                            DropletsPane.Visibility = Visibility.Visible;
                            RelativeHumidityPane.Visibility = Visibility.Visible;
                            TemperaturePane.Visibility = Visibility.Visible;
                            PrecipitationPane.Visibility = Visibility.Visible;
                            SurfacePollutantsPane.Visibility = Visibility.Visible;
                            TimeOfWetnessPane.Visibility = Visibility.Visible;
                            PitDistributionPane.Visibility = Visibility.Visible;
                            SteelWeightLossPane.Visibility = Visibility.Visible;
                            SteelCorrPane.Visibility = Visibility.Collapsed;
                            FleetAnalysisPane.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in errorMessages)
                            {
                                sb.Append(s);
                                sb.Append("\n");
                            }
                            MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (LongTermWindow.fleetAnalysis)
                    {
                        ToggleLoadingVisability(true);

                        List<string> selectedTails = new List<string>();
                        foreach (string item in LongTermWindow.selectedTails)
                        {
                            selectedTails.Add(item);
                        }

                        DateTime StartDate = (DateTime)LongTermWindow.DiagnosticStart;
                        DateTime EndDate = (DateTime)LongTermWindow.DiagnosticEnd;

                        MultiLocationAnalysis ml = new MultiLocationAnalysis();
                        List<MultiTailDataObject> ObjectSource = new List<MultiTailDataObject>();
                        if (errorMessages.Count == 0)
                        {

                            int indexId = 1;
                            RadTreeViewItem dateItem = new RadTreeViewItem() { Header = "Diagnostic Analysis" };
                            foreach (string Tail in selectedTails)
                            {
                                var list = (from t in db.t_History
                                            where t.Tail_Number == Tail && StartDate <= t.Departure_Time && EndDate >= t.Arrival_Time
                                            select t).OrderBy(x => x.Arrival_Time);
                                SingleAnalysisFunctions cm = new SingleAnalysisFunctions();
                                List<AnalysisTask> tasks = new List<AnalysisTask>();
                                foreach (t_History his in list)
                                {
                                    AnalysisTask convertedHistory = new AnalysisTask();
                                    if (his.Arrival_Time.Date < StartDate)
                                    {
                                        convertedHistory.StartDate = StartDate;
                                    }
                                    else
                                    {
                                        convertedHistory.StartDate = his.Arrival_Time.Date;
                                    }
                                    if (his.Departure_Time.Date > EndDate)
                                    {
                                        convertedHistory.EndDate = EndDate;
                                    }
                                    else
                                    {
                                        convertedHistory.EndDate = his.Departure_Time.Date;
                                    }
                                    convertedHistory.Location = his.Location;
                                    convertedHistory.WashFreq = 0;
                                    convertedHistory.DetergentWash = false;
                                    convertedHistory.MetDataType = MetDataTypes.Meteorological;
                                    convertedHistory.CrystalSize = 6;
                                    convertedHistory.LocationString = GetLocationById(his.Location);
                                    tasks.Add(convertedHistory);
                                }
                                int numTasks = tasks.Count();
                                cd.BaseNames = new string[numTasks];
                                cd.cutoffValues = new int[numTasks];
                                cd.cutoffValuesDroplets = new int[numTasks];


                                int wpIndex = 1;
                                int guamIndex = 1;
                                int tinkerIndex = 1;
                                int eglinIndex = 1;
                                int hickamIndex = 1;
                                int hillIndex = 1;
                                int lukeIndex = 1;
                                int kwIndex = 1;
                                int robinIndex = 1;

                                int index = 0;
                                baseNamesAndTimeStamps.Clear();
                                foreach (AnalysisTask task in tasks)
                                {
                                    if (task.LocationString == "Wright Patterson Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Wright Patt" + wpIndex))
                                        {
                                            wpIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Wright Patt" + wpIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Guam International Airport")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Guam" + guamIndex))
                                        {
                                            guamIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Guam" + guamIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Tinker Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Tinker" + tinkerIndex))
                                        {
                                            tinkerIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Tinker" + tinkerIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Eglin Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Eglin" + eglinIndex))
                                        {
                                            eglinIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Eglin" + eglinIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Hickam Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Hickam" + hickamIndex))
                                        {
                                            hickamIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Hickam" + hickamIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Hill Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Hill" + hillIndex))
                                        {
                                            hillIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Hill" + hillIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Luke Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Luke" + lukeIndex))
                                        {
                                            lukeIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Luke" + lukeIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Naval Air Station Key West")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Key West" + kwIndex))
                                        {
                                            kwIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Key West" + kwIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else if (task.LocationString == "Robins Air Force Base")
                                    {
                                        if (Array.Exists(cd.BaseNames, element => element == "Robins" + robinIndex))
                                        {
                                            robinIndex++;
                                        }
                                        baseNamesAndTimeStamps.Add("Robins" + robinIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    else
                                    {
                                        baseNamesAndTimeStamps.Add(task.LocationString + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                                    }
                                    index++;
                                }



                                int value = ml.BeginMultiTailAnalysis(StartDate, EndDate);
                                MultiTailDataObject m = new MultiTailDataObject();
                                m.Id = indexId;
                                m.MDS = "F-15";
                                m.TailNumber = Tail;
                                m.LocalizedCorrosion = value;
                                m.FatigueDamage = value;
                                ObjectSource.Add(m);
                                indexId++;

                                RadTreeViewItem singleDateItem = new RadTreeViewItem() { Header = Tail };
                                foreach (string item in baseNamesAndTimeStamps)
                                {
                                    singleDateItem.Items.Add(new RadTreeViewItem() { Header = item });
                                }
                                dateItem.Items.Add(singleDateItem);
                            }
                            FleetGridView.ItemsSource = ObjectSource;

                            ParameterTree.Items.Clear();

                            RadTreeViewItem materialItem = new RadTreeViewItem() { Header = "Materials" };
                            foreach (string item in LongTermWindow.materials)
                            {
                                materialItem.Items.Add(new RadTreeViewItem() { Header = item });
                            }
                            RadTreeViewItem tailItem = new RadTreeViewItem() { Header = "Selected Tails" };
                            foreach (string item in LongTermWindow.selectedTails)
                            {
                                tailItem.Items.Add(new RadTreeViewItem() { Header = item });
                            }
                            ParameterTree.Items.Add(new RadTreeViewItem() { Header = "Fleet Analysis", Items = { new RadTreeViewItem() { Header = "MDS", Items = { new RadTreeViewItem() { Header = LongTermWindow.MDS } } }, new RadTreeViewItem() { Header = "Scenario Type", Items = { new RadTreeViewItem() { Header = LongTermWindow.scenarioType } } }, tailItem, materialItem, dateItem } });

                            DropletsPane.Visibility = Visibility.Collapsed;
                            RelativeHumidityPane.Visibility = Visibility.Collapsed;
                            TemperaturePane.Visibility = Visibility.Collapsed;
                            PrecipitationPane.Visibility = Visibility.Collapsed;
                            SurfacePollutantsPane.Visibility = Visibility.Collapsed;
                            TimeOfWetnessPane.Visibility = Visibility.Collapsed;
                            PitDistributionPane.Visibility = Visibility.Collapsed;
                            SteelWeightLossPane.Visibility = Visibility.Collapsed;
                            FleetAnalysisPane.Visibility = Visibility.Visible;
                            SteelCorrPane.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in errorMessages)
                            {
                                sb.Append(s);
                                sb.Append("\n");
                            }
                            MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {

                    }
                }

            }catch(Exception sqlError){
                if (sqlError.InnerException != null)
                {
                   errorMessages.Add("Error encountered during analysis, Exact error message was: " + sqlError.InnerException.Message);
                }
                else
                {
                    errorMessages.Add("Error encountered during analysis, Exact error message was: " + sqlError.Message);
                }
                StringBuilder sb = new StringBuilder();
                foreach (string s in errorMessages)
                {
                    sb.Append(s);
                    sb.Append("\n");
                }
                MessageBoxResult result = MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            ToggleLoadingVisability(false);
        }

        private void IncrementLoadingBar(string step, int value)
        {
            Dispatcher.Invoke(DispatcherPriority.Loaded,
            (Action)(() =>
            {
                LoadingLabel.Content = step;
                PercentageLabel.Content = value + "%";
                LoadingBar.Value = value;
            }));

        }

        private void ToggleLoadingVisability(bool visable)
        {
            Dispatcher.Invoke(DispatcherPriority.Loaded,
            (Action)(() =>
                {
                    if (visable)
                    {
                        this.InstructionText.Visibility = Visibility.Hidden;
                        this.InstructionTextTwo.Visibility = Visibility.Hidden;
                        this.InstructionTextThree.Visibility = Visibility.Hidden;
                        this.InstructionTextFour.Visibility = Visibility.Hidden;
                        //this.InstructionTextFive.Visibility = Visibility.Hidden;
                        this.LoadingLabel.Visibility = Visibility.Visible;
                        this.PercentageLabel.Visibility = Visibility.Visible;
                        this.LoadingBar.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LoadingLabel.Content = "Reading File";
                        PercentageLabel.Content = "10%";
                        LoadingBar.Value = 10;
                        this.LoadingLabel.Visibility = Visibility.Hidden;
                        this.PercentageLabel.Visibility = Visibility.Hidden;
                        this.LoadingBar.Visibility = Visibility.Hidden;
                        this.InstructionText.Visibility = Visibility.Visible;
                        this.InstructionTextTwo.Visibility = Visibility.Visible;
                        this.InstructionTextThree.Visibility = Visibility.Visible;
                        this.InstructionTextFour.Visibility = Visibility.Visible;
                        //this.InstructionTextFive.Visibility = Visibility.Visible;
                    }
                }));
        }

        public CorrosionData singleAnalysisFromDate(bool entireRange, DateTime start, DateTime end, int location)
        {
            var dataList = new List<t_Sensor_Data>();

            using (var context = new CorrosionModelEntities())
            {
                if (!entireRange){
                    dataList = context
                    .t_Sensor_Data
                    .Where(x => x.Timestamp >= start && x.Timestamp <= end && x.Location == location)
                    .ToList();
                }
                else
                {
                    dataList = context
                    .t_Sensor_Data
                    .Where(x => x.Location == location)
                    .ToList();
                }

                //foreach (t_Sensor_Data data in dataList)
                //{
                //    Random random = new Random();
                //    if(data.Timestamp.Month < 3)
                //    {
                //        data.TC1 = random.NextDouble() * (25 - 18) + 18;
                //    }
                //    else if (data.Timestamp.Month < 5)
                //    {
                //        data.TC1 = random.NextDouble() * (30 - 20) + 20;
                //    }
                //    else if (data.Timestamp.Month < 8)
                //    {
                //        data.TC1 = random.NextDouble() * (40 - 30) + 30;
                //    }
                //    else if (data.Timestamp.Month < 11)
                //    {
                //        data.TC1 = random.NextDouble() * (30 - 20) + 20;
                //    }
                //    else
                //    {
                //        data.TC1 = random.NextDouble() * (25 - 18) + 18;
                //    }
                //    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                //}
                //context.SaveChanges();
            }

            
            List<CombinedDataViewModel> combinedDataList = new List<CombinedDataViewModel>();

            int i = 0;
            foreach(t_Sensor_Data data in dataList)
            {
                CombinedDataViewModel cdvm = new CombinedDataViewModel();
                cdvm.ID = i;
                cdvm.TOWSensor = data.TOW;
                cdvm.TOWCalc = 0;
                cdvm.SurfaceTemp = data.TC2;
                cdvm.TimeInterval = 60;
                cdvm.ATM_RH = data.SHTRH;
                cdvm.ATM_Temp = data.SHTTemp;
                cdvm.Rain = Convert.ToDouble(data.Rain);
                cdvm.Timestamp = data.Timestamp;
                cdvm.Month = data.Timestamp.Month;
                cdvm.DataSourceType = 1;
                cdvm.corr1 = data.Corr1;
                cdvm.corr2 = data.Corr2;
                cdvm.corrTemp = data.TC1;
                combinedDataList.Add(cdvm);

                i++;
            }

            if (errorMessages.Count == 0 || (errorMessages.Count != 0 && dataList.Count != 0))
            {
                errorMessages.Clear();
                SingleAnalysisFunctions cm = new SingleAnalysisFunctions();
                Dictionary<string, PollutionModel> pollutionData = getAllPollutionValues();
                

                try
                {
                    IncrementLoadingBar("Running Analysis", 30);
                    CorrosionData cdt = cm.BeginAnalysisSingle(combinedDataList, location, pollutionData, true);
                    cdt.BaseNames[0] = "Custom Location";
                    cdt.fileTitle = "Timespan Analysis";
                    IncrementLoadingBar("Finalizing", 80);
                    return cdt;

                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        if (combinedDataList.Count != 0)
                        {
                            errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.InnerException.Message);
                        }
                        else
                        {
                            errorMessages.Add("Data could not be processed for selected analysis");
                        }
                    }
                    else
                    {
                        if (combinedDataList.Count != 0)
                        {
                            errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.Message);
                        }
                        else
                        {
                            errorMessages.Add("Data could not be processed for selected analysis");
                        }
                    }
                }
            }
            return cd;
        }

        public CorrosionData singleAnalysisFromFileCombined(string fileName, int location)
        {

            string result = string.Empty;

            string extention = System.IO.Path.GetExtension(System.IO.Path.GetFileName(fileName));
            List<CombinedDataViewModel> fileData = new List<CombinedDataViewModel>();
            string title = "";
            if (extention == ".csv" || extention == ".txt")
            {
                result = new StreamReader(fileName).ReadToEnd();
                string fixedResult = result.Replace('\r', ' ');
                string[] allLines = fixedResult.Split('\n');
                int linenumber = 1;
                int savedMonth = 0;
                foreach (string lineText in allLines)
                {
                    if (linenumber >= 3)
                    {
                        lineText.TrimEnd(',');
                        string trimmedLineText = lineText.TrimEnd(',', ' ');
                        string[] lineValues = trimmedLineText.Split(',');
                        if (lineValues.Length == 11 && (lineValues[0].TrimEnd(' ') != "0" || linenumber == 3))//7 items per row
                        {
                            try
                            {
                                CombinedDataViewModel s = new CombinedDataViewModel();
                                if (lineValues[0].TrimEnd(' ') != "0" && lineValues[0] != "NULL" && lineValues[0].TrimEnd(' ') != "" && !lineValues[0].Contains("#"))
                                {
                                    s.Record_Number = Convert.ToInt32(lineValues[0].TrimEnd(' '));
                                }
                                else
                                {
                                    s.Record_Number = 0;
                                }
                                if (lineValues[1].TrimEnd(' ') != "0" && lineValues[1] != "NULL" && lineValues[1].TrimEnd(' ') != "" && !lineValues[1].Contains("#"))
                                {
                                    s.TOWSensor = Convert.ToDouble(lineValues[1].TrimEnd(' '));
                                }
                                else
                                {
                                    s.TOWSensor = 0;
                                }
                                if (lineValues[2].TrimEnd(' ') != "0" && lineValues[2] != "NULL" && lineValues[2].TrimEnd(' ') != "" && !lineValues[2].Contains("#"))
                                {
                                    s.TOWCalc = Convert.ToDouble(lineValues[2].TrimEnd(' '));
                                }
                                else
                                {
                                    s.TOWCalc = 0;
                                }
                                if (lineValues[3].TrimEnd(' ') != "0" && lineValues[3] != "NULL" && lineValues[3].TrimEnd(' ') != "" && !lineValues[3].Contains("#"))
                                {
                                    s.SurfaceTemp = Convert.ToDouble(lineValues[3]);
                                }
                                else
                                {
                                    s.SurfaceTemp = 0;
                                }
                                if (lineValues[4].TrimEnd(' ') != "0" && lineValues[4] != "NULL" && lineValues[4].TrimEnd(' ') != "" && !lineValues[4].Contains("#"))
                                {
                                    s.TimeInterval = Convert.ToInt32(lineValues[4].TrimEnd(' '));
                                }
                                else
                                {
                                    s.TimeInterval = 0;
                                }
                                if (lineValues[5].TrimEnd(' ') != "0" && lineValues[5] != "NULL" && lineValues[5].TrimEnd(' ') != "" && !lineValues[5].Contains("#"))
                                {
                                    s.ATM_RH = Convert.ToDouble(lineValues[5].TrimEnd(' '));
                                }
                                else
                                {
                                    s.ATM_RH = 0;
                                }
                                if (lineValues[6].TrimEnd(' ') != "0" && lineValues[6] != "NULL" && lineValues[6].TrimEnd(' ') != "" && !lineValues[6].Contains("#"))
                                {
                                    s.ATM_Temp = Convert.ToDouble(lineValues[6].TrimEnd(' '));
                                }
                                else
                                {
                                    s.ATM_Temp = 0;
                                }
                                if (lineValues[7].TrimEnd(' ') != "0" && lineValues[7] != "NULL" && lineValues[7].TrimEnd(' ') != "" && !lineValues[7].Contains("#"))
                                {
                                    s.Rain = Convert.ToDouble(lineValues[7].TrimEnd(' '));
                                }
                                else
                                {
                                    s.Rain = 0;
                                }
                                if (lineValues[8].TrimEnd(' ') != "" && lineValues[8] != "NULL" && lineValues[8].TrimEnd(' ') != "" && !lineValues[8].Contains("#"))
                                {
                                    s.Timestamp = Convert.ToDateTime(lineValues[8].TrimEnd(' '));
                                }
                                else
                                {
                                    s.Timestamp = DateTime.Now;
                                }
                                if (lineValues[9].TrimEnd(' ') != "0" && lineValues[9] != "NULL" && lineValues[9].TrimEnd(' ') != "" && !lineValues[9].Contains("#"))
                                {
                                    double catchDouble = Convert.ToDouble(lineValues[9].TrimEnd(' '));
                                    s.Month = (int)catchDouble;
                                    savedMonth = s.Month;
                                }
                                else
                                {
                                    s.Month = savedMonth;
                                }
                                if (lineValues[10].TrimEnd(' ') != "0" && lineValues[10] != "NULL" && lineValues[10].TrimEnd(' ') != "" && !lineValues[10].Contains("#"))
                                {
                                    double catchDouble = Convert.ToDouble(lineValues[10].TrimEnd(' '));
                                    s.DataSourceType = (int)catchDouble;
                                }
                                else
                                {
                                    s.DataSourceType = 1;
                                }
                                //if (lineValues[11].TrimEnd(' ') != "0" && lineValues[11] != "NULL" && lineValues[11].TrimEnd(' ') != "" && !lineValues[11].Contains("#"))
                                //{
                                //    double catchDouble = Convert.ToDouble(lineValues[11].TrimEnd(' '));
                                //    s.corrTemp = catchDouble;
                                //}
                                //else
                                //{
                                //    s.corrTemp = 30;
                                //}
                                fileData.Add(s);
                            }
                            catch (Exception e)
                            {
                                errorMessages.Add("Invalid data was encountered. Error encountered on line " + linenumber + ". Exact error message was: " + e.InnerException.Message);
                            }
                        }else if (linenumber > 3 && lineValues.Length == 1)
                        {

                        }
                        else
                        {
                            if ((lineValues.Length != 11 && lineValues[0].TrimEnd(' ') != "0") || (lineValues.Length != 11 && lineValues[0].TrimEnd(' ') != "")) {
                                errorMessages.Add("File contains an incorrect number of columns (" + lineValues.Length + "). Error encountered on line " + linenumber);
                            }
                        }
                    }
                    else if (linenumber == 1)
                    {
                        title = lineText.TrimEnd(',', ' ');
                    }
                    else if (linenumber == 2)
                    {

                    }
                    linenumber++;
                }
            }
            if (errorMessages.Count == 0 || (errorMessages.Count != 0 && fileData.Count != 0))
            {
                errorMessages.Clear();

                //int limit = 5000;
                //int lowerLimit = 0;

                //var dataList = new List<t_Sensor_Data>();

                //using (var context = new CorrosionModelEntities())
                //{
                //    dataList = context
                //    .t_Sensor_Data
                //    .ToList();

                //}


                //while (limit < fileData.Count)
                //{
                //    int i = 0;
                //    List<t_Sensor_Data> sensorDataList = new List<t_Sensor_Data>();
                //    foreach (CombinedDataViewModel cdvm in fileData)
                //    {
                //        if (cdvm.DataSourceType == 2)
                //        {

                //        }
                //        else if (i < lowerLimit)
                //        {
                //            i++;
                //        }
                //        else if (i < limit)
                //        {
                //            using (var context = new CorrosionModelEntities())
                //            {
                //                dataList[i].TC1 = cdvm.corrTemp;
                //                sensorDataList.Add(dataList[i]);
                //            }

                //            i++;
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }

                //    using (var context = new CorrosionModelEntities())
                //    {
                //        int addIndex = 0;
                //        foreach (var sensorData in sensorDataList)
                //        {
                //            context.Entry(sensorData).State = System.Data.Entity.EntityState.Modified;
                //            addIndex++;
                //        }

                //        context.SaveChanges();
                //    }
                //    limit += 5000;
                //    lowerLimit += 5000;
                //}



                SingleAnalysisFunctions cm = new SingleAnalysisFunctions();
                Dictionary<string, PollutionModel> pollutionData = getAllPollutionValues();

                try
                {
                    CorrosionData cdt = cm.BeginAnalysisSingle(fileData, location, pollutionData, false);
                    IncrementLoadingBar("Finalizing", 80);
                    cdt.BaseNames[0] = "Custom Location";
                    cdt.fileTitle = title;
                    return cdt;

                }
                catch(Exception e)
                {
                    if (e.InnerException != null)
                    {
                        if (fileData.Count != 0)
                        {
                            errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.InnerException.Message);
                        }
                        else
                        {
                            errorMessages.Add("File data could not be processed for selected analysis");
                        }
                    }
                    else
                    {
                        if (fileData.Count != 0)
                        {
                            errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.Message);
                        }
                        else
                        {
                            errorMessages.Add("File data could not be processed for selected analysis");
                        }
                    }
                }
            }
            return cd;
        }

        public CorrosionData multiLocationAnalysis(bool finalAnalysis, string selectedTail, DateTime StartDate, DateTime EndDate)
        {
            var list = (from t in db.t_History
                        where t.Tail_Number == selectedTail && StartDate <= t.Departure_Time && EndDate >= t.Arrival_Time
                        select t).OrderBy(x => x.Arrival_Time);
            SingleAnalysisFunctions cm = new SingleAnalysisFunctions();
            List<AnalysisTask> tasks = new List<AnalysisTask>();
            foreach (t_History his in list)
            {
                AnalysisTask convertedHistory = new AnalysisTask();
                if (his.Arrival_Time.Date < StartDate)
                {
                    convertedHistory.StartDate = StartDate;
                }
                else
                {
                    convertedHistory.StartDate = his.Arrival_Time.Date;
                }
                if (his.Departure_Time.Date > EndDate)
                {
                    convertedHistory.EndDate = EndDate;
                }
                else
                {
                    convertedHistory.EndDate = his.Departure_Time.Date;
                }
                convertedHistory.Location = his.Location;
                convertedHistory.WashFreq = 0;
                convertedHistory.DetergentWash = false;
                convertedHistory.MetDataType = MetDataTypes.Meteorological;
                convertedHistory.CrystalSize = 6;
                convertedHistory.LocationString = GetLocationById(his.Location);
                tasks.Add(convertedHistory);
            }
            Dictionary<string, PollutionModel> pollutionData = getAllPollutionValues();

            CorrosionData cd = new CorrosionData();
            List<CombinedDataViewModel> sensorData = new List<CombinedDataViewModel>();

            try
            {

                int wpIndex = 1;
                int guamIndex = 1;
                int tinkerIndex = 1;
                int eglinIndex = 1;
                int hickamIndex = 1;
                int hillIndex = 1;
                int lukeIndex = 1;
                int kwIndex = 1;
                int robinIndex = 1;

                int numTasks = tasks.Count();
                cd.BaseNames = new string[numTasks];
                cd.cutoffValues = new int[numTasks];
                cd.cutoffValuesDroplets = new int[numTasks];

                int index = 0;
                baseNamesAndTimeStamps.Clear();
                foreach (AnalysisTask task in tasks)
                {
                    var a = ReadFile(task);
                    var b = a.OrderBy(t => t.Timestamp);
                    sensorData.AddRange(b);
                    cd.cutoffValues[index] = sensorData.Count;
                    if (task.LocationString == "Wright Patterson Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Wright Patt" + wpIndex))
                        {
                            wpIndex++;
                        }
                        cd.BaseNames[index] = "Wright Patt" + wpIndex;
                        baseNamesAndTimeStamps.Add("Wright Patt" + wpIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Guam International Airport")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Guam" + guamIndex))
                        {
                            guamIndex++;
                        }
                        cd.BaseNames[index] = "Guam" + guamIndex;
                        baseNamesAndTimeStamps.Add("Guam" + guamIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Tinker Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Tinker" + tinkerIndex))
                        {
                            tinkerIndex++;
                        }
                        cd.BaseNames[index] = "Tinker" + tinkerIndex;
                        baseNamesAndTimeStamps.Add("Tinker" + tinkerIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Eglin Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Eglin" + eglinIndex))
                        {
                            eglinIndex++;
                        }
                        cd.BaseNames[index] = "Eglin" + eglinIndex;
                        baseNamesAndTimeStamps.Add("Eglin" + eglinIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Hickam Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Hickam" + hickamIndex))
                        {
                            hickamIndex++;
                        }
                        cd.BaseNames[index] = "Hickam" + hickamIndex;
                        baseNamesAndTimeStamps.Add("Hickam" + hickamIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Hill Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Hill" + hillIndex))
                        {
                            hillIndex++;
                        }
                        cd.BaseNames[index] = "Hill" + hillIndex;
                        baseNamesAndTimeStamps.Add("Hill" + hillIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Luke Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Luke" + lukeIndex))
                        {
                            lukeIndex++;
                        }
                        cd.BaseNames[index] = "Luke" + lukeIndex;
                        baseNamesAndTimeStamps.Add("Luke" + lukeIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Naval Air Station Key West")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Key West" + kwIndex))
                        {
                            kwIndex++;
                        }
                        cd.BaseNames[index] = "Key West" + kwIndex;
                        baseNamesAndTimeStamps.Add("Key West" + kwIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else if (task.LocationString == "Robins Air Force Base")
                    {
                        if (Array.Exists(cd.BaseNames, element => element == "Robins" + robinIndex))
                        {
                            robinIndex++;
                        }
                        cd.BaseNames[index] = "Robins" + robinIndex;
                        baseNamesAndTimeStamps.Add("Robins" + robinIndex + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    else
                    {
                        cd.BaseNames[index] = task.LocationString;
                        baseNamesAndTimeStamps.Add(task.LocationString + ": " + task.StartDate.ToShortDateString() + " - " + task.EndDate.ToShortDateString());
                    }
                    index++;
                }

                cm.BeginAnalysisMultiple(tasks, selectedTail, sensorData, pollutionData, ref cd);

            }catch(Exception e)
            {
                if (e.InnerException != null)
                {
                    if (sensorData.Count != 0)
                    {
                        errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.InnerException.Message);
                    }else
                    {
                        errorMessages.Add("No sensor data avalible for selected analysis");
                    }
                }else
                {
                    if (sensorData.Count != 0)
                    {
                        errorMessages.Add("Error encountered during analysis, Exact error message was: " + e.Message);
                    }
                    else
                    {
                        errorMessages.Add("No sensor data avalible for selected analysis");
                    }
                }
            }
            return cd;
        }

        public string GetLocationById(int ID)
        {
            List<t_Location> l1 = db.t_Location.Where(r => (r.Id == ID)).ToList();
            string LocationName = (l1.FirstOrDefault()).Name;
            return LocationName;
        }

        public List<CombinedDataViewModel> ReadFile(AnalysisTask _task)
        {
            List<CombinedDataViewModel> x = GetMetData(_task, savedindex);
            savedindex = x.Count();
            return x;

        }

        public List<CombinedDataViewModel> GetMetData(AnalysisTask _t, int startindex)
        { 
            List<CombinedDataViewModel> DataList = new List<CombinedDataViewModel>();

            List<CombinedData> list = db.CombinedDatas.Where(r => (r.Timestamp > _t.StartDate && r.Timestamp < _t.EndDate && r.Location == _t.Location)).ToList();
            int id = startindex;
            foreach (CombinedData item in list)
            {
                DataList.Add(new CombinedDataViewModel
                {
                    ID = id,
                    Record_Number = Convert.ToInt32(item.Record_Number),
                    TOWSensor = Convert.ToDouble(item.TOWSensor),
                    TOWCalc = Convert.ToDouble(item.TOWCalc),
                    SurfaceTemp = Convert.ToDouble(item.SurfaceTemp),
                    TimeInterval = Convert.ToInt32(item.TimeInterval),
                    ATM_RH = Convert.ToDouble(item.ATM_RH),
                    ATM_Temp = Convert.ToDouble(item.ATM_Temp),
                    Rain = Convert.ToDouble(item.Rain),
                    Timestamp = item.Timestamp ?? DateTime.Now,
                    Month = Convert.ToInt32(item.Month),
                    Location = item.Location ?? 0,
                    DataSourceType = Convert.ToInt32(item.DataType)
                });
                id++;
            }
            return DataList;
        }

        public Dictionary<string, PollutionModel> getAllPollutionValues()
        {
            var a = (from t in db.t_Salt_Composition
                     join y in db.t_Base_Properties on new { t.Location, t.Month } equals new { y.Location, y.Month }
                     select new PollutionModel
                     {
                         Deposit = (double)y.Salt_dep,
                         AmmNit = (double)t.NH4NO3,
                         AmmSul = (double)t.NH42SO4,
                         RainConc = (double)y.Conc,
                         NaCl = (double)t.NaCl,
                         MgCl2 = (double)t.MgC12,
                         Na2S04 = (double)t.Na2S04,
                         CaC12 = (double)t.CaC12,
                         KCI = (double)t.KCI,
                         MgSO4 = (double)t.MgSO4,
                         K2SO4 = (double)t.K2SO4,
                         CaSO4 = (double)t.CaSO4,
                         total = (double)t.Total,
                         HSMass = (double?)t.HSMass,
                         HNMass = (double?)t.HNMass,
                         Sea = 0.5,
                         RainDep = 2.20775,
                         Location = t.Location,
                         Month = t.Month
                     });
            Dictionary<string, PollutionModel> result = new Dictionary<string, PollutionModel>();
            foreach (PollutionModel pm in a)
            {
                result.Add(pm.Location + "-" + pm.Month, pm);
            }

            return result;
        }

        public List<string> GetAllTails()
        {
            List<t_History> his = db.t_History.ToList();
            List<string> tails = new List<string>();
            foreach (t_History item in his)
            {
                if (!tails.Contains(item.Tail_Number))
                {
                    tails.Add(item.Tail_Number);
                }
            }
            return tails;
        }

        // Customized ILoggerProvider, writes logs to text files
        public class CustomFileLoggerProvider : ILoggerProvider
        {
            private readonly StreamWriter _logFileWriter;

            public CustomFileLoggerProvider(StreamWriter logFileWriter)
            {
                _logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new CustomFileLogger(categoryName, _logFileWriter);
            }

            public void Dispose()
            {
                _logFileWriter.Dispose();
            }
        }

        // Customized ILogger, writes logs to text files
        public class CustomFileLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly StreamWriter _logFileWriter;
            const int sizeLimitInBytes = 500 * 1024 * 1024; //500 * 1024 * 1024 is a limit of 500 MB

            public CustomFileLogger(string categoryName, StreamWriter logFileWriter)
            {
                _categoryName = categoryName;
                _logFileWriter = logFileWriter;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                // Ensure that only information level and higher logs are recorded
                return logLevel >= LogLevel.Information;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                // Ensure that only information level and higher logs are recorded
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                if(_logFileWriter.BaseStream.Position > sizeLimitInBytes)
                {
                    string errorFilePath1 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile1";
                    string errorFilePath2 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile2";
                    string errorFilePath3 = ConfigurationManager.AppSettings["ErrorFilePath"] + "ErrorFile3";
                }

                // Get the formatted log message
                var message = formatter(state, exception);

                //Write log messages to text file
                _logFileWriter.WriteLine($"[{logLevel}] [{_categoryName}] {message}");
                _logFileWriter.Flush();
            }
        }

    }
}
