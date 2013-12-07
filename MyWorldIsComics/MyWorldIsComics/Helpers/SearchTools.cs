using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;

namespace MyWorldIsComics.Helpers
{
    class SearchTools
    {
        private static List<string> _suggestionsList = new List<string>();

        public SearchTools()
        {
            FillSuggestions();
        }

        public void SearchBoxEventsSuggestionsRequested(SearchBoxSuggestionsRequestedEventArgs args)
        {
            string queryText = args.QueryText;
            if (string.IsNullOrEmpty(queryText)) return;
            Windows.ApplicationModel.Search.SearchSuggestionCollection suggestionCollection = args.Request.SearchSuggestionCollection;

            foreach (string suggestion in _suggestionsList.Where(suggestion => suggestion.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)))
            {
                suggestionCollection.AppendQuerySuggestion(suggestion);
            }
        }

        public static async void FetchSuggestions()
        {
            List<string> names = new List<string>();
            int totalPages;
            var suggestionsString = await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, 0);
            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                reader.ReadToFollowing("number_of_page_results");
                totalPages = reader.ReadElementContentAsInt();
            }

            names.AddRange(MapSuggestionCharacters(await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, 0)));

            //for (int i = 0; i < totalPages; i++)
            //{
            //    names.AddRange(MapSuggestionCharacters(await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, i * 100)));
            //}

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile suggestionFile = await localFolder.CreateFileAsync("suggestionFile.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(suggestionFile, string.Join(",", names));
        }

        private async void FillSuggestions()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile suggestionFile = await localFolder.GetFileAsync("suggestionFile.txt");
            _suggestionsList = new List<string>((await FileIO.ReadTextAsync(suggestionFile)).Split(','));

        }

        private static IEnumerable<string> MapSuggestionCharacters(string suggestionListString)
        {
            return new CharacterMapper().GetSuggestionsList(suggestionListString);
        }
    }
}
