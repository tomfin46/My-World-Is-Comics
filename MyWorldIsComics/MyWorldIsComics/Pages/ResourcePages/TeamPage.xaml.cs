using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{
    #region usings



    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class TeamPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary teamPageViewModel = new ObservableDictionary();

        private Team _team;

        private static Team SavedTeam;

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

            var team = e.NavigationParameter as Team;

            if (SavedTeam != null && team != null && SavedTeam.Name == team.Name)
            {
                _team = SavedTeam;
                this.HideOrShowFilteredSections();
            }
            else
            {
                _team = team;
                await this.LoadTeam();
            }
            TeamPageViewModel["Team"] = _team;
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }
            // Save response content so don't have to fetch from api service again
            SavedTeam = _team;
        }

        private async Task LoadTeam()
        {
            try
            {
                List<string> filters = new List<string> { "first_appeared_in_issue", "disbanded_in_issues", "characters", "character_enemies", "character_friends" };

                foreach (string filter in filters)
                {
                    await FetchFilteredTeamResource(filter);
                    switch (filter)
                    {
                        case "first_appeared_in_issue":
                            await FetchFirstAppearance();
                            break;
                        case "disbanded_in_issues":
                            await FetchDisbandIssues();
                            break;
                        case "characters":
                            await FetchFirstMember();
                            break;
                        case "character_enemies":
                            await FetchFirstEnemy();
                            break;
                        case "character_friends":
                            await FetchFirstFriend();
                            break;
                    }
                    TeamPageViewModel["Team"] = _team;
                    HideOrShowFilteredSections();

                    if (_team.MemberIds.Count > 1) await FetchRemainingMembers();
                    HideOrShowFilteredSections();
                    if (_team.EnemyIds.Count > 1) await FetchRemainingEnemies();
                    HideOrShowFilteredSections();
                    if (_team.FriendIds.Count > 1) await FetchRemainingFriends();
                    HideOrShowFilteredSections();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        #region Fetch Methods

        private async Task FetchFilteredTeamResource(string filter)
        {
            string filteredTeamString = await ComicVineSource.GetFilteredTeamAsync(_team.UniqueId, filter);
            _team = GetMappedTeamFromFilter(filteredTeamString, filter);
        }

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
                HideOrShowFilteredSections();
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
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId.ToString()));
        }

        #endregion

        #region Mapping Methods

        private Character GetMappedCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
        }

        private Team GetMappedTeamFromFilter(string filteredTeamString, string filter)
        {
            if (filteredTeamString == ServiceConstants.QueryNotFound)
            {
                return new Team
                {
                    FirstAppearanceIssue = new Issue
                    {
                        Name = filteredTeamString
                    }
                };
            }
            return new TeamMapper().MapFilteredXmlObject(_team, filteredTeamString, filter);
        }

        private Issue GetMappedIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private void HideOrShowFilteredSections()
        {
            FirstAppearanceSection.Visibility = _team.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
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

        private void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            switch (e.Section.Header.ToString())
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
                    // TODO Frame.Navigate(typeof(IssuesPage), _team.Friends);
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
    }
}
