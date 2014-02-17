using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Pages;
using MyWorldIsComics.Pages.CollectionPages;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using Windows.Storage;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Common;
    using Data;

    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataModel.Interfaces;
    using MyWorldIsComics.Mappers;

    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private Results _trendingCharacters;
        private Character _topCharacter;

        private List<string> suggestionsList = new List<string>();
        private Dictionary<int, string> suggestionsDictionary = new Dictionary<int, string>();


        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        public HubPage()
        {
            InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;
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
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }
            if (MarvelWikiaSource.IsCanceled()) { MarvelWikiaSource.ReinstateCts(); }
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                await LoadTrendingCharacters();
            }
            catch (HttpRequestException)
            {
                pageTitle.Text = "An internet connection is required here";
            }

            //SearchTools.FetchSuggestions();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // Save response content so don't have to fetch from api service again
            Results results = DefaultViewModel["TrendingCharacters"] as Results;
            if (results == null) return;

            results.ResultsList.Insert(0, DefaultViewModel["TopCharacter"] as Character);
            SavedData.TrendingCharacters = results;
        }

        private async Task LoadTrendingCharacters()
        {
            if (SavedData.TrendingCharacters != null)
            {
                _topCharacter = SavedData.TrendingCharacters.ResultsList.First() as Character;
                DefaultViewModel["TopCharacter"] = _topCharacter;

                SavedData.TrendingCharacters.ResultsList.RemoveAt(0);
                _trendingCharacters = SavedData.TrendingCharacters;
                DefaultViewModel["TrendingCharacters"] = _trendingCharacters;
            }

            else
            {
                _trendingCharacters = new Results { Name = "Characters", ResultsList = new ObservableCollection<IResource>() };
                DefaultViewModel["TrendingCharacters"] = _trendingCharacters;

                var response = await MarvelWikiaSource.GetTrendingCharactersAsync();
                TrendingCharactersMapper tcm = new TrendingCharactersMapper();
                tcm.DeserializeJsonContent(response);

                int firstCharacter = await SetTopCharacter(tcm);
                DefaultViewModel["TopCharacter"] = _topCharacter;

                await FillTrendingCharacters(tcm, firstCharacter);
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
            int loopLimit = firstCharacter + 13;
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
            var title = tcm.GetResponseTitle(index);
            title = title.Substring(0, title.IndexOf('('));
            var results = await ComicVineSource.ExecuteSearchLimitOneAsync(title);
            var character = new CharacterMapper().MapTrendingCharactersXmlObject(results);
            
            return character.Name == null ? null : character;
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
            var itemId = ((Character)e.ClickedItem).UniqueId;
            Frame.Navigate(typeof(CharacterPage), itemId);
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void HeroImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var topChar = DefaultViewModel["TopCharacter"] as Character;
            var itemId = topChar.UniqueId;
            Frame.Navigate(typeof(CharacterPage), itemId);
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
    }
}
