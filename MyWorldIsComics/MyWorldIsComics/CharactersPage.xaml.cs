using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MyWorldIsComics.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229
using MyWorldIsComics.ResourcePages;

namespace MyWorldIsComics
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Mappers;

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
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public CharactersPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
            if (SavedTeam != null && SavedCollectionName != null && e.NavigationParameter == null)
            {
                this.team = SavedTeam;
                this.collectionName = SavedCollectionName;
            }
            else
            {
                var teamFromNav = e.NavigationParameter as Dictionary<string, Team>;
                if (teamFromNav == null) return;

                foreach (KeyValuePair<string, Team> keyValuePair in teamFromNav.Take(1))
                {
                    this.team = keyValuePair.Value;
                    this.collectionName = keyValuePair.Key;
                } 
            }

            switch (this.collectionName)
            {
                case "members":
                    this.collection = this.team.Members;
                    this.collectionIds = this.team.MemberIds;
                    break;
                case "enemies":
                    this.collection = this.team.Enemies;
                    this.collectionIds = this.team.EnemyIds;
                    break;
                case "friends":
                    this.collection = this.team.Friends;
                    this.collectionIds = this.team.FriendIds;
                    break;
            }

            this.DefaultViewModel["Team"] = this.team;
            this.DefaultViewModel["Items"] = this.collection;

            try
            {
                foreach (int characterId in this.collectionIds.GetRange(this.collection.Count, this.collectionIds.Count - this.collection.Count))
                {
                    Character character = await FetchCharacter(characterId);
                    if (this.collection.Any(m => m.UniqueId == character.UniqueId)) continue;
                    this.collection.Add(character);
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
            SavedTeam = this.team;
            SavedCollectionName = this.collectionName;
        }

        private async Task<Character> FetchCharacter(int characterId)
        {
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId.ToString()));
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
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.Name);
        }
    }
}
