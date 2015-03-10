using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GitHubStats.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Security;

namespace GitHubStats.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        [HttpGet] // Sample URL: http://localhost:11277/Api/values/Languages?repoName=musescore
        public List<Language> Languages([FromUri] string repoName)
        {
            Logger.LogTrace("It's going to retrive languages from repository " + repoName);
            string apiUrl = "";
            string homepage = "";
            if( getRepoInfo(repoName, ref apiUrl, ref homepage) ){
                return getLanguages(apiUrl);
            }


            return new List<Language>();
        }

        [HttpGet] // Sample URL: http://localhost:11277/Api/values/Stats?repoName=musescore
        public Info Stats([FromUri] string repoName)
        {
            Logger.LogTrace("It's going to retrive Stats from repository " + repoName);

            Info info = new Info();

            string apiUrl = "";
            string homepage = "";
            if( getRepoInfo(repoName, ref apiUrl, ref homepage) ){
                info.Homepage = homepage;
                info.Contributorcount = getContributorCount(apiUrl);
                info.Branchcount = getBranchCount(apiUrl);
            }

            return info;
        }

        [HttpGet] // Sample URL: http://localhost:11277/Api/values/Contributors?repoName=musescore
        public List<Contributor> Contributors([FromUri] string repoName)
        {
            Logger.LogTrace("It's going to retrive Contributors from repository " + repoName);

            string apiUrl = "";
            string homepage = "";
            if (getRepoInfo(repoName, ref apiUrl, ref homepage))
            {
                return getContributors(apiUrl);
            }

            return new List<Contributor>();
        }

        [HttpGet] // Sample URL: http://localhost:11277/Api/values/BranchList?repoName=musescore
        public List<string> BranchList([FromUri] string repoName)
        {
            Logger.LogTrace("It's going to retrive BranchList from repository " + repoName);

            string apiUrl = "";
            string homepage = "";
            if (getRepoInfo(repoName, ref apiUrl, ref homepage))
            {
                return getBranchList(apiUrl);
            }

            return new List<string>();
        }

        [HttpGet] // Sample URL: http://localhost:11277/Api/values/CommitsInBranch?repoName=musescore&branchName=2.0beta1&topDate=2012-08-01T00:00:00Z
        public List<CommitsInDay> CommitsInBranch([FromUri] string repoName, [FromUri] string branchName, [FromUri] string topDate)
        {
            Logger.LogTrace("It's going to retrive CommitsInBranch from repository " + repoName);

            string apiUrl = "";
            string homepage = "";
            try
            {
                if (getRepoInfo(repoName, ref apiUrl, ref homepage))
                {
                    DateTime dateTime = DateTime.Parse(topDate);
                    return getCommitsInBranch(apiUrl, branchName, dateTime);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }

            return new List<CommitsInDay>();
        }


        // *** Helper Methods ***

        private List<Language> getLanguages(string baseUrl)
        {
           
            List<Language> langs = new List<Language>();

            string jsonStr = getGithubResponse(baseUrl + "languages");

            try
            {
                dynamic objLang = JsonConvert.DeserializeObject(jsonStr);

                foreach (dynamic item in objLang)
                {
                    string langName = item.Name;
                    int langSize = item.Value;
                    langs.Add(new Language { Name = langName, Size = langSize });
                }

            }
            catch(Exception e)
            {
                Logger.LogError(e.Message);
            }

            return langs;
        }

        private bool getRepoInfo(string repoName, ref string apiUrl, ref string homepage)
        {
            bool result = true;

            string jsonStr = getGithubResponse("https://api.github.com/search/repositories?q=" + repoName + "+in:name");

            try
            {
                dynamic searchRes = JsonConvert.DeserializeObject(jsonStr);

                foreach (dynamic item in searchRes.items)
                {
                    try
                    {
                        dynamic owner = item.owner;
                        string ownerName = owner.login;
                        apiUrl = "https://api.github.com/repos/" + ownerName + "/" + repoName + "/";
                        homepage = item.homepage;
                        if (apiUrl.Length > 0)
                            break;
                    }
                    catch(Exception e) 
                    {
                        Logger.LogWarning(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                result = false;
            }

            return result;
        }

        private List<string> getBranchList(string baseUrl)
        {
            List<string> branchListStr = new List<string>();
            
            string jsonStr = getGithubResponse(baseUrl + "branches");

            try
            {
                List<dynamic> branchList = JsonConvert.DeserializeObject<List<dynamic>>(jsonStr);
                foreach (dynamic branchItem in branchList)
                {
                    string branchName = branchItem.name;
                    branchListStr.Add(branchName);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

            return branchListStr;
        }

        private int getBranchCount(string baseUrl)
        {
            int branchCount = 0;
            
            string jsonStr = getGithubResponse(baseUrl + "branches");

            try
            {
                List<dynamic> branchList = JsonConvert.DeserializeObject<List<dynamic>>(jsonStr);
                branchCount = branchList.Count();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

            return branchCount;
        }

        private int getContributorCount(string baseUrl)
        {
            int contributorCount = 0;
            
            string jsonStr = getGithubResponse(baseUrl + "contributors");

            try
            {
                List<dynamic> contributorList = JsonConvert.DeserializeObject<List<dynamic>>(jsonStr);
                contributorCount = contributorList.Count();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

            return contributorCount;
        }

        private List<Contributor> getContributors(string baseUrl)
        {

            List<Contributor> contributors = new List<Contributor>();
            
            string jsonStr = getGithubResponse(baseUrl + "stats/contributors");

            try
            {
                dynamic objContributor = JsonConvert.DeserializeObject(jsonStr);

                foreach (dynamic item in objContributor)
                {
                    string contributorName = item.author.login;
                    int totalCommit = item.total;
                    contributors.Add(new Contributor { Name = contributorName, CommitCount = totalCommit });
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

            return contributors;
        }

        private List<CommitsInDay> getCommitsInBranch(string baseUrl, string branchName, DateTime topDate)
        {
            List<CommitsInDay> commitsInDayList = new List<CommitsInDay>();

            string endDate = topDate.ToString("yyyy-MM-ddT23:59:59Z");
            string startDate = topDate.AddDays(-20).ToString("yyyy-MM-ddT00:00:00Z");
            string jsonStr = getGithubResponse(baseUrl + "commits?sha=" + branchName + "&since=" + startDate + "&until=" + endDate);

            if (jsonStr.Length > 0)
            {
                try
                {
                    dynamic objContributor = JsonConvert.DeserializeObject(jsonStr);
                    
                    List<DateTime> dayTimeList = new List<DateTime>();
                    foreach (dynamic item in objContributor)
                    {
                        DateTime dateTime = item.commit.committer.date;
                        dayTimeList.Add(dateTime);
                        //dayTimeList.Add(DateTime.ParseExact(dateTime, "yyyy-MM-ddT23:59:59Z", System.Globalization.CultureInfo.InvariantCulture)); // YYYY-MM-DDTHH:MM:SSZ
                    }

                    List<string> dateList = new List<string>();
                    foreach (DateTime dateTime in dayTimeList)
                    {
                        string dateTimeStr = dateTime.ToString("yyyy-MM-dd");
                        if (!dateList.Contains(dateTimeStr))
                        {
                            dateList.Add(dateTimeStr);
                            commitsInDayList.Add(new CommitsInDay { Day = dateTimeStr, Count = 1 });
                        }
                        else
                        {
                            int index = dateList.IndexOf(dateTimeStr);
                            ++commitsInDayList[index].Count;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            }

            return commitsInDayList;
        }

        private string getGithubResponse(string uri)
        {
            
            string jsonStr = "";

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback
            (
                delegate { return true; }
            );

            WebClient webClient = new WebClient();

            webClient.Headers.Add(HttpRequestHeader.Accept, "*/*");
            webClient.Headers.Add(HttpRequestHeader.UserAgent, "Foo");

            try
            {
                jsonStr = webClient.DownloadString(uri);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

            return jsonStr;
        }



    }

    public class Logger
    { // Purpose of making json in manual way is to improve performannce
        public Logger()
        {
        }

        static public void LogTrace(string message)
        {
            WriteLog( "{\"LogLevel\":\"Trace\",\"ReceiveTime\":\"" 
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static public void LogDebug(string message)
        {
            WriteLog("{\"LogLevel\":\"Debug\",\"ReceiveTime\":\""
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static public void LogWarning(string message)
        {
            WriteLog("{\"LogLevel\":\"Warning\",\"ReceiveTime\":\""
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static public void LogInfo(string message)
        {
            WriteLog("{\"LogLevel\":\"Info\",\"ReceiveTime\":\""
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static public void LogError(string message)
        {
            WriteLog("{\"LogLevel\":\"Error\",\"ReceiveTime\":\""
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static public void LogFatal(string message)
        {
            WriteLog("{\"LogLevel\":\"Fatal\",\"ReceiveTime\":\""
                + System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                + "\",\"message\":\"" + message.Replace('"', '\'') + "\"}");
        }

        static private void WriteLog(string message)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText("C:\\GitHupStatsLog.txt");
            try
            {
                sw.WriteLine(message);
            }
            finally
            {
                sw.Close();
            }
        }
    }
}


