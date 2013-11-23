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
            CharacterMapper.QuickMapXmlObject(parser, out character);
            return character;
        }

        public static async Task<Character> GetCharacterAsync(int id)
        {
            string characterParser = await QueryServiceAsync(comicVineSource.ContructUrl(Resources.ResourcesEnum.Character, id.ToString()));
            
            Character character;
            CharacterMapper.MapXmlObject(characterParser, out character);
            return character;
        }

        public static async Task<Description> FormatDescriptionAsync(string descriptionString)
        {
            return await Task.Run(() => DescriptionMapper.MapDescription(descriptionString));
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
                    uri += "?query=" + query + "&limit=1" + "&";
                    break;
                case Resources.ResourcesEnum.Character:
                    uri += "4005-" + query + "/?";
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
