using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{
    #region usings

    using System.Net.Http;

    using Windows.UI.Xaml.Markup;

    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.Helpers;

    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class TeamPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary teamPageViewModel = new ObservableDictionary();

        private Team _team;
        private Description _teamDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary TeamPageViewModel
        {
            get { return teamPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        public TeamPage()
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

            int id;
            try
            {
                id = int.Parse(e.NavigationParameter as string);
            }
            catch (ArgumentNullException)
            {
                id = (int)e.NavigationParameter;
            }

            try
            {
                await this.LoadTeam(id);
            }
            catch (HttpRequestException)
            {
                _team = new Team { Name = "An internet connection is required here" };
                TeamPageViewModel["Team"] = _team;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }
            // Save response content so don't have to fetch from api service again
            SavedData.Team = _team;
        }

        #region Load Team
        
        private async Task LoadTeam(int id)
        {
            try
            {
                if (SavedData.Team != null && SavedData.Team.UniqueId == id) { _team = SavedData.Team; }
                else { _team = await GetTeam(id); }
                PageTitle.Text = _team.Name;
                
                TeamPageViewModel["Team"] = _team;

                if (_team.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    StatsHubSection.Visibility = Visibility.Visible;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_team.FirstAppearanceIssue == null) await FetchFirstAppearance();
                    HideOrShowSections();

                    await LoadFilters();
                    
                    if (_team.MemberIds.Count > 1) await this.FetchRemainingMembers();
                    HideOrShowSections();
                    if (_team.EnemyIds.Count > 1) await this.FetchRemainingEnemies();
                    HideOrShowSections();
                    if (_team.FriendIds.Count > 1) await this.FetchRemainingFriends();
                    HideOrShowSections();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task<Team> GetTeam(int id)
        {
            var teamSearchString = await ComicVineSource.GetTeamAsync(id);
            return MapTeam(teamSearchString);
        }

        private async Task LoadFilters()
        {
            List<string> filters = new List<string> { "disbanded_in_issues", "characters", "character_enemies", "character_friends" };
            foreach (string filter in filters)
            {
                var filteredTeamString = await ComicVineSource.GetFilteredTeamAsync(_team.UniqueId, filter);
                _team = new TeamMapper().MapFilteredXmlObject(_team, filteredTeamString, filter);

                if (_team.IssuesDispandedInIds.Count > 0 && _team.IssuesDispandedIn.Count == 0) await this.FetchDisbandIssues();
                if (_team.MemberIds.Count > 0 && _team.Members.Count == 0) await this.FetchFirstMember();
                if (_team.EnemyIds.Count > 0 && _team.Enemies.Count == 0) await this.FetchFirstEnemy();
                if (_team.FriendIds.Count > 0 && _team.Friends.Count == 0) await this.FetchFirstFriend();
                HideOrShowSections();
            }
        }

        private Team MapTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound ? new Team { Name = ServiceConstants.QueryNotFound } : new TeamMapper().MapXmlObject(teamString);
        } 

        #endregion

        #region Load Description
        
        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _teamDescription.UniqueId = _team.UniqueId;
            CreateDataTemplates();
        }

        private async Task FormatDescriptionForPage()
        {
            _teamDescription = await ComicVineSource.FormatDescriptionAsync(_team.DescriptionString);
        } 

        #endregion

        #region Fetch Methods
        
        private async Task FetchFirstAppearance()
        {
            if (_team.FirstAppearanceId != 0)
            {
                Issue issue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(_team.FirstAppearanceId));
                _team.FirstAppearanceIssue = issue;
            }
        }

        #region Fetch First

        private async Task FetchFirstMember()
        {
            foreach (var memberId in _team.MemberIds.Take(1).Where(memberId => _team.Members.All(m => m.UniqueId != memberId)))
            {
                _team.Members.Add(await FetchCharacter(memberId));
            }
        }

        private async Task FetchFirstEnemy()
        {
            foreach (var enemyId in _team.EnemyIds.Take(1).Where(enemyId => _team.Enemies.All(e => e.UniqueId != enemyId)))
            {
                _team.Enemies.Add(await FetchCharacter(enemyId));
            }
        }

        private async Task FetchFirstFriend()
        {
            foreach (var friendId in _team.FriendIds.Take(1).Where(friendId => _team.Friends.All(f => f.UniqueId != friendId)))
            {
                _team.Friends.Add(await FetchCharacter(friendId));
            }
        }

        #endregion

        #region Fetch Remaining
        private async Task FetchDisbandIssues()
        {
            foreach (var issueId in _team.IssuesDispandedInIds.Where(issueId => _team.IssuesDispandedIn.All(i => i.UniqueId != issueId)))
            {
                _team.IssuesDispandedIn.Add(GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(issueId)));
                this.HideOrShowSections();
            }
        }

        private async Task FetchRemainingMembers()
        {
            var firstId = _team.Members.First().UniqueId;
            foreach (int memberId in _team.MemberIds.Where(id => id != firstId).Take(8))
            {
                Character character = await FetchCharacter(memberId);
                if (_team.Members.Any(m => m.UniqueId == character.UniqueId)) continue;
                _team.Members.Add(character);
            }
        }

        private async Task FetchRemainingEnemies()
        {
            var firstId = _team.Enemies.First().UniqueId;
            foreach (int enemyId in _team.EnemyIds.Where(id => id != firstId).Take(8))
            {
                Character character = await FetchCharacter(enemyId);
                if (_team.Enemies.Any(e => e.UniqueId == character.UniqueId)) continue;
                _team.Enemies.Add(character);
            }
        }

        private async Task FetchRemainingFriends()
        {
            var firstId = _team.Friends.First().UniqueId;
            foreach (int friendId in _team.FriendIds.Where(id => id != firstId).Take(8))
            {
                Character character = await FetchCharacter(friendId);
                if (_team.Friends.Any(f => f.UniqueId == character.UniqueId)) continue;
                _team.Friends.Add(character);
            }
        }

        #endregion

        private async Task<Character> FetchCharacter(int characterId)
        {
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
        }

        #endregion

        #region Mapping Methods

        private Team GetMappedTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().MapXmlObject(teamString);
        }
        
        private Character GetMappedCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
        }

        private Issue GetMappedIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _teamDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section, i));
                i++;
            }
        }

        #endregion

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _team.FirstAppearanceIssue != null ? Visibility.Visible : Visibility.Collapsed;
            IssuesDispandedInSection.Visibility = _team.IssuesDispandedIn.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            MemberSection.Visibility = _team.Members.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            MemberSection.IsHeaderInteractive = _team.Members.Count > 8;

            EnemiesSection.Visibility = _team.Enemies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EnemiesSection.IsHeaderInteractive = _team.Enemies.Count > 8;

            FriendSection.Visibility = _team.Friends.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FriendSection.IsHeaderInteractive = _team.Friends.Count > 8;
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

        #region Event Handlers

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "First Appearance" || header != "Members" || header != "Enemies"
                || header != "Allies" || header != "Issues Dispanded In") await this.FormatDescriptionForPage();
            switch (header)
            {
                case "First Appearance":
                    IssuePage.BasicIssue = _team.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _team.FirstAppearanceIssue);
                    break;
                case "Members":
                    Frame.Navigate(typeof(CharactersPage), new Dictionary<String, Team> { { "members", this._team } });
                    break;
                case "Enemies":
                    Frame.Navigate(typeof(CharactersPage), new Dictionary<String, Team> { { "enemies", this._team } });
                    break;
                case "Allies":
                    Frame.Navigate(typeof(CharactersPage), new Dictionary<String, Team> { { "friends", this._team } });
                    break;
                case "Issues Dispanded In":
                    // TODO Frame.Navigate(typeof(IssuesPage), _team.IssuesDispandedIn);
                    break;
                default:
                    Section section = _teamDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
                    Frame.Navigate(typeof (DescriptionSectionPage), section);
                    break;
            }
        }

        private void GridView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.UniqueId);
        }

        private void GridView_IssueClick(object sender, ItemClickEventArgs e)
        {
            var issue = ((Issue)e.ClickedItem);
            IssuePage.BasicIssue = issue;
            Frame.Navigate(typeof(IssuePage), issue);
        }
        
        private void StatsHubSection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HeaderBorder.Opacity = HeaderBorder.Opacity <= 0 ? 100 : 0;
            BackButton.Visibility = BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            StatsHubSection.Visibility = StatsHubSection.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            ImageHubSection.Visibility = ImageHubSection.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void VolumeName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VolumePage), _team.FirstAppearanceIssue.VolumeId);
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

        #endregion
    }
}
