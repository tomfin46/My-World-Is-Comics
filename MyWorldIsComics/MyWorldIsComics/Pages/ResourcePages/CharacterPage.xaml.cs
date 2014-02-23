using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private ObservableDictionary _characterPageViewModel = new ObservableDictionary();

        private Character _character;
        private Description _characterDescription;

        public ObservableDictionary CharacterPageViewModel
        {
            get { return _characterPageViewModel; }
            set { _characterPageViewModel = value; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public CharacterPage()
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
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }

            Character character = e.NavigationParameter as Character;

            int id;
            if (character != null)
            {
                id = character.Id;
                _character = character;
                PageTitle.Text = _character.Name;
                CharacterPageViewModel["Character"] = _character;
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
                if (SavedData.Character != null && SavedData.Character.Id == id) { _character = SavedData.Character; }
                else { _character = await GetCharacter(id); }
            }
            catch (HttpRequestException)
            {
                _character = new Character { Name = "An internet connection is required here" };
                CharacterPageViewModel["Character"] = _character;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }

            PageTitle.Text = _character.Name;
            CharacterPageViewModel["Character"] = _character;
            await LoadCharacter();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Character = _character;
        }

        #region Load Character

        private async Task<Character> GetCharacter(int id)
        {
            var characterSearchString = await ComicVineSource.GetCharacterAsync(id);
            return MapCharacter(characterSearchString);
        }

        private async Task LoadCharacter()
        {
            try
            {
                if (_character.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_character.First_Appeared_In_Issue.Volume == null)
                    {
                        await FetchFirstAppearance();
                    }
                    HideOrShowSections();

                    if (_character.Teams.Count > 0)
                    {
                        await FetchFirstTeamDetails();
                    }
                    HideOrShowSections();
                    if (_character.Teams.Count > 1) await FetchRemainingTeamsDetails();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _characterDescription.UniqueId = _character.Id;
            CreateDataTemplates();
        }

        private void HideOrShowSections()
        {
            TeamSection.Visibility = _character.Teams.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _character.First_Appeared_In_Issue.Image != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstTeamDetails()
        {
            foreach (var t in _character.Teams.Take(1))
            {
                Team team = MapTeam(await ComicVineSource.GetQuickTeamAsync(t.Id));
                _character.Teams.RemoveAt(0);
                _character.Teams.Insert(0, team);
            }
        }

        private async Task FetchRemainingTeamsDetails()
        {
            for (int i = 1; i < _character.Teams.Count; ++i)
            {
                Team team = MapTeam(await ComicVineSource.GetQuickTeamAsync(_character.Teams[i].Id));
                _character.Teams[i] = team;
            }
        }

        private async Task FetchFirstAppearance()
        {
            if (_character.First_Appeared_In_Issue.Id != 0)
            {
                _character.First_Appeared_In_Issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_character.First_Appeared_In_Issue.Id));
                _character.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_character.First_Appeared_In_Issue);
            }
        }

        #endregion

        #region Mapping Methods

        private Character MapCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound
                ? new Character { Name = "Character Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
        }

        private Team MapTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound
                ? new Team { Name = "Team Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseTeam>(teamString).Results;
        }

        private Issue MapIssue(string issueString)
        {
            return issueString == ServiceConstants.QueryNotFound
                ? new Issue { Name = "Issue Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseIssue>(issueString).Results;
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _characterDescription = await ComicVineSource.FormatDescriptionAsync(_character.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _characterDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
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

        #region Event Listners

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team.Id);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Teams" || e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _character);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _character.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _character.First_Appeared_In_Issue);
                    break;
                default:
                    Section section = _characterDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
                    Frame.Navigate(typeof(DescriptionSectionPage), section);
                    break;
            }
        }

        private void BioHubSection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HeaderBorder.Opacity = HeaderBorder.Opacity <= 0 ? 100 : 0;
            BackButton.Visibility = BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            BioHubSection.Visibility = BioHubSection.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            ImageHubSection.Visibility = ImageHubSection.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
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

        private void VolumeName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VolumePage), _character.First_Appeared_In_Issue.Volume.Id);
        }

        #endregion
    }
}
