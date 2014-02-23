using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Interfaces;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages;
using MyWorldIsComics.Pages.ResourcePages;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

namespace MyWorldIsComics
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();

        private Results _trendingCharacters;
        private Character _topCharacter;
        private int _randLimit;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }

        public HubPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
            _navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //SearchTools.FetchSuggestions();

            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }
            if (MarvelWikiaSource.IsCanceled()) { MarvelWikiaSource.ReinstateCts(); }

            try
            {
                await CalcCharacterLimit();
                await LoadTrendingCharacters();
            }
            catch (HttpRequestException)
            {
                pageTitle.Text = "An internet connection is required here";
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }

        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            _trendingCharacters.ResultsList.Insert(0, DefaultViewModel["RandomCharacter"] as Character);
            _trendingCharacters.ResultsList.Insert(0, DefaultViewModel["TopCharacter"] as Character);
            
            SavedData.TrendingCharacters = _trendingCharacters;
        }

        private async Task LoadTrendingCharacters()
        {
            if (SavedData.TrendingCharacters != null)
            {
                _topCharacter = SavedData.TrendingCharacters.ResultsList.First() as Character;
                DefaultViewModel["TopCharacter"] = _topCharacter;
                SavedData.TrendingCharacters.ResultsList.RemoveAt(0);

                if (SavedData.TrendingCharacters.ResultsList.Count > 1)
                {
                    var character = SavedData.TrendingCharacters.ResultsList.First() as Character;
                    DefaultViewModel["RandomCharacter"] = character;
                    SavedData.TrendingCharacters.ResultsList.RemoveAt(0);
                }

                _trendingCharacters = SavedData.TrendingCharacters;
                DefaultViewModel["TrendingCharacters"] = _trendingCharacters;
            }

            else
            {
                _trendingCharacters = new Results { Name = "Characters", ResultsList = new ObservableCollection<IResponse>() };
                DefaultViewModel["TrendingCharacters"] = _trendingCharacters;

                //var response = ServiceConstants.QueryNotFound;
                var response = await MarvelWikiaSource.GetTrendingCharactersAsync();
                if (response != ServiceConstants.QueryNotFound)
                {
                    TrendingCharactersMapper tcm = new TrendingCharactersMapper();
                    tcm.DeserializeJsonContent(response);

                    int firstCharacter = await SetTopCharacter(tcm);
                    DefaultViewModel["TopCharacter"] = _topCharacter;

                    DefaultViewModel["RandomCharacter"] = await GetRandomCharacter();
                    await FillTrendingCharacters(tcm, firstCharacter);
                }
                else
                {
                    response = await ComicVineSource.GetCharacterAsync(90035);
                    var jsonResponse = JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(response);

                    //response = await ComicVineSource.GetLatestUpdatedCharacters();
                    //var jsonResponse = JsonDeserialize.DeserializeJsonString<JsonMultipleBaseCharacter>(response);
                    var characters = jsonResponse.Results;

                    _topCharacter = characters/*.First()*/;
                    DefaultViewModel["TopCharacter"] = _topCharacter;

                    DefaultViewModel["RandomCharacter"] = await GetRandomCharacter();
                }

            }
        }

        private async Task<int> SetTopCharacter(TrendingCharactersMapper tcm)
        {
            int firstChar = 0;
            _topCharacter = null;

            while (_topCharacter == null)
            {
                var topChar = await GetTrendingCharacter(tcm, firstChar);
                if (topChar != null)
                {
                    _topCharacter = topChar;
                }
                else
                {
                    ++firstChar;
                }
            }

            return firstChar;
        }

        private async Task FillTrendingCharacters(TrendingCharactersMapper tcm, int firstCharacter)
        {
            int loopLimit = firstCharacter + 23;
            for (int i = firstCharacter + 1; i < loopLimit; ++i)
            {
                var trendChar = await GetTrendingCharacter(tcm, i);
                if (trendChar != null)
                {
                    _trendingCharacters.ResultsList.Add(trendChar);
                }
                else
                {
                    ++loopLimit;
                }
            }
        }

        private async Task<Character> GetTrendingCharacter(TrendingCharactersMapper tcm, int index)
        {
            var charName = tcm.ExtractCurrentAlias(index);
            if (charName == ServiceConstants.AliasNotFound)
            {
                charName = tcm.GetResponseTitle(index);
                charName = charName.Substring(0, charName.IndexOf(" (", System.StringComparison.Ordinal));
            }

            var results = await ComicVineSource.ExecuteCharacterFilterLimitOneAsync(charName);
            return MapTrendingCharacter(results);
        }

        private Character MapTrendingCharacter(string characterString)
        {
            try
            {
                var json = JsonDeserialize.DeserializeJsonString<JsonMultipleBaseCharacter>(characterString);
                return json.Number_Of_Total_Results > 0 ? json.Results[0] : null;
            }
            catch (JsonSerializationException)
            {
                return characterString == ServiceConstants.QueryNotFound
                    ? new Character {Name = "Character Not Found"}
                    : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private async Task CalcCharacterLimit()
        {
            var response = await ComicVineSource.GetOffsetCharacterIds(0);
            var jsonResponse = JsonDeserialize.DeserializeJsonString<JsonMultipleBaseCharacter>(response);
            response = await ComicVineSource.GetOffsetCharacterIds(jsonResponse.Number_Of_Total_Results - 1);
            jsonResponse = JsonDeserialize.DeserializeJsonString<JsonMultipleBaseCharacter>(response);

            _randLimit = jsonResponse.Results.First().Id + 1;
        }

        private async Task<Character> GetRandomCharacter()
        {
            string response;
            Random rand = new Random();
            Character character;
            do
            {
                var randId = rand.Next(_randLimit);
                response = await ComicVineSource.GetCharacterAsync(randId);
            } while (response == ServiceConstants.QueryNotFound);

            var charResponse = JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(response);
            character = charResponse.Results;

            return character;
        }

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character);
        }
        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void HeroImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(CharacterPage), DefaultViewModel["TopCharacter"] as Character);
        }

        private void RandHeroImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var textBlock = e.OriginalSource as TextBlock;
            if (textBlock != null && textBlock.Text == "New Random Character")
            {
                RandCharGen(sender, e);
            }
            else
            {
                Frame.Navigate(typeof(CharacterPage), DefaultViewModel["RandomCharacter"] as Character); 
            }
        }

        private void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            new SearchTools().SearchBoxEventsSuggestionsRequested(args);
        }

        private void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var queryText = args.QueryText;
            if (string.IsNullOrEmpty(queryText)) return;
            Frame.Navigate(typeof(SearchResultsPage), queryText);
        }

        private async void RandCharGen(object sender, TappedRoutedEventArgs e)
        {
            DefaultViewModel["RandomCharacter"] = await GetRandomCharacter();
        }
    }
}
