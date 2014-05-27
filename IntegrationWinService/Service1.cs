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
        }

        MRPEntities db = new MRPEntities();
        Timer objTimer;

        /// <summary>
        /// Treggered when servive started
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            objTimer = new Timer();
            objTimer.Interval =  60 * 1000; // 1 hour
            objTimer.Elapsed += objTimer_Elapsed;
            objTimer.Enabled = true;

            WriteLog("Integration service started");
            
        }

        /// <summary>
        /// Treggered based on timer interval 
        /// </summary>
        /// <param name="args"></param>
        void objTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //WriteLog("On elapse");
                ScheduledExternalIntegration obj = new ScheduledExternalIntegration();
                obj.ScheduledSync();
            }
            catch (Exception ex)
            {
                WriteLog("Exception occured : \n Message : " + ex.Message + "\n Detail : \n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Treggered when servive stoped
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStop()
        {
            objTimer.Enabled = false;
            WriteLog("Gameplan integration service has stoped working");
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

        /// <summary>
        /// Function to write log message
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            string LogFilePath = System.Configuration.ConfigurationManager.AppSettings["LogFilePath"].ToString();
            if (LogFilePath != string.Empty)
            {
                StreamWriter write = File.AppendText(@"" + LogFilePath);
                write.WriteLine(DateTime.Now.ToString() + " : " + message + "\n");
                write.Flush();
                write.Close();
            }
        }
    }
}
