

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;

namespace MyWorldIsComics
{
    using Common;
    using DataModel.Resources;
    using DataSource;
    using Mappers;
    using ResourcePages;
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Character character;


        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
            set { defaultViewModel = value; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        public CharacterPage()
        {
            InitializeComponent();

            character = new Character();

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
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            string name = e.NavigationParameter as string;

            string prevName = "";
            if (SavedData.QuickCharacter != null)
            {
                prevName = SavedData.QuickCharacter.Name;
            }

            await LoadQuickCharacter(name);

            BioHubSection.Visibility = Visibility.Visible;

            //await LoadDescription(DefaultViewModel["QuickCharacter"] as Character);

            await LoadCharacter(DefaultViewModel["QuickCharacter"] as Character);

            TeamSection.IsHeaderInteractive = true;

            await LoadQuickTeams(DefaultViewModel["Character"] as Character, prevName);
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        private async Task LoadQuickCharacter(string name)
        {
            if (SavedData.QuickCharacter != null && SavedData.QuickCharacter.Name == name)
            {
                DefaultViewModel["QuickCharacter"] = SavedData.QuickCharacter;
            }
            else
            {
                var quickCharacterString = await ComicVineSource.ExecuteSearchAsync(name);
                DefaultViewModel["QuickCharacter"] = MapQuickCharacter(quickCharacterString);
            }
        }

        private Character MapQuickCharacter(string quickCharacterString)
        {
            if (quickCharacterString == ServiceConstants.QueryNotFound) return new Character();
            Character quickCharacter;
            CharacterMapper.QuickMapXmlObject(quickCharacterString, out quickCharacter);
            return quickCharacter;
        }

        private async Task LoadDescription(Character quickCharacter)
        {
            var characterDescription = await ComicVineSource.FormatDescriptionAsync(quickCharacter.DescriptionString);
            DefaultViewModel["CharacterDescription"] = characterDescription;
        }

        private async Task LoadCharacter(Character quickCharacter)
        {
            if (SavedData.Character != null && SavedData.Character.Name == quickCharacter.Name)
            {
                character = SavedData.Character;
                DefaultViewModel["Character"] = character;
            }
            else
            {
                var characterString = await ComicVineSource.GetCharacterAsync(quickCharacter.UniqueId);
                character = MapCharacter(characterString);
                DefaultViewModel["Character"] = character;
            }
        }

        private Character MapCharacter(string characterString)
        {
            if (characterString == ServiceConstants.QueryNotFound)
            {
                return new Character
                {
                    TeamIds = new List<int>(),
                    Teams = new ObservableCollection<Team>()
                    {
                        new Team
                        {
                            Name = characterString
                        }
                    }
                };
            }

            Character characterToMap;
            CharacterMapper.MapXmlObject(characterString, out characterToMap);
            return characterToMap;
        }

        private async Task LoadQuickTeams(Character character, string prevName)
        {
            if (SavedData.Character == null || character.Name != prevName || SavedData.Character.Teams == null)
            {
                foreach (int teamId in character.TeamIds.Take(10))
                {
                    Team team = MapQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                    if (character.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                    character.Teams.Add(team);
                }
            }
        }

        private Team MapQuickTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
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


        #region Event Listners

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            // Save response content so don't have to fetch from api service again
            SavedData.QuickCharacter = DefaultViewModel["QuickCharacter"] as Character;
            SavedData.Character = DefaultViewModel["Character"] as Character;
            //SavedData.QuickTeams = this.DefaultViewModel["QuickTeams"] as List<Team>;

            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team);
        }

        private void Page_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            BackButton.Visibility = BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            var characterToSend = character;
            Frame.Navigate(typeof(TeamsPage), characterToSend);
        }

        #endregion
    }
}
