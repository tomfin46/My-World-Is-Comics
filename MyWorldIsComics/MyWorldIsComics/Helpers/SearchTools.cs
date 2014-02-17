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
            int totalResults;
            var suggestionsString = await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Volumes, 0);
            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                reader.ReadToFollowing("number_of_total_results");
                totalResults = reader.ReadElementContentAsInt();
            }

            var pages = totalResults/100;

            for (int i = 0; i < pages+1; i++)
            {
                IEnumerable<string> results = MapSuggestionCharacters(await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Volumes, i * 100));
                foreach (string result in results.Where(result => !names.Contains(result)))
                {
                    names.Add(result);
                }
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile suggestionFile = await localFolder.CreateFileAsync("suggestionFile.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(suggestionFile, string.Join(",", names));
        }

        private async void FillSuggestions()
        {
            var uri = new Uri("ms-appx:///Assets/suggestionFile.txt");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);

            List<string> results = new List<string>((await FileIO.ReadTextAsync(file)).Split(','));
            _suggestionsList = new List<string>((await FileIO.ReadTextAsync(file)).Split(','));

        }

        private static IEnumerable<string> MapSuggestionCharacters(string suggestionListString)
        {
            return GetSuggestionsList(suggestionListString);
        }

        private static List<string> GetSuggestionsList(string suggestionsString)
        {
            List<string> names = new List<string>();

            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new List<string>();
                reader.ReadToFollowing("results");

                if (reader.Name != "volume") { reader.ReadToFollowing("volume"); }
                do
                {
                    if (reader.Name == "volume" && reader.NodeType != XmlNodeType.EndElement)
                    {

                        reader.ReadToFollowing("name");
                        var name = reader.ReadElementContentAsString();
                        names.Add(name);
                    }
                    else if (reader.Name == "results" && reader.NodeType == XmlNodeType.EndElement)
                    {
                        return names;
                    }
                }
                while (reader.Read());
            }
            return names;
        }
    }
}
