namespace MyWorldIsComics.ResourcePages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Navigation;

    using MyWorldIsComics.Common;
    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Mappers;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Character character;
        private Description description;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
            set { this.defaultViewModel = value; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public CharacterPage()
        {
            this.InitializeComponent();

            this.character = new Character();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.navigationHelper_LoadState;
            this.navigationHelper.SaveState += this.navigationHelper_SaveState;
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

            await this.LoadQuickCharacter(name);

            this.BioHubSection.Visibility = Visibility.Visible;

            await this.LoadDescription(this.DefaultViewModel["QuickCharacter"] as Character);

            await this.LoadCharacter(this.DefaultViewModel["QuickCharacter"] as Character);

            this.TeamSection.IsHeaderInteractive = true;

            await this.LoadFirstAppearance(this.DefaultViewModel["Character"] as Character, prevName);

            this.FirstAppearanceSection.IsHeaderInteractive = true;

            await this.LoadQuickTeams(this.DefaultViewModel["Character"] as Character, prevName);
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (this.Frame.CurrentSourcePageType.Name == "HubPage")
            {
                this.character = null;
            }
            else
            {
                // Save response content so don't have to fetch from api service again
                SavedData.QuickCharacter = this.DefaultViewModel["QuickCharacter"] as Character;
                SavedData.Character = this.DefaultViewModel["Character"] as Character;
                SavedData.FirstAppearance = this.DefaultViewModel["FirstAppearance"] as Issue;
            }
        }

        private async Task LoadQuickCharacter(string name)
        {
            if (SavedData.QuickCharacter != null && SavedData.QuickCharacter.Name == name)
            {
                this.DefaultViewModel["QuickCharacter"] = SavedData.QuickCharacter;
            }
            else
            {
                var quickCharacterString = await ComicVineSource.ExecuteSearchAsync(name);
                this.DefaultViewModel["QuickCharacter"] = this.MapQuickCharacter(quickCharacterString);
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
            description = characterDescription;
            this.DefaultViewModel["CharacterDescription"] = characterDescription;
        }

        private async Task LoadCharacter(Character quickCharacter)
        {
            if (SavedData.Character != null && SavedData.Character.Name == quickCharacter.Name)
            {
                this.character = SavedData.Character;
                this.DefaultViewModel["Character"] = this.character;
            }
            else
            {
                var characterString = await ComicVineSource.GetCharacterAsync(quickCharacter.UniqueId);
                this.character = this.MapCharacter(characterString);
                this.DefaultViewModel["Character"] = this.character;
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
                    Team team = this.MapQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                    if (character.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                    character.Teams.Add(team);
                }
            }
        }

        private Team MapQuickTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private async Task LoadFirstAppearance(Character character, string prevName)
        {
            if (SavedData.FirstAppearance != null && SavedData.Character != null && SavedData.Character.FirstAppearanceId == SavedData.FirstAppearance.UniqueId)
            {
                this.DefaultViewModel["FirstAppearance"] = SavedData.FirstAppearance;
            }
            else
            {
                Issue issue = this.MapIssue(await ComicVineSource.GetIssueAsync(character.FirstAppearanceId.ToString()));
                character.FirstAppearanceIssue = issue;
                this.DefaultViewModel["FirstAppearance"] = character.FirstAppearanceIssue;
            }
        }

        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
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
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        #region Event Listners

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            this.Frame.Navigate(typeof(TeamPage), team);
        }

        private void Page_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.BackButton.Visibility = this.BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            var characterToSend = this.character;
            this.Frame.Navigate(typeof(TeamsPage), characterToSend);
        }

        private void CurrentEventsWebView_Loaded(object sender, RoutedEventArgs e)
        {
            var webView = sender as WebView;
            if (webView != null && this.description != null)
            {
                webView.NavigateToString(this.description.CurrentEvents);
            }
        }

        #endregion
    }
}
