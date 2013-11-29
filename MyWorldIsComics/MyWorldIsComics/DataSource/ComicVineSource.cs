using System.Threading;

namespace MyWorldIsComics.DataSource
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataModel.Resources;
    using System.Net.Http;
    using DataModel;
    using DataModel.Enums;
    using Mappers;

    using MyWorldIsComics.DataModel.DescriptionContent;

    #endregion

    class ComicVineSource
    {
        private static readonly ComicVineSource comicVineSource = new ComicVineSource();
        private static HttpClient client;
        private static CancellationTokenSource cts;

        public ComicVineSource()
        {
            client = new HttpClient();
            cts = new CancellationTokenSource();
        }

        public static async Task<string> ExecuteSearchAsync(string query)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query));
        }

        public static async Task<string> GetCharacterAsync(int id)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, id.ToString()));
        }

        public static async Task<string> GetFilteredCharacterAsync(int id, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, id.ToString(), filter));
        }

        public static async Task<string> GetQuickTeamAsync(string teamId)
        {
            List<string> filters = new List<string>{ "deck", "id", "image", "name"};
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId, filters));
        }

        public static async Task<string> GetIssueAsync(string issueId)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId));
        }

        public static async Task<string> GetFilteredIssueAsync(int issueId, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId.ToString(), filter));
        }

        public static async Task<Description> FormatDescriptionAsync(string descriptionString)
        {
            return await Task.Run(() => DescriptionMapper.MapDescription(descriptionString));
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

            return content;
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query)
        {
            string uri = ServiceConstants.ComicVineBaseUrl + Resources.GetResourceTerm(resourcesEnum) + "/";
            switch (resourcesEnum)
            {
                case Resources.ResourcesEnum.Search:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "&limit=1" + "&";
                    break;
                case Resources.ResourcesEnum.Character:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "/?";
                    break;
                case Resources.ResourcesEnum.Team:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "/?";
                    break;
                case Resources.ResourcesEnum.Issue:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "/?";
                    break;
                default:
                    uri += query + "/";
                    break;
            }
            uri += ServiceConstants.ComicVineApiKey + "&" + ServiceConstants.ComicVineFormat;
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query, string filter)
        {
            string uri = this.ContructUrl(resourcesEnum, query).AbsoluteUri;
            uri += "&field_list=" + filter;
            return new Uri(uri);
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query, IEnumerable<string> filters)
        {
            string uri = this.ContructUrl(resourcesEnum, query).AbsoluteUri;
            uri += "&field_list=" + string.Join(",", filters);
            return new Uri(uri);
        }

        public static void CancelTask()
        {
            cts.Cancel();
        }

        public static void ReinstateCts()
        {
            cts = new CancellationTokenSource();
        }

        public static bool IsCanceled()
        {
            return cts.IsCancellationRequested;
        }

    }
}
