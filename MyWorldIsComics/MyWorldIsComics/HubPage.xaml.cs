using Windows.UI.Xaml.Input;
using MyWorldIsComics.Pages;
using MyWorldIsComics.Pages.CollectionPages;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics
{
    #region usings

    using System;
    using System.Collections.Generic;
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
    using MyWorldIsComics.Mappers;

    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private string[] suggestionsList;

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
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-3");
            DefaultViewModel["Section3Items"] = sampleDataGroup;

            this.FetchSuggestions();
            this.FillSuggestions();
        }

        private async void FetchSuggestions()
        {
            Dictionary<int, string> characters = new Dictionary<int, string>();
            StringBuilder sb = new StringBuilder();
            int totalResults;
            var suggestionsString = await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, 0);
            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                reader.ReadToFollowing("number_of_total_results");
                totalResults = reader.ReadElementContentAsInt();
            }

            for (int i = 0; i < totalResults+100; i+=100)
            {
                var dictReturn = MapSuggestionCharacters(await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, i));
                foreach (KeyValuePair<int, string> keyValuePair in dictReturn)
                {
                    characters.Add(keyValuePair.Key, keyValuePair.Value);
                    sb.Append(keyValuePair.Key + ":" + keyValuePair.Value + ",");
                }
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.CreateFileAsync("suggestionFile.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, sb.ToString());
        }

        private async void FillSuggestions()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.GetFileAsync("suggestionFile.txt");
            suggestionsList = (await FileIO.ReadTextAsync(sampleFile)).Split(',');
        }

        private IDictionary<int, string> MapSuggestionCharacters(string suggestionListString)
        {
            return new CharacterMapper().GetSuggestionsList(suggestionListString);
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
            Frame.Navigate(typeof(TeamsPage), ((SampleDataGroup)group).UniqueId);
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
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
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
            Frame.Navigate(typeof(FurtherDescription));
        }

        private void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            string queryText = args.QueryText;
            if (string.IsNullOrEmpty(queryText)) return;
            Windows.ApplicationModel.Search.SearchSuggestionCollection suggestionCollection = args.Request.SearchSuggestionCollection;

            foreach (string suggestion in suggestionsList.Where(suggestion => suggestion.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)))
            {
                suggestionCollection.AppendQuerySuggestion(suggestion);
            }
        }

        private void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var queryText = args.QueryText;
            if (!string.IsNullOrEmpty(queryText))
            {
                Frame.Navigate(typeof(SearchResultsPage), queryText);
            }
        }
    }
}
