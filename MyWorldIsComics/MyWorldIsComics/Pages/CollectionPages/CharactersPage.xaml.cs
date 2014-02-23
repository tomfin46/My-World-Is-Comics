using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics.Pages.CollectionPages
{
    using MyWorldIsComics.Helpers;

    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class CharactersPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Team _team;
        private string _collectionName;

        private static Team SavedTeam;
        public static string CollectionName;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }


        public CharactersPage()
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
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (SavedData.Team != null)
            {
                _team = SavedData.Team;
            }
            else
            {
                var teamFromNav = e.NavigationParameter as Team;
                if (teamFromNav == null) return;

                _team = teamFromNav;
            }
            DefaultViewModel["Team"] = _team;

            switch (CollectionName)
            {
                case "Members":
                    itemsViewSource.Source = _team.Characters;
                    //DefaultViewModel["Items"] = _team.Characters;
                    break;
                case "Enemies":
                    itemsViewSource.Source = _team.Character_Enemies;
                    //DefaultViewModel["Items"] = _team.Character_Enemies;
                    break;
                case "Allies":
                    itemsViewSource.Source = _team.Character_Friends;
                    //DefaultViewModel["Items"] = _team.Character_Friends;
                    break;
            }
            pageTitle.Text = CollectionName;

        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Team = _team;
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

        private void GridView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            SavedData.Team = _team;

            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.Id);
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
