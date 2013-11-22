using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataSource
{
    using System.Collections.ObjectModel;
    using System.Net.Http;

    using Windows.Data.Json;

    using MyWorldIsComics.Common;
    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.Enums;
    using MyWorldIsComics.Mappers;

    class ComicVineSource
    {
        private static readonly ComicVineSource comicVineSource = new ComicVineSource();
        private static HttpClient client;

        private readonly ObservableCollection<Characters> characters = new ObservableCollection<Characters>();
        public ObservableCollection<Characters> Characters
        {
            get { return this.characters; }
        }

        public ComicVineSource()
        {
            client = new HttpClient();
        }

        public static async Task<Character> ExecuteSearchAsync(string query)
        {
            string parser = await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Search, query));
            Character character;
            CharacterMapper.MapXmlObject(parser, out character);
            return character;
        }

        public static async Task<Character> GetCharacterAsync(string query)
        {
            string parser = await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, query));
            Character character;
            CharacterMapper.MapJsonObject(parser, out character);
            return character;
        }

        private static async Task<string> QueryServiceAsync(Uri uri)
        {
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private Uri ContructUrl(Resources.ResourcesEnum resourcesEnum, string query)
        {
            string uri = ServiceConstants.ComicVineBaseUrl + Resources.GetResourceTerm(resourcesEnum) + "/";
            switch (resourcesEnum)
            {
                case Resources.ResourcesEnum.Search:
                    uri += "?query=" + query;
                    break;
                default:
                    uri += query + "/";
                    break;
            }
            uri += "&" + ServiceConstants.ComicVineApiKey + "&" + ServiceConstants.ComicVineFormat + "&limit=1";
            return new Uri(uri);
        }
    }
}
