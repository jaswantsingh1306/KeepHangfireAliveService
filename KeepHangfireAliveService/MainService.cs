using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Linq;
using System.Configuration;

namespace KeepHangfireAliveService
{
    public partial class MainService : ServiceBase
    {
        System.Timers.Timer timeDelay;
        public MainService()
        {

            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);
            timeDelay.Interval = 600000;

        }

        protected override void OnStart(string[] args)
        {
            WriteLog("Service is Started");
            timeDelay.Enabled = true;
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);
        }

        protected override void OnStop()
        {
            WriteLog("Service Stopped");
            timeDelay.Enabled = false;
            SendMail obj = new SendMail();
            //TODO During deployment - change env name in below line
            obj.SendEMail("Hangfire alive service stopped, see logs-DEV","");
        }
        // Custom methods starts from here
        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteLog("Process started");
            List<string> allEndPoints = GetAllUrls();
            for (int i = 0; i < allEndPoints.Count; i++)
            {
                string endPoint = allEndPoints[i];
                HitUrl(endPoint);
            }
            WriteLog("Process complete");

        }

        private List<string> GetAllUrls()
        {
            //TODO Change end points- If dev can hit live url the  no required to host this scheduler on every server- ADD END POINTS HERE
            List<string> allEndPoints = new List<string>();
            var baseURL = ConfigurationManager.AppSettings["baseURL"].ToString();// "http://172.20.43.66";
            var postprefixs = ConfigurationManager.AppSettings["postprefixs"].ToString().Split(',');

            var serverManager = new ServerManager();
            foreach (var app in from site in serverManager.Sites
                                from app in site.Applications
                                where app.Path.StartsWith("/Job", StringComparison.OrdinalIgnoreCase)
                                select app)
            {
                Array.ForEach(postprefixs, postprefix => allEndPoints.Add(string.Concat(baseURL, app.Path, postprefix)));
            }


            #region Prod Urls
         //   allEndPoints.Add("http://172.20.43.66/jobcustomerinvoicesapposting/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/jobunexpectedservice/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/JobPostOrderCISCO/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/JobSubscriptionPollCISCOJob/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/JobPendingOrdersProcessing/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/JobAWSInvoicing/hangfire/recurring");
         //   allEndPoints.Add("http://172.20.43.66/JobNCETransitionScheduler/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/jobcreditnotesplit/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/jobazurefraudalert/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/jobmoderncommercerenewal/dashboard/recurring");
         //   allEndPoints.Add("http://172.20.43.66/jobterminationscheduler/hangfire/recurring");
	        //allEndPoints.Add("http://172.20.43.66/jobMagentoDataSync/dashboard/recurring");
            #endregion

            return allEndPoints;

        }
        private void HitUrl(string completeUrlWithParams)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage externalEndPointResponse = client.GetAsync(completeUrlWithParams).Result;
            if (!externalEndPointResponse.IsSuccessStatusCode)
            {
                WriteLog($"Something went wrong. Url is {completeUrlWithParams}");
                SendMail obj = new SendMail();
                //TODO During deployment - change env name in below line
                obj.SendEMail("Hangfire alive service ERROR- see logs-DEV", completeUrlWithParams);
            }
        }

        private void WriteLog(string message)
        {
            string createText = DateTime.Now.ToString() + "   " + message + Environment.NewLine;

            string path = System.AppDomain.CurrentDomain.BaseDirectory + "LogFileHangfireAlive.txt";
            File.AppendAllText(path, createText);
        }
    }
}
