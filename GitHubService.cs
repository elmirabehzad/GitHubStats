using System;

namespace GitHubStats.Helpers
{
    public class GitHubService
    {
        public GitHubService()
        {
        }

        readonly string uri = "https://api.github.com";

        public async Task<List<Language>> getLanguages()
        {

            //using (HttpClient httpClient = new HttpClient())
            //{

            //    return JsonConvert.DeserializeObject<List<Language>>(
            //        await httpClient.GetStringAsync(uri)
            //    );
            //}
            Language[] language = new Language[] 
        {
        new Language { Name = "PHP", Size = 765 },
        new Language { Name = "C++", Size = 654 }
        };

            return language;
        }
    }
}