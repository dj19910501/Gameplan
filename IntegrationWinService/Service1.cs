using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using Integration;
using RevenuePlanner.Models;

namespace IntegrationWinService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();

            this.ServiceName = System.Configuration.ConfigurationManager.AppSettings.Get("ServiceName");
        }

        MRPEntities db = new MRPEntities();
        Timer objTimer;

        /// <summary>
        /// Treggered when servive started
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {

            int TimeGap = 60 - DateTime.Now.Minute; // Minutes gap between current time and next full o'clocks

            objTimer = new Timer();
            objTimer.Interval = TimeGap * 60 * 1000; // 1 hour
            objTimer.Elapsed += objTimer_Elapsed;
            objTimer.Enabled = true;

            WriteLog("Integration service started");

            Sync();
        }

        /// <summary>
        /// Treggered based on timer interval 
        /// </summary>
        /// <param name="args"></param>
        void objTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            objTimer.Interval = 60 * 60 * 1000;
            Sync();
        }

        /// <summary>
        /// Treggered when servive stoped
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStop()
        {
            objTimer.Enabled = false;
            WriteLog("Plan integration service has stoped working");

            try
            {
                string notificationShare = "";
                string emailBody = "";
                string SupportEmail = System.Configuration.ConfigurationManager.AppSettings["SupportEmail"].ToString();
                string FromSupportMail = System.Configuration.ConfigurationManager.AppSettings["FromSupportMail"].ToString();

                Notification notification = new Notification();
                notificationShare = Custom_Notification.IntegrationWindowsService.ToString();
                notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));
                emailBody = notification.EmailContent.Replace("[time]", DateTime.Now.ToString());
                Common.sendMail(SupportEmail, FromSupportMail, emailBody, notification.Subject);
            }
            catch (Exception ex)
            { WriteLog("Exception occurs while sending email at: " + DateTime.Now.ToString() + " " + ex.Message); }
        }

        /// <summary>
        /// Function for scheduled synchronization
        /// </summary>
        public void Sync()
        {
            try
            {
                Guid applicationId =Guid.Parse( System.Configuration.ConfigurationManager.AppSettings.Get("BDSApplicationCode"));
                ScheduledExternalIntegration obj = new ScheduledExternalIntegration(applicationId);
                obj.ScheduledSync();
            }
            catch (Exception ex)
            {
                WriteLog("Exception occured : \n Message : " + ex.Message + "\n Details : \n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Function to write log message
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            //string LogFilePath = System.Configuration.ConfigurationManager.AppSettings["LogFilePath"].ToString();
            //if (LogFilePath != string.Empty)
            //{
            //    StreamWriter write = File.AppendText(@"" + LogFilePath);
            //    write.WriteLine(DateTime.Now.ToString() + " : " + message + "\n");
            //    write.Flush();
            //    write.Close();
            //}
            try
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                //if (!File.Exists(Path.GetDirectoryName(location) + "\\Log.txt"))
                //{
                //    File.Create(Path.GetDirectoryName(location) + "\\Log.txt");
                //}
                StreamWriter write = new StreamWriter(Path.GetDirectoryName(location) + "\\Log.txt", true);
                //StreamWriter write = File.AppendText(Path.GetDirectoryName(location) + "\\Log.txt");
                write.WriteLine(DateTime.Now.ToString() + " : " + message + "\n");
                write.Flush();
                write.Close();
            }
            catch (Exception) { }
        }
    }
}
