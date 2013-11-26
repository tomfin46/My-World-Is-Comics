namespace MyWorldIsComics.DataSource
{
    #region usings

    using System;
    using System.Threading.Tasks;
    using DataModel.Resources;
    using System.Net.Http;
    using DataModel;
    using DataModel.Enums;
    using Mappers;

    #endregion

    class ComicVineSource
    {
        private static readonly ComicVineSource comicVineSource = new ComicVineSource();
        private static HttpClient client;

        public ComicVineSource()
        {
            client = new HttpClient();
        }

        public static async Task<string> ExecuteSearchAsync(string query)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query));
        }

        public static async Task<string> GetCharacterAsync(int id)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, id.ToString()));
        }

        public static async Task<string> GetQuickTeamAsync(string teamId)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId));
        }

        public static async Task<string> GetIssueAsync(string issueId)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId));
        }

        public static async Task<Description> FormatDescriptionAsync(string descriptionString)
        {
            return await Task.Run(() => DescriptionMapper.MapDescription(descriptionString));
        }

        private static async Task<string> QueryServiceAsync(Uri uri)
        {
            var response = await client.GetAsync(uri);
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

            return content;
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query)
        {
            string uri = ServiceConstants.ComicVineBaseUrl + Resources.GetResourceTerm(resourcesEnum) + "/";
            switch (resourcesEnum)
            {
                case Resources.ResourcesEnum.Search:
                    uri += "?query=" + query + "&limit=1" + "&";
                    break;
                case Resources.ResourcesEnum.Character:
                    uri += "4005-" + query + "/?";
                    break;
                case Resources.ResourcesEnum.Team:
                    uri += "4060-" + query + "/?";
                    break;
                case Resources.ResourcesEnum.Issue:
                    uri += "4000-" + query + "/?";
                    break;
                default:
                    uri += query + "/";
                    break;
            }
            uri += ServiceConstants.ComicVineApiKey + "&" + ServiceConstants.ComicVineFormat;
            return new Uri(uri);
        }

    }
}
