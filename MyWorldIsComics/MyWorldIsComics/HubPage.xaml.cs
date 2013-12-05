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

            FetchSuggestions();
        }

        private async void FetchSuggestions()
        {
            List<string> characters = new List<string>();
            int totalResults;
            var suggestionsString = await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, 0);
            using (XmlReader reader = XmlReader.Create(new StringReader(suggestionsString)))
            {
                reader.ReadToFollowing("number_of_total_results");
                totalResults = reader.ReadElementContentAsInt();
            }

            for (int i = 100; i < totalResults/100; i+=100)
            {
                characters.AddRange(MapSuggestionCharacters(await ComicVineSource.GetSuggestionList(DataModel.Enums.Resources.ResourcesEnum.Characters, i)));
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.CreateFileAsync("suggestionFile.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, String.Join(",", characters));
        }

        private IEnumerable<string> MapSuggestionCharacters(string suggestionListString)
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
            foreach (string suggestion in suggestionList.Where(suggestion => suggestion.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)))
            {
                suggestionCollection.AppendQuerySuggestion(suggestion);
            }
        }

        private void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var queryText = args.QueryText;
            if (!string.IsNullOrEmpty(queryText))
            {
                // TODO Frame.Navigate(SearchResultsPage, queryText);
                //MainPage.Current.NotifyUser(queryText, NotifyType.StatusMessage);
            }
        }

        /// <summary>
        /// App provided suggestions list
        /// </summary>
        private static readonly string[] suggestionList =
            {
                "Shanghai", "Istanbul", "Karachi", "Delhi", "Mumbai", "Moscow", "São Paulo", "Seoul", "Beijing", "Jakarta",
                "Tokyo", "Mexico City", "Kinshasa", "New York City", "Lagos", "London", "Lima", "Bogota", "Tehran", "Ho Chi Minh City",
                "Hong Kong", "Bangkok", "Dhaka", "Cairo", "Hanoi", "Rio de Janeiro", "Lahore", "Chonquing", "Bengaluru", "Tianjin",
                "Baghdad", "Riyadh", "Singapore", "Santiago", "Saint Petersburg", "Surat", "Chennai", "Kolkata", "Yangon", "Guangzhou",
                "Alexandria", "Shenyang", "Hyderabad", "Ahmedabad", "Ankara", "Johannesburg", "Wuhan", "Los Angeles", "Yokohama",
                "Abidjan", "Busan", "Cape Town", "Durban", "Pune", "Jeddah", "Berlin", "Pyongyang", "Kanpur", "Madrid", "Jaipur",
                "Nairobi", "Chicago", "Houston", "Philadelphia", "Phoenix", "San Antonio", "San Diego", "Dallas", "San Jose",
                "Jacksonville", "Indianapolis", "San Francisco", "Austin", "Columbus", "Fort Worth", "Charlotte", "Detroit",
                "El Paso", "Memphis", "Baltimore", "Boston", "Seattle Washington", "Nashville", "Denver", "Louisville", "Milwaukee",
                "Portland", "Las Vegas", "Oklahoma City", "Albuquerque", "Tucson", "Fresno", "Sacramento", "Long Beach", "Kansas City",
                "Mesa", "Virginia Beach", "Atlanta", "Colorado Springs", "Omaha", "Raleigh", "Miami", "Cleveland", "Tulsa", "Oakland",
                "Minneapolis", "Wichita", "Arlington", "Bakersfield", "New Orleans", "Honolulu", "Anaheim", "Tampa", "Aurora",
                "Santa Ana", "St. Louis", "Pittsburgh", "Corpus Christi", "Riverside", "Cincinnati", "Lexington", "Anchorage",
                "Stockton", "Toledo", "St. Paul", "Newark", "Greensboro", "Buffalo", "Plano", "Lincoln", "Henderson", "Fort Wayne",
                "Jersey City", "St. Petersburg", "Chula Vista", "Norfolk", "Orlando", "Chandler", "Laredo", "Madison", "Winston-Salem",
                "Lubbock", "Baton Rouge", "Durham", "Garland", "Glendale", "Reno", "Hialeah", "Chesapeake", "Scottsdale",
                "North Las Vegas", "Irving", "Fremont", "Irvine", "Birmingham", "Rochester", "San Bernardino", "Spokane",
                "Toronto", "Montreal", "Vancouver", "Ottawa-Gatineau", "Calgary", "Edmonton", "Quebec City", "Winnipeg", "Hamilton"
            };
    }
}
