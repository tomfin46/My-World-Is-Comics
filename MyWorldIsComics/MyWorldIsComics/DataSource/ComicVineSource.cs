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
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query, new List<string> { "deck", "id", "image", "name" }));
        }

        #region Get Suggestion Lists

        public static async Task<string> GetSuggestionList(Resources.ResourcesEnum resourceEnum, int offset)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(resourceEnum, offset.ToString(), new List<string>{ "id", "name", "publisher" }));
        }

        #endregion

        #region GetBasic methods

        public static async Task<string> GetCharacterAsync(int characterId)
        {
            List<string> filters = new List<string> { "aliases", "birth", "count_of_issue_appearances", "deck", "description", "first_appeared_in_issue", 
                "id", "image", "name", "real_name", "teams" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, characterId.ToString(), filters));
        }

        #endregion

        #region GetFiltered methods

        public static async Task<string> GetFilteredCharacterAsync(int id, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, id.ToString(), filter));
        }

        public static async Task<string> GetFilteredTeamAsync(int teamId, string filter)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId.ToString(), filter));
        }

        public static async Task<string> GetFilteredIssueAsync(int issueId, List<string> filters)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId.ToString(), filters));
        }

        public static async Task<string> GetSpecificIssueAsync(int volumeId, int issueId)
        {
            List<string> filters = new List<string> { "cover_date", "description", "id", "image", "issue_number", "name", "store_date", "volume" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issues, volumeId.ToString(), issueId.ToString(), filters));
        }

        public static async Task<string> GetFilteredVolumeAsync(int volumeId, List<string> filters)
        {
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Volume, volumeId.ToString(), filters));
        }

        #endregion

        #region GetQuick methods

        public static async Task<string> GetQuickCharacterAsync(int characterId)
        {
            List<string> filters = new List<string> { "deck", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, characterId.ToString(), filters));
        }

        public static async Task<string> GetQuickTeamAsync(int teamId)
        {
            List<string> filters = new List<string> { "deck", "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Team, teamId.ToString(), filters));
        }

        public static async Task<string> GetQuickIssueAsync(int issueId)
        {
            List<string> filters = new List<string> { "cover_date", "description", "id", "image", "issue_number", "name", "store_date", "volume" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Issue, issueId.ToString(), filters));
        }

        public static async Task<string> GetQuickCreatorAsync(int personId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Person, personId.ToString(), filters));
        }

        public static async Task<string> GetQuickLocationAsync(int locationId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Location, locationId.ToString(), filters));
        }

        public static async Task<string> GetQuickConceptAsync(int conceptId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Concept, conceptId.ToString(), filters));
        }

        public static async Task<string> GetQuickObjectAsync(int objectId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Object, objectId.ToString(), filters));
        }

        public static async Task<string> GetQuickStoryArcAsync(int storyArcId)
        {
            List<string> filters = new List<string> { "id", "image", "name" };
            return await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.StoryArc, storyArcId.ToString(), filters));
        }

        #endregion

        public static async Task<Description> FormatDescriptionAsync(string descriptionString)
        {
            return await Task.Run(() => new DescriptionMapper().MapDescription(descriptionString));
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

        #region ConstructUrl

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query)
        {
            string uri = ServiceConstants.ComicVineBaseUrl + Resources.GetResourceTerm(resourcesEnum) + "/";
            switch (resourcesEnum)
            {
                case Resources.ResourcesEnum.Search:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "&limit=30&";
                    break;
                case Resources.ResourcesEnum.Issues:
                    uri += "?";
                    break;
                case Resources.ResourcesEnum.Characters:
                    uri += "?offset=" + query + "&";
                    break;
                default:
                    uri += Resources.GetResourceId(resourcesEnum) + query + "/?";
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

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string volumeId, string issueId, IEnumerable<string> filters)
        {
            string uri = this.ContructUrl(resourcesEnum, volumeId).AbsoluteUri;
            uri += "&field_list=" + string.Join(",", filters);
            uri += "&filter=volume:" + volumeId + ",issue_number:" + issueId;
            return new Uri(uri);
        }

        #endregion

        #region CancellationTokenSource

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

        #endregion
    }
}
