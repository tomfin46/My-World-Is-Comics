// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
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
        private Team team;
        private string collectionName;
        private ObservableCollection<Character> collection;
        private List<int> collectionIds;

        private static Team SavedTeam;
        private static string SavedCollectionName;

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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }

            if (SavedTeam != null && SavedCollectionName != null && e.NavigationParameter == null)
            {
                team = SavedTeam;
                collectionName = SavedCollectionName;
            }
            else
            {
                var teamFromNav = e.NavigationParameter as Dictionary<string, Team>;
                if (teamFromNav == null) return;

                foreach (KeyValuePair<string, Team> keyValuePair in teamFromNav.Take(1))
                {
                    team = keyValuePair.Value;
                    collectionName = keyValuePair.Key;
                } 
            }

            switch (collectionName)
            {
                case "members":
                    collection = team.Members;
                    collectionIds = team.MemberIds;
                    break;
                case "enemies":
                    collection = team.Enemies;
                    collectionIds = team.EnemyIds;
                    break;
                case "friends":
                    collection = team.Friends;
                    collectionIds = team.FriendIds;
                    break;
            }

            DefaultViewModel["Team"] = team;
            pageTitle.Text = char.ToUpper(collectionName[0]) + collectionName.Substring(1);
            DefaultViewModel["Items"] = collection;

            try
            {
                foreach (int characterId in collectionIds.GetRange(collection.Count, collectionIds.Count - collection.Count))
                {
                    Character character = GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
                    if (collection.Any(m => m.UniqueId == character.UniqueId)) continue;
                    collection.Add(character);
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }
            // Save response content so don't have to fetch from api service again
            SavedTeam = team;
            SavedCollectionName = collectionName;
        }

        private Character GetMappedCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
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
            SavedTeam = team;
            SavedCollectionName = collectionName;

            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.UniqueId);
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
