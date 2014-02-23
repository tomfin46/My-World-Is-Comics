using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{
    #region usings

    using System.Net.Http;
    using DataModel.DescriptionContent;
    using Helpers;

    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class TeamPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _teamPageViewModel = new ObservableDictionary();

        private Team _team;
        private Description _teamDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary TeamPageViewModel
        {
            get { return _teamPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public TeamPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
            _navigationHelper.SaveState += navigationHelper_SaveState;
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

            Team team = e.NavigationParameter as Team;

            int id;
            if (team != null)
            {
                id = team.Id;
                _team = team;
                PageTitle.Text = _team.Name;
                TeamPageViewModel["Team"] = _team;
            }
            else
            {
                try
                {
                    id = int.Parse(e.NavigationParameter as string);
                }
                catch (ArgumentNullException)
                {
                    id = (int)e.NavigationParameter;
                }
            }

            try
            {
                if (SavedData.Team != null && SavedData.Team.Id == id) { _team = SavedData.Team; }
                else { _team = await GetTeam(id); }
            }
            catch (HttpRequestException)
            {
                _team = new Team { Name = "An internet connection is required here" };
                TeamPageViewModel["Team"] = _team;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }


            PageTitle.Text = _team.Name;
            TeamPageViewModel["Team"] = _team;
            await LoadTeam();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Team = _team;
        }

        #region Load Team

        private async Task LoadTeam()
        {
            try
            {
                if (_team.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    StatsHubSection.Visibility = Visibility.Visible;
                    BioHubSection.Visibility = Visibility.Visible;
                    HideOrShowSections();

                    await LoadDescription();

                    if (SavedData.Team == null || SavedData.Team.Id != _team.Id)
                    {
                        if (_team.First_Appeared_In_Issue.Volume == null) await FetchFirstAppearance();
                        HideOrShowSections();

                        await LoadFilters();

                        if (_team.Characters.Count > 1) await FetchRemainingMembers();
                        HideOrShowSections();
                        if (_team.Character_Enemies.Count > 1) await FetchRemainingEnemies();
                        HideOrShowSections();
                        if (_team.Character_Friends.Count > 1) await FetchRemainingFriends();
                        HideOrShowSections();
                    }
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
            var filters = new List<string> { "Disbanded_In_Issues", "Characters", "Character_Enemies", "Character_Friends" };
            foreach (string filter in filters)
            {
                var filteredTeamString = await ComicVineSource.GetFilteredTeamAsync(_team.Id, filter.ToLowerInvariant());

                var filterObj = JsonDeserialize.DeserializeJsonString<JsonSingularBaseTeam>(filteredTeamString);

                var teamType = _team.GetType();
                var prop = teamType.GetRuntimeProperty(filter);
                prop.SetValue(_team, prop.GetValue(filterObj.Results));

                if (_team.Disbanded_In_Issues.Count > 0) await FetchDisbandIssues();
                if (_team.Characters.Count > 0) await FetchFirstMember();
                if (_team.Character_Enemies.Count > 0) await FetchFirstEnemy();
                if (_team.Character_Friends.Count > 0) await FetchFirstFriend();
                HideOrShowSections();
            }
        }

        #endregion

        #region Load Description

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _teamDescription.UniqueId = _team.Id;
            CreateDataTemplates();
        }

        private async Task FormatDescriptionForPage()
        {
            _teamDescription = await ComicVineSource.FormatDescriptionAsync(_team.Description);
        }

        #endregion

        #region Fetch Methods

        private async Task FetchFirstAppearance()
        {
            if (_team.First_Appeared_In_Issue.Id != 0)
            {
                _team.First_Appeared_In_Issue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(_team.First_Appeared_In_Issue.Id));
                _team.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_team.First_Appeared_In_Issue);
                TeamPageViewModel["Team"] = _team;
            }
        }

        #region Fetch First

        private async Task FetchFirstMember()
        {
            Character character = await FetchCharacter(_team.Characters[0].Id);
            _team.Characters[0] = character;
        }

        private async Task FetchFirstEnemy()
        {
            Character character = await FetchCharacter(_team.Character_Enemies[0].Id);
            _team.Character_Enemies[0] = character;
        }

        private async Task FetchFirstFriend()
        {
            Character character = await FetchCharacter(_team.Character_Friends[0].Id);
            _team.Character_Friends[0] = character;
        }

        #endregion

        #region Fetch Remaining

        private async Task FetchDisbandIssues()
        {
            for (int i = 0; i < _team.Disbanded_In_Issues.Count; ++i)
            {
                Issue issue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(_team.Disbanded_In_Issues[i].Id));
                _team.Disbanded_In_Issues[i] = issue;
                HideOrShowSections();
            }
        }

        private async Task FetchRemainingMembers()
        {
            for (int i = 1; i < _team.Characters.Count; ++i)
            {
                Character character = await FetchCharacter(_team.Characters[i].Id);
                _team.Characters[i] = character;
            }
        }

        private async Task FetchRemainingEnemies()
        {
            for (int i = 1; i < _team.Character_Enemies.Count; ++i)
            {
                Character character = await FetchCharacter(_team.Character_Enemies[i].Id);
                _team.Character_Enemies[i] = character;
            }
        }

        private async Task FetchRemainingFriends()
        {
            for (int i = 1; i < _team.Character_Friends.Count; ++i)
            {
                Character character = await FetchCharacter(_team.Character_Friends[i].Id);
                _team.Character_Friends[i] = character;
            }
        }

        #endregion

        private async Task<Character> FetchCharacter(int characterId)
        {
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
        }

        #endregion

        #region Mapping Methods

        private Team MapTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound
                ? new Team { Name = "Team Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseTeam>(teamString).Results;
        }

        private Character GetMappedCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound
                ? new Character { Name = "Character Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
        }

        private Issue GetMappedIssue(string issueString)
        {
            return issueString == ServiceConstants.QueryNotFound
                ? new Issue { Name = "Issue Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseIssue>(issueString).Results;
        }

        #endregion

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _teamDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }

        #endregion

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _team.First_Appeared_In_Issue.Image != null ? Visibility.Visible : Visibility.Collapsed;
            IssuesDispandedInSection.Visibility = _team.Disbanded_In_Issues.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            MemberSection.Visibility = _team.Characters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            MemberSection.IsHeaderInteractive = _team.Characters.Count > 8;

            EnemiesSection.Visibility = _team.Character_Enemies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EnemiesSection.IsHeaderInteractive = _team.Character_Enemies.Count > 8;

            FriendSection.Visibility = _team.Character_Friends.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FriendSection.IsHeaderInteractive = _team.Character_Friends.Count > 8;
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
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Handlers

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "First Appearance" || header != "Members" || header != "Enemies"
                || header != "Allies" || header != "Issues Dispanded In") await FormatDescriptionForPage();
            switch (header)
            {
                case "First Appearance":
                    IssuePage.BasicIssue = _team.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _team.First_Appeared_In_Issue);
                    break;
                case "Members":
                case "Enemies":
                case "Allies":
                    CharactersPage.CollectionName = header;
                    Frame.Navigate(typeof(CharactersPage), _team);
                    break;
                case "Issues Dispanded In":
                    // TODO Frame.Navigate(typeof(IssuesPage), _team.IssuesDispandedIn);
                    break;
                default:
                    Section section = _teamDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
                    Frame.Navigate(typeof(DescriptionSectionPage), section);
                    break;
            }
        }

        private void GridView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character);
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
            Frame.Navigate(typeof(VolumePage), _team.First_Appeared_In_Issue.Volume);
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
