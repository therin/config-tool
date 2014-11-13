using System;
using System.Configuration;
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
using System.Diagnostics;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
using System.Web;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.ServiceProcess;
using System.Configuration.Install;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Reflection;
using Microsoft.SqlServer;
using NetFwTypeLib;
using Microsoft.Win32;
using Microsoft.SqlServer.Management;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Common;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Permissions;
using System.Security;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using IWshRuntimeLibrary;

namespace Config_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
                   {
                       String dllName = new AssemblyName(bargs.Name).Name + ".dll";
                       var assem = Assembly.GetExecutingAssembly();
                       String resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
                       if (resourceName == null) return null; // Not found, maybe another handler will find it
                       using (var stream = assem.GetManifestResourceStream(resourceName))
                       {
                           Byte[] assemblyData = new Byte[stream.Length];
                           stream.Read(assemblyData, 0, assemblyData.Length);
                           return Assembly.Load(assemblyData);
                       }
                   };

                InitializeComponent();
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message);;
            }
        }

        string machineName = System.Environment.MachineName;
        bool is64BitSystem = Environment.Is64BitOperatingSystem;
        WebClient webClient;
        Stopwatch sw = new Stopwatch();
        Server srv;
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private readonly BackgroundWorker worker = new BackgroundWorker();
       
     
        public void listSQLInstances(ComboBox box)
        {
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            System.Data.DataTable table = instance.GetDataSources();
            foreach (System.Data.DataRow row in table.Rows)
            {
                if (row["ServerName"] != DBNull.Value && Environment.MachineName.Equals(row["ServerName"].ToString()))
                {
                    string item = string.Empty;
                    item = row["ServerName"].ToString();
                    if (row["InstanceName"] != DBNull.Value || !string.IsNullOrEmpty(Convert.ToString(row["InstanceName"]).Trim()))
                    {
                        item += @"\" + Convert.ToString(row["InstanceName"]).Trim();
                    }
                    box.Items.Add(item);
                }
            }
        }
        
        public void listSQLInstances2(ComboBox box)
        {
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        if (instanceName == "MSSQLSERVER")
                        {
                            box.Items.Add(machineName);    
                        }
                        else
                        {
                            box.Items.Add(machineName + "\\" + instanceName);
                        }
                    }
                }
            }
        }
     
        public void listSQLDB(ComboBox instanceBox, ComboBox resultBox)
        {

            String conxString = "Data Source=" + instanceBox.SelectedItem.ToString() + "; Integrated Security=True;";

            using (SqlConnection sqlConx = new SqlConnection(conxString))
            {
                sqlConx.Open();
                DataTable tblDatabases = sqlConx.GetSchema("Databases");
                sqlConx.Close();

                foreach (DataRow row in tblDatabases.Rows)
                {
                    resultBox.Items.Add(row["database_name"]);
                }
            }

        }

        public void LegacyBackupDatabase(string BackUpLocation, string BackUpFileName, string DatabaseName, string ServerName)
        {

            DatabaseName = "[" + DatabaseName + "]";
            string fileUNQ = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
            BackUpFileName = BackUpFileName + fileUNQ + ".bak";
            string SQLBackUp = @"BACKUP DATABASE " + DatabaseName + " TO DISK = N'" + BackUpLocation + @"\" + BackUpFileName + @"'";
            string svr = "Server=" + ServerName + ";Database=master;Integrated Security=True";
            SqlConnection cnBk = new SqlConnection(svr);
            SqlCommand cmdBkUp = new SqlCommand(SQLBackUp, cnBk);
            try
            {
                cnBk.Open();
                cmdBkUp.ExecuteNonQuery();
                cmdBkUp.CommandTimeout = 60 * 60; // for an hour or more
                logTextbox2.Text = SQLBackUp + " ######## Server name " + ServerName + " Database " + DatabaseName + " successfully backed up to " + BackUpLocation + @"\" + BackUpFileName + "\n Back Up Date : " + DateTime.Now.ToString();
            }

            catch (Exception ex)
            {
                logTextbox2.Text = ex.Message;
            }

            finally
            {
                if (cnBk.State == ConnectionState.Open)
                {
                    cnBk.Close();
                }
            }
        }


        public void BackupDatabaseSMO(string BackUpLocation, string fileName, string databaseName, string ServerName)
        {

            ServerConnection connection = new ServerConnection(ServerName);
            connection.StatementTimeout = 600;
            Server sqlServer = new Server(connection);
            string path = "";
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Backup";
            string localPath = new Uri(path).LocalPath;
            Backup bkp = new Backup();
            //databaseName = "[" + databaseName + "]";
            string fileUNQ = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
            fileName = localPath + "\\" + fileName + fileUNQ + ".bak";

            Thread.Sleep(100);
            Thread.Sleep(100);
            backup_button.Visibility = Visibility.Hidden;
            Thread.Sleep(100);
            sqlBackupProgressBar.Visibility = Visibility.Visible;

            try
            {
   
                bkp.Action = BackupActionType.Database;
                bkp.Database = databaseName;
                bkp.Devices.AddDevice(fileName, DeviceType.File);
                sqlBackupProgressBar.Value = 0;
                sqlBackupProgressBar.Maximum = 100;
                sqlBackupProgressBar.Value = 10;
                bkp.PercentCompleteNotification = 10;
                bkp.PercentComplete += new PercentCompleteEventHandler(BackupCompletionStatusInPercent);
                bkp.Complete += new ServerMessageEventHandler(Backup_Completed);

                bkp.SqlBackup(sqlServer);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Thread.Sleep(100);
                Thread.Sleep(100);
                backup_button.Visibility = Visibility.Visible;
                Thread.Sleep(100);
                sqlBackupProgressBar.Visibility = Visibility.Hidden;
            }

            Thread.Sleep(100);
            Thread.Sleep(100);
            backup_button.Visibility = Visibility.Visible;
            Thread.Sleep(100);
            sqlBackupProgressBar.Visibility = Visibility.Hidden;
                       
        }

        public void BackupCompletionStatusInPercent(object sender, PercentCompleteEventArgs args)
        {
            UpdateProgressBar(sqlBackupProgressBar, args.Percent);
        }

        public void Backup_Completed(object sender, ServerMessageEventArgs args)
        {
            MessageBox.Show(args.Error.Message.ToString());
        }
   
        
        public void RestoreDatabase(String databaseName, String backUpFile, String serverName)
        {

            Thread.Sleep(100);
            Thread.Sleep(100);
            restoredbButton.Visibility = Visibility.Hidden;
            Thread.Sleep(100);
            sqlRestoreProgressBar.Visibility = Visibility.Visible;

            try
            {

                sqlRestoreProgressBar.Value = 0;
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string localPath = new Uri(path).LocalPath;
                string dataFileLocation = localPath + "\\" + databaseName + ".mdf";
                string logFileLocation = localPath + "\\" + databaseName + "_Log" + ".ldf";
                Restore sqlRestore = new Restore();
                BackupDeviceItem deviceItem = new BackupDeviceItem(backUpFile, DeviceType.File);
                sqlRestore.Devices.Add(deviceItem);
                sqlRestore.Database = databaseName;
                ServerConnection connection = new ServerConnection(serverName);
                connection.StatementTimeout = 600;
                Server sqlServer = new Server(connection);
                sqlRestore.Action = RestoreActionType.Database;
                string logFile = System.IO.Path.GetDirectoryName(backUpFile);
                logFile = System.IO.Path.Combine(logFile, databaseName + "_Log.ldf");
                string dataFile = System.IO.Path.GetDirectoryName(backUpFile);
                dataFile = System.IO.Path.Combine(dataFile, databaseName + ".mdf");
                Database db = sqlServer.Databases[databaseName];
                RelocateFile rf = new RelocateFile(databaseName, dataFile);
                System.Data.DataTable logicalRestoreFiles = sqlRestore.ReadFileList(sqlServer);
                sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[0][0].ToString(), dataFileLocation));
                sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[1][0].ToString(), logFileLocation));
                sqlRestore.ReplaceDatabase = true;
                sqlServer.KillAllProcesses(databaseName);
                sqlRestore.Wait();

                sqlRestoreProgressBar.Maximum = 100;
                sqlRestoreProgressBar.Value = 10;
                sqlRestore.PercentCompleteNotification = 10;
                sqlRestore.PercentComplete += new PercentCompleteEventHandler(CompletionStatusInPercent);
                sqlRestore.Complete += new ServerMessageEventHandler(Restore_Completed);
                //sqlRestore.Information += new ServerMessageEventHandler(Restore_Information);

                sqlRestore.SqlRestore(sqlServer);
                //db = sqlServer.Databases[databaseName];
                //db.SetOnline();
                //sqlServer.Refresh();


                Thread.Sleep(100);
                restoredbButton.Visibility = Visibility.Visible;
                Thread.Sleep(100);
                sqlRestoreProgressBar.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                Thread.Sleep(100);
                restoredbButton.Visibility = Visibility.Visible;
                Thread.Sleep(100);
                sqlRestoreProgressBar.Visibility = Visibility.Hidden;
            }
        }


        public void CompletionStatusInPercent(object sender, PercentCompleteEventArgs args)
        {
            UpdateProgressBar(sqlRestoreProgressBar, args.Percent);

        }

        private void UpdateProgressBar(System.Windows.Controls.ProgressBar progressBar, double progressValue)
        {


            var progressBarDelegateSetValue = new UpdateProgressBarDelegate(progressBar.SetValue);

                // Update the Value of the ProgressBar:
                // 1) Pass the "progressBarDelegateSetValue" delegate that points to the ProgressBar1.SetValue method
                // 2) Set the DispatcherPriority to "Background"
                // 3) Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value 

                var resultDispatch = Dispatcher.Invoke(
                    progressBarDelegateSetValue,
                    DispatcherPriority.Background,
                    new object[]
                        {
                            System.Windows.Controls.ProgressBar.ValueProperty,
                            progressValue
                        });

            }

     
        public void Restore_Completed(object sender, ServerMessageEventArgs args)
        {

            MessageBox.Show(args.Error.Message.ToString());

        }

        private void Restore_Information(object sender, ServerMessageEventArgs args)
        {

            MessageBox.Show(args.Error.Message.ToString());
            

        }
   
   
           
        public void DownloadFile(string urlAddress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            labelSpeed.Content = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

            // Update the progressbar percentage only when the value is not the same.
            progressBar.Value = e.ProgressPercentage;

            // Show the percentage on our label.
            //labelPerc.Content = e.ProgressPercentage.ToString() + "%";

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            //labelDownloaded.Content = string.Format("{0} MB's / {1} MB's",
            //   (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
            //   (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (e.Cancelled == true)
            {
                MessageBox.Show("Download has been canceled.");
            }
            else
            {
                MessageBox.Show("Download completed!");
            }
        }
        //  */
        /// Create a Windows service when it does not exist, else configure it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="binPath"></param>
        /// <param name="userName"></param>
        /// <param name="unecryptedPassword"></param>
        /// <param name="startupType"></param>
        public void CreateService(string name, string displayName, string binPath, string userName, string unecryptedPassword, string startupType, string dependency)
        {

            // Determine statuptype
            string startupTypeConverted = string.Empty;
            switch (startupType)
            {
                case "Automatic":
                    startupTypeConverted = "delayed-auto";
                    break;
                case "Disabled":
                    startupTypeConverted = "disabled";
                    break;
                case "Manual":
                    startupTypeConverted = "demand";
                    break;
                default:
                    startupTypeConverted = "delayed-auto";
                    break;
            }

            // Determine if service has to be created (Create) or edited (Config)
            StringBuilder builder = new StringBuilder();
            //ServiceController[] services1 = ServiceController.GetServices();
            //if (ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(name)))
            //{
            //    builder.AppendFormat("{0} {1} ", "Config", name);
            //}
            //else
            //{
            //builder.AppendFormat("{0} {1} ", "create", name);
            //}
            builder.AppendFormat("sc create \"{0}\"  ", name);
            builder.AppendFormat("binPath= \"{0}\"  ", binPath);
            builder.AppendFormat("displayName= \"{0}\"  ", displayName);

            // Only add "obj" when username is not empty. If omitted the "Local System" account will be used
            if (!string.IsNullOrEmpty(userName))
            {
                builder.AppendFormat("obj= \"{0}\"  ", userName);
            }

            // Only add password when unecryptedPassword it is not empty and user name is not "NT AUTHORITY\Local Service" or NT AUTHORITY\NetworkService
            if (!string.IsNullOrEmpty(unecryptedPassword) && !unecryptedPassword.Equals(@"NT AUTHORITY\Local Service") && !unecryptedPassword.Equals(@"NT AUTHORITY\NetworkService"))
            {
                builder.AppendFormat("password= \"{0}\"  ", unecryptedPassword);
            }

            builder.AppendFormat("start= \"{0}\"  ", startupTypeConverted);
            if (!string.IsNullOrEmpty(dependency))
            {
                builder.AppendFormat("depend= \"{0}\"  ", dependency);
            }
            

            logTextbox.Text = builder.ToString();

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
            processInfo.Verb = "runas";
            processInfo.Arguments = "/K " + builder.ToString() + " && sc failure " + "\"" + name + "\"" + " reset= 86400 actions= restart/60000 ";
            Process.Start(processInfo);
            listServices2();
                      
            

        }

        public void listServices(ComboBox comboBoxName, string filter)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.DisplayName.ToLower().Contains(filter))
                {
                    string serviceName = service.ServiceName;
                    string serviceDisplayName = service.DisplayName;
                    string status = service.Status.ToString();
                    comboBoxName.Items.Add(serviceName);
                }
            }
        }

        public void listServices2()
        {
            servicesListbox.Items.Clear();
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service.DisplayName.ToLower().Contains("smarttrade"))
                {
                    string serviceName = service.ServiceName;
                    string serviceDisplayName = service.DisplayName;
                    string status = service.Status.ToString();
                    servicesListbox.Items.Add(serviceName);
                }
            }

        }
        
   
        private void ServiceComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceComboBox.Items.Clear();
            listServices(ServiceComboBox, "sql");
        }



        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            clientTextbox.Text = machineName;
        }

        private void currentClient_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Client\\SmartTrade.exe.Config");
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServiceMachineName']").Attributes["value"].Value;
                currentClient.Content = value;
            }
            catch (Exception)
            {
                currentClient.Content = "Directory not found";
                buttonClient.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Client\\SmartTrade.exe.Config");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();
            string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServiceMachineName']").Attributes["value"].Value;
            string portValue = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServicePort']").Attributes["value"].Value;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServiceMachineName']").Attributes["value"].Value = clientTextbox.Text;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServicePort']").Attributes["value"].Value = clientPortTextbox.Text;
            string uriPath = path + "\\Client\\SmartTrade.exe.Config";
            string localPath = new Uri(uriPath).LocalPath;
            doc.Save(localPath);
            currentClient.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServiceMachineName']").Attributes["value"].Value;
            currentClient_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServicePort']").Attributes["value"].Value;

        }

        private void mobileTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            mobileTextbox.Text = machineName;
        }

        private void currentMobile_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachine']").Attributes["value"].Value;
                currentMobile.Content = value;
            }
            catch (Exception)
            {
                currentMobile.Content = "Directory not found";
                buttonMobile.IsEnabled = false;
            }
        }


        private void buttonMobile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe.config");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();
            string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachine']").Attributes["value"].Value;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachine']").Attributes["value"].Value = mobileTextbox.Text;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value = mobilePortTextbox.Text;
            string uriPath = path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe.config";
            string localPath = new Uri(uriPath).LocalPath;
            doc.Save(localPath);
            currentMobile.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachine']").Attributes["value"].Value;
            currentMobile_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
        }

        private void currentTime_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartTime\\STWorkerService.exe.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachineName']").Attributes["value"].Value;
                currentTime.Content = value;
            }
            catch (Exception)
            {
                currentTime.Content = "Directory not found";
                buttonTime.IsEnabled = false;
            }
        }

        private void timeTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            timeTextbox.Text = machineName;
        }

        private void buttonTime_Click(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartTime\\STWorkerService.exe.config");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();
            string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachineName']").Attributes["value"].Value;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachineName']").Attributes["value"].Value = timeTextbox.Text;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value = timePortTextbox.Text;
            string uriPath = path + "\\SmartTime\\STWorkerService.exe.config";
            string localPath = new Uri(uriPath).LocalPath;
            doc.Save(localPath);
            currentTime.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServiceMachineName']").Attributes["value"].Value;
            currentTime_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
        }

        private void currentGate_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartGate\\Web.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServiceMachine']").Attributes["value"].Value;
                currentGate.Content = value;
            }
            catch (Exception)
            {
                currentGate.Content = "Directory not found";
                buttonGate.IsEnabled = false;
            }
        }

        private void gateTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            gateTextbox.Text = machineName;
        }

        private void buttonGate_Click(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartGate\\Web.config");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();
            string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServiceMachine']").Attributes["value"].Value;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServiceMachine']").Attributes["value"].Value = gateTextbox.Text;
            doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServicePort']").Attributes["value"].Value = gatePortTextbox.Text;
            string uriPath = path + "\\SmartGate\\Web.config";
            string localPath = new Uri(uriPath).LocalPath;
            doc.Save(localPath);
            currentGate.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServiceMachine']").Attributes["value"].Value;
            currentGate_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServicePort']").Attributes["value"].Value;
        }

        private void currentService_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[3]/value/text()");
                currentService.Content = aConnection.InnerText;
            }
            catch (Exception)
            {
                currentService.Content = "Directory not found";
                buttonService.IsEnabled = false;
            }

        }

        private void serviceComputerTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            gateTextbox.Text = machineName;
        }

        /*
        private void serviceDBTextbox_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[@name='SqlConnectionString']");
                serviceDBTextbox.Text = aConnection.InnerText;
            }
            catch (Exception)
            {
                currentService.Content = "Directory not found";
                buttonService.IsEnabled = false;
            }
        }
        
        */
        private void serviceComNameTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            serviceComNameTextbox.Text = machineName;
        }


        private void buttonService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[3]/value/text()");
                XmlNode portConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[1]/value/text()");
                XmlNode updaterLocation = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[6]/value/text()");
                XmlNode updaterBackup = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[7]/value/text()");
                portConnection.InnerText = servicePortTextbox.Text;
                updaterLocation.InnerText = updaterlocationTextbox.Text;
                updaterBackup.InnerText = updaterbackupTextbox.Text;

                
                SqlConnectionStringBuilder builder = null;
                builder = new SqlConnectionStringBuilder(aConnection.InnerText);
                if (String.IsNullOrEmpty(serviceSQLNameTextbox_Copy.Text))
                {
                    builder.DataSource = serviceComNameTextbox.Text;
                }
                else
                {
                    builder.DataSource = serviceComNameTextbox.Text + @"\" + serviceSQLNameTextbox_Copy.Text;
                }
                
                builder.InitialCatalog = serviceDBNameTextbox.Text;
                string uriPath = path + "\\Service\\SmartTrade.Service.Host.exe.config";
                string localPath = new Uri(uriPath).LocalPath;
                currentService_Copy.Content = portConnection.InnerText;
                aConnection.InnerText = builder.ToString();
                currentService.Content = aConnection.InnerText;
                doc.Save(localPath);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }



        private void buttonService_Click_deprecated(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[3]/value/text()");
                XmlNode portConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[1]/value/text()");
                XmlNode updaterLocation = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[6]/value/text()");
                XmlNode updaterBackup = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[7]/value/text()");
                //aConnection.InnerText = serviceDBTextbox.Text;
                portConnection.InnerText = servicePortTextbox.Text;
                updaterLocation.InnerText = updaterlocationTextbox.Text;
                updaterBackup.InnerText = updaterbackupTextbox.Text;
                string uriPath = path + "\\Service\\SmartTrade.Service.Host.exe.config";
                string localPath = new Uri(uriPath).LocalPath;
                doc.Save(localPath);
                currentService.Content = aConnection.InnerText;
                currentService_Copy.Content = portConnection.InnerText;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }


        private void clientPortTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Client\\SmartTrade.exe.Config");
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServicePort']").Attributes["value"].Value;
                clientPortTextbox.Text = value;
            }
            catch (Exception)
            {
                clientPortTextbox.Text = "Directory not found";
                buttonClient.IsEnabled = false;
            }

        }

        private void mobilePortTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
                mobilePortTextbox.Text = value;
            }
            catch (Exception)
            {
                currentMobile.Content = "Directory not found";
                buttonMobile.IsEnabled = false;
            }
        }

        private void gatePortTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartGate\\Web.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServicePort']").Attributes["value"].Value;
                gatePortTextbox.Text = value;
            }
            catch (Exception)
            {
                currentGate.Content = "Directory not found";
                buttonGate.IsEnabled = false;
            }
        }

        private void timePortTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlTextReader reader = new XmlTextReader(path + "\\SmartTime\\STWorkerService.exe.config");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(reader);
                reader.Close();
                string value = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
                timePortTextbox.Text = value;
            }
            catch (Exception)
            {
                currentTime.Content = "Directory not found";
                buttonTime.IsEnabled = false;
            }

        }

        private void servicePortTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[@name='ServicePort']");
                servicePortTextbox.Text = aConnection.InnerText;
            }
            catch (Exception)
            {
                currentService.Content = "Directory not found";
                buttonService.IsEnabled = false;
            }

        }



        public void servicesListbox_Loaded(object sender, RoutedEventArgs e)
        {
            servicesListbox.Items.Clear();
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.DisplayName.ToLower().Contains("smarttrade"))
                {
                    string serviceName = service.ServiceName;
                    string serviceDisplayName = service.DisplayName;
                    string status = service.Status.ToString();
                    servicesListbox.Items.Add(serviceName);
                }
            }
            mobileSComboBox_Loaded(sender, e);
            smarttimeSCombobox_Loaded(sender, e);
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            string dependency;
            if (ServiceComboBox.SelectedIndex == -1)
            {
                dependency = "";
            }
            else
            {
                dependency = ServiceComboBox.SelectedItem.ToString(); 
            }
            
            string name = STServiceTextbox.Text;
            string displayName = STServiceTextbox.Text;
            string binPath = servicePathTextbox.Text;
            string startupType = "Automatic";
            string userName = "";
            string unecryptedPassword = "";
          
            CreateService(name, displayName, binPath, userName, unecryptedPassword, startupType, dependency);
            Thread.Sleep(100);
            mobileSComboBox_Loaded(sender, e);
            smarttimeSCombobox_Loaded(sender, e);
            listServices2();


        }

        private void mobileSComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            mobileSComboBox.Items.Clear();
            listServices(mobileSComboBox, "smarttrade");
        }

        private void smarttimeSCombobox_Loaded(object sender, RoutedEventArgs e)
        {
            smarttimeSCombobox.Items.Clear();
            listServices(smarttimeSCombobox, "smarttrade");
        }



        private void servicePathTextbox_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\Service\\SmartTrade.Service.Host.exe";
                string localPath = new Uri(uriPath).LocalPath;
                servicePathTextbox.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

        private void mobilePathTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe";
                string localPath = new Uri(uriPath).LocalPath;
                mobilePathTextbox.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

        private void timeTextbox_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\SmartTime\\STWorkerService.exe";
                string localPath = new Uri(uriPath).LocalPath;
                timeTextbox_Copy.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

        private void currentClient_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Client\\SmartTrade.exe.Config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                currentClient_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='LastServicePort']").Attributes["value"].Value;
            }
            catch (Exception)
            {

            }
        }

        private void currentMobile_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Mobile\\SmartTrade.Mobile.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                currentMobile_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
            }
            catch (Exception)
            {

            }
        }

        private void currentService_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode portConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[1]/value/text()");
                currentService_Copy.Content = portConnection.InnerText;
            }
            catch (Exception)
            {

            }
        }

        private void currentGate_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\SmartGate\\Web.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                currentGate_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='ServicePort']").Attributes["value"].Value;
            }
            catch (Exception)
            {

            }

        }

        private void currentTime_Copy_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\SmartTime\\STWorkerService.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                currentTime_Copy.Content = doc.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key='SmartTradeServicePort']").Attributes["value"].Value;
            }
            catch (Exception)
            {

            }
        }

        private void createMobileButton_Click(object sender, RoutedEventArgs e)
        {
            string dependency;
            if (mobileSComboBox.SelectedIndex == -1)
            {
                dependency = "";
            }
            else
            {
                dependency = mobileSComboBox.SelectedItem.ToString();
            }
            
            string name = STMobileTextbox.Text;
            string displayName = STMobileTextbox.Text;
            string binPath = mobilePathTextbox.Text;
            string startupType = "Automatic";
            string userName = "";
            string unecryptedPassword = "";
           
            CreateService(name, displayName, binPath, userName, unecryptedPassword, startupType, dependency);
            Thread.Sleep(100);
            listServices2();


        }

        private void createTimeButton_Click(object sender, RoutedEventArgs e)
        {
            string dependency;
            if (smarttimeSCombobox.SelectedIndex == -1)
            {
                dependency = "";
            }
            else
            {
                dependency = smarttimeSCombobox.SelectedItem.ToString();
            }
            string name = STTimeTextbox.Text;
            string displayName = STTimeTextbox.Text;
            string binPath = timeTextbox_Copy.Text;
            string startupType = "Automatic";
            string userName = "";
            string unecryptedPassword = "";

            CreateService(name, displayName, binPath, userName, unecryptedPassword, startupType, dependency);
            Thread.Sleep(100);
            listServices2();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            try
            {
                string servicename = "";
                Dispatcher.Invoke(new Action(() => { servicename = servicesListbox.SelectedItem.ToString(); }));
                ServiceController service = new ServiceController(servicename);
                TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
                Dispatcher.Invoke(new Action(() => { startSButton.Visibility = Visibility.Hidden; }));
                Dispatcher.Invoke(new Action(() => { StartServiceProgressBar.Visibility = Visibility.Visible; }));


                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
                
                if (service.Status == ServiceControllerStatus.Running)
                {
                    MessageBox.Show("Service started successfully.");
                }
                else
                {
                    MessageBox.Show("Service not started.");
                    //MessageBox.Show("  Current State: {0}", service.Status.ToString("f"));
                }

                //MessageBox.Show("Service started");
                //Dispatcher.Invoke(new Action(() => { MessageBox.Show("Service started"); }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }


        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            //MessageBox.Show("Service started");
            StartServiceProgressBar.Visibility = Visibility.Hidden;
            startSButton.Visibility = Visibility.Visible;
            //Dispatcher.Invoke(new Action(() => { StartServiceProgressBar.Visibility = Visibility.Hidden; }));
            //Dispatcher.Invoke(new Action(() => { startSButton.Visibility = Visibility.Visible; }));

        }


        private void startSButton_Click(object sender, RoutedEventArgs e)
        {

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();

            
        }

        private void stopSButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceController service = new ServiceController(servicesListbox.SelectedItem.ToString());
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(6000);

                service.Stop();
                MessageBox.Show("Service stopped");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void deleteSButton_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Do you really want to delete it?",
          "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ServiceController service = new ServiceController(servicesListbox.SelectedItem.ToString());
                string item = string.Format("\"{0}\"", servicesListbox.SelectedItem.ToString());
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(2000);
                    ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
                    processInfo.Verb = "runas";
                    processInfo.Arguments = "/K " + "sc delete " + item;
                    logTextbox.Text = "/K " + "sc delete " + item;
                    Process.Start(processInfo);
                    Thread.Sleep(100);
                    listServices2();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                // Do not close the window
            }
            listServices2();
        }

        private void sqlDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(downloadCombobox.Text);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            logTextbox2.Text = System.AppDomain.CurrentDomain.BaseDirectory + filename;
            if (filename == "v6blank-AUS.bak" || filename == "v6blank-NZ.bak" || filename == "v6demo-AUS.bak" || filename == "v6demo-NZ.bak")
            {
                DownloadFile(downloadCombobox.Text, System.AppDomain.CurrentDomain.BaseDirectory + "backup\\" + filename);
            }
            else
            {
                DownloadFile(downloadCombobox.Text, System.AppDomain.CurrentDomain.BaseDirectory + filename);
            }
        }


        private void systemLabel_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (is64BitSystem) { systemLabel.Content = "64 bit OS detected"; }
            else { systemLabel.Content = "32 bit OS detected"; }
        }

        private void sqlCmdTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            sqlCmdTextbox.Text = Properties.Resources.ConfigurationFile;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                System.IO.File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "\\ConfigurationFile.ini", sqlCmdTextbox.Text);
                string filename = "";
                if (is64BitSystem) { filename = "SQLEXPRWT_x64_ENU.exe"; }
                else { filename = "SQLEXPRWT_x86_ENU.exe"; }
                string configFileName = System.AppDomain.CurrentDomain.BaseDirectory + "\\ConfigurationFile.ini";
                ProcessStartInfo processInfo = new ProcessStartInfo("\"" + System.AppDomain.CurrentDomain.BaseDirectory + filename + "\"");
                processInfo.Verb = "runas";
                processInfo.Arguments = "/QS /SAPWD=\"Admin1234\" /CONFIGURATIONFILE=\"" + configFileName + "\"";
                logTextbox2.Text = "/QS /SAPWD=\"Admin1234\" /CONFIGURATIONFILE=\"" + configFileName + "\"";
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void sqlInstanceCombobox_DropDownOpened(object sender, EventArgs e)
        {
            sqlInstanceCombobox.Items.Clear();
            try
            {
                listSQLInstances2(sqlInstanceCombobox);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void sqlDBCombobox_DropDownOpened(object sender, EventArgs e)
        {
            sqlDBCombobox.Items.Clear();
            try
            {
                listSQLDB(sqlInstanceCombobox, sqlDBCombobox);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void backupPathTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\backup";
                string localPath = new Uri(uriPath).LocalPath;
                backupPathTextbox.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

        private void restorePathTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\backup";
                string localPath = new Uri(uriPath).LocalPath;
                restorePathTextbox.Text = localPath;
            }
            catch (Exception)
            {
            }
        }



        private void backupdbButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BackupDatabaseSMO(backupPathTextbox.Text, "st_backup_", sqlDBCombobox.SelectedItem.ToString(), sqlInstanceCombobox.SelectedItem.ToString());
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                Thread.Sleep(100);
                Thread.Sleep(100);
                backup_button.Visibility = Visibility.Visible;
                Thread.Sleep(100);
                sqlBackupProgressBar.Visibility = Visibility.Hidden;
            }

        }

        private void firewallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Description = "SmartTrade Firewall Rule";
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallRule.Enabled = true;
                firewallRule.Name = "SmartTrade Firewall Rule";
                firewallRule.LocalPorts = porttextBox.Text;
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(firewallRule);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void selectfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\backup";
                string localPath = new Uri(uriPath).LocalPath;
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension 
                dlg.InitialDirectory = localPath;
                dlg.DefaultExt = ".bak";
                dlg.Filter = "backup Files (*.bak)|*.bak";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;
                    restorePathTextbox.Text = filename;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        public void restoredbButton_Click(object sender, RoutedEventArgs e)
        {


            
            if (sqlDBCombobox.Items.Contains(restoredDBnameTextbox.Text))
            {
                if (MessageBox.Show("Database with the same name already exists, this operation will overwrite it, are you sure?",
           "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    try
                    {
                        RestoreDatabase(restoredDBnameTextbox.Text, restorePathTextbox.Text, sqlInstanceCombobox.SelectedItem.ToString());
                        Thread.Sleep(100);
                        sqlDBCombobox_DropDownOpened(sender, e);
                    }
                    catch (Exception ex)
                    {
                        
                        MessageBox.Show(ex.Message);
                        Thread.Sleep(100);
                        restoredbButton.Visibility = Visibility.Visible;
                        Thread.Sleep(100);
                        sqlRestoreProgressBar.Visibility = Visibility.Hidden;
                    }


                }
                else
                {
                    // Do not close the window
                }
            }
            else
            {


                try
                {
                    RestoreDatabase(restoredDBnameTextbox.Text, restorePathTextbox.Text, sqlInstanceCombobox.SelectedItem.ToString());
                    Thread.Sleep(100);
                    sqlDBCombobox_DropDownOpened(sender, e);
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show(ex.Message);
                }

            }
         
            
        }
             
        private void permissionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string localPath = new Uri(path).LocalPath;
                DirectorySecurity sec = Directory.GetAccessControl(localPath);
                // Using this instead of the "Everyone" string means we work on non-English systems.
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(localPath, sec);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void shareButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NTAccount ntAccount = new NTAccount("Everyone");
                SecurityIdentifier oGrpSID = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
                byte[] utenteSIDArray = new byte[oGrpSID.BinaryLength];
                oGrpSID.GetBinaryForm(utenteSIDArray, 0);

                ManagementObject oGrpTrustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
                oGrpTrustee["Name"] = "Everyone";
                oGrpTrustee["SID"] = utenteSIDArray;

                ManagementObject oGrpACE = new ManagementClass(new ManagementPath("Win32_Ace"), null);
                oGrpACE["AccessMask"] = 2032127;//Full access
                oGrpACE["AceFlags"] = AceFlags.ObjectInherit | AceFlags.ContainerInherit; //propagate the AccessMask to the subfolders
                oGrpACE["AceType"] = AceType.AccessAllowed;
                oGrpACE["Trustee"] = oGrpTrustee;

                ManagementObject oGrpSecurityDescriptor = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);
                oGrpSecurityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT
                oGrpSecurityDescriptor["DACL"] = new object[] { oGrpACE };

                
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string localPath = new Uri(path).LocalPath;
                ManagementClass managementClass = new ManagementClass("Win32_Share");
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
                ManagementBaseObject outParams;

                //inParams["Description"] = Description;
                inParams["Name"] = shareTextBox.Text;
                inParams["Path"] = localPath;
                inParams["MaximumAllowed"] = null;
                inParams["Password"] = null;
                inParams["Access"] = oGrpSecurityDescriptor;
                inParams["Type"] = 0x0; // Disk Drive

                // Invoke the method on the ManagementClass object
                outParams = managementClass.InvokeMethod("Create", inParams, null);
                MessageBox.Show("Done!");

                /*

                // Check to see if the method invocation was successful
                if ((uint)(outParams.Properties["ReturnValue"].Value) != 0)
                {
                    throw new Exception("Unable to share directory.");
                }
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error!");
            }

        }

        private void backup_button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                
                StringBuilder builder = new StringBuilder();
                string path = backupPathTextbox.Text + @"\backup.bat";
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
                builder.AppendFormat(@"C:\WINDOWS\SYSTEM32\schtasks.exe /Create /RL HIGHEST /TN ");
                builder.AppendFormat("\"SmartTrade Database backup\" /TR \"\"\"\"{0}\"\"\"\" ", path);
                builder.AppendFormat(@"/sc DAILY /ST 17:00");
                processInfo.Verb = "runas";
                processInfo.Arguments = "/K " + builder.ToString();
                Process.Start(processInfo);
                var Process2 = new Process();
                var ProcessStartInfo = new ProcessStartInfo("cmd", "/C control schedtasks");
                ProcessStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process2.StartInfo = ProcessStartInfo;
                Process2.Start();

            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message);
            }
        }

        private void updaterlocationTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader2 = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc2 = new XmlDocument();
                doc2.Load(reader2);
                reader2.Close();
                XmlNode aConnection2 = doc2.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[6]/value/text()");
  
            }
            catch (Exception ex)
            {
                currentService.Content = "Directory not found";
                buttonService.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\UpdateServer\\SmartTrade.Updater.Server.exe";
                string localPath = new Uri(uriPath).LocalPath;
                updaterlocationTextbox.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

        private void updaterbackupTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string path;
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                XmlTextReader reader = new XmlTextReader(path + "\\Service\\SmartTrade.Service.Host.exe.config");
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                XmlNode aConnection = doc.DocumentElement.SelectSingleNode("/configuration/applicationSettings/SmartTrade.Service.Host.Settings/setting[7]/value/text()");
            }
            catch (Exception)
            {
                currentService.Content = "Directory not found";
                buttonService.IsEnabled = false;
            }
            try
            {
                string path = "";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\Backup";
                string localPath = new Uri(uriPath).LocalPath;
                updaterbackupTextbox.Text = localPath;
            }
            catch (Exception)
            {

            }
        }

     
        private void shortcut_Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string path = "";
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + shortcut_Textbox.Text + @".lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                //shortcut.Description = shortcut_Textbox.Text;
                //shortcut.Hotkey = "Ctrl+Shift+q";
                //shortcut.TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\notepad.exe";
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                string uriPath = path + "\\Client\\SmartTrade.exe";
                string localPath = new Uri(uriPath).LocalPath;
                string uriPath2 = path + "\\Client";
                string localPath2 = new Uri(uriPath2).LocalPath;
                shortcut.TargetPath = localPath;
                shortcut.WorkingDirectory = localPath2;
                shortcut.Save();
                MessageBox.Show("Shortcut created");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            
        }

      

   

 
    }
}


  
 
       


