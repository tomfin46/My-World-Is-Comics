using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace MyWorldIsComics.DataSource
{
    class MarvelWikiaSource
    {

        private static readonly MarvelWikiaSource marvelWikiaSource = new MarvelWikiaSource();
        private static HttpClient client;
        private static CancellationTokenSource cts;

        public MarvelWikiaSource()
        {
            client = new HttpClient();
            cts = new CancellationTokenSource();
        }

        public static async Task<string> GetTrendingCharactersAsync()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "category", "characters" } };
            return await QueryServiceAsync(marvelWikiaSource.ContructUrl("Articles", "Top", parameters));
        }

        private static async Task<string> QueryServiceAsync(Uri uri)
        {
            var response = await client.GetAsync(uri, cts.Token);
            string content;
            try
            {
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                content = ServiceConstants.QueryNotFound;
            }

            if (content.Contains(ServiceConstants.ObjectNotFound))
            {
                content = ServiceConstants.QueryNotFound;
            }

            return content;
        }

        private Uri ContructUrl(string section, string method, Dictionary<string, string> parameters)
        {
            string uri = ServiceConstants.MarvelWikiaBaseUrl + section + "/" + method + "?";
            foreach (var param in parameters)
            {
                uri += param.Key + "=" + param.Value;
            }

            return new Uri(uri);
        }
    }
}
