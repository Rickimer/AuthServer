using AuthServer.BLL.DTO.User;
using Newtonsoft.Json;
using System.Text;

namespace AuthServer.BLL.Auth
{
    public class OAuth2Service : IOAuth2Service
    {
        private const string GitHub_Client_ID = "569e058ccfea13790450";
        private const string GitHub_Client_Secret = "63f2b6291a46d7803ec7d725bcc899cad40a0817"; //todo: в секреты

        public async Task<UserGithubRegisterDto> GitHubLogin(string code)
        {
            var gitparams = $"?client_id={GitHub_Client_ID}&client_secret={GitHub_Client_Secret}&code={code}";
            string url = $"https://github.com/login/oauth/access_token/{gitparams}";
            HttpContent gitcontent = new StringContent(gitparams, Encoding.UTF8, "application/json");
            string access_token = "";

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(url))
                {
                    res.EnsureSuccessStatusCode();
                    //Then get the data or content from the response in the next using statement, then within it you will get the data, and convert it to a c# object.
                    using (HttpContent content = res.Content)
                    {
                        string responseBody = await res.Content.ReadAsStringAsync();
                        var response = responseBody.Split("&");
                        access_token = response.First(e => e.StartsWith("access_token")).Split("=")[1];
                        var data = await GetGitHubUserData(access_token);

                        var gitHubUserData = JsonConvert.DeserializeObject<UserGithubRegisterDto>(data);
                        return gitHubUserData;
                    }
                }
            }
        }

        private async Task<string> GetGitHubUserData(string accessToken)
        {
            string baseUrl = "https://api.github.com/user";
            var result = "";

            //We will now define your HttpClient with your first using statement which will use a IDisposable.
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("user-agent", "net.core");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                //In the next using statement you will initiate the Get Request, use the await keyword so it will execute the using statement in order.
                //The HttpResponseMessage which contains status code, and data from response.
                using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                {
                    //Then get the data or content from the response in the next using statement, then within it you will get the data, and convert it to a c# object.
                    using (HttpContent content = res.Content)
                    {
                        result = await res.Content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }
    }
}
