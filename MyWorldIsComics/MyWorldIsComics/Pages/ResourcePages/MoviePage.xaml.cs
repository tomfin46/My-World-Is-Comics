using MyWorldIsComics.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

namespace MyWorldIsComics.Pages.ResourcePages
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Helpers;
    using MyWorldIsComics.Mappers;
    using MyWorldIsComics.Pages.CollectionPages;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class MoviePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary moviePageViewModel = new ObservableDictionary();

        private Movie _movie;

        private Description _movieDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary MoviePageViewModel
        {
            get { return this.moviePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public MoviePage()
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
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
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
                await LoadMovie(id);
            }
            catch (HttpRequestException)
            {
                _movie = new Movie { Name = "An internet connection is required here" };
                MoviePageViewModel["Movie"] = _movie;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Movie = _movie;
        }

        #region Load Movie

        private async Task LoadMovie(int id)
        {
            try
            {
                if (SavedData.Movie != null && SavedData.Movie.UniqueId == id) { _movie = SavedData.Movie; }
                else { _movie = await this.GetMovie(id); }
                PageTitle.Text = _movie.Name;

                MoviePageViewModel["Movie"] = _movie;

                if (_movie.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    await LoadFilters();

                    if (_movie.CharacterIds.Count > 1) await this.FetchRemainingCharacters();
                    HideOrShowSections();
                    if (_movie.ConceptIds.Count > 1) await this.FetchRemainingConcepts();
                    HideOrShowSections();
                    if (_movie.LocationIds.Count > 1) await this.FetchRemainingLocations();
                    HideOrShowSections();
                    if (_movie.TeamIds.Count > 1) await this.FetchRemainingTeams();
                    HideOrShowSections();
                    if (_movie.WriterIds.Count > 1) await this.FetchRemainingWriters();
                    HideOrShowSections();
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
            _movieDescription.UniqueId = _movie.UniqueId;
            CreateDataTemplates();
        }

        private async Task<Movie> GetMovie(int id)
        {
            var movieSearchString = await ComicVineSource.GetMovieAsync(id);
            return this.MapMovie(movieSearchString);
        }

        private async Task LoadFilters()
        {
            List<string> filters = new List<string> { "characters", "concepts", "locations", "producers", "teams", "writers" };
            foreach (string filter in filters)
            {
                var filteredMovieString = await ComicVineSource.GetFilteredMovieAsync(_movie.UniqueId, filter);
                _movie = new MovieMapper().MapFilteredXmlObject(_movie, filteredMovieString, filter);

                if (_movie.CharacterIds.Count > 0 && _movie.Characters.Count == 0) await this.FetchFirstCharacter();
                if (_movie.ConceptIds.Count > 0 && _movie.Concepts.Count == 0) await this.FetchFirstConcept();
                if (_movie.LocationIds.Count > 0 && _movie.Locations.Count == 0) await this.FetchFirstLocation();
                if (_movie.TeamIds.Count > 0 && _movie.Teams.Count == 0) await this.FetchFirstTeam();
                if (_movie.WriterIds.Count > 0 && _movie.Writers.Count == 0) await this.FetchFirstWriter();
                HideOrShowSections();
            }
        }

        private Movie MapMovie(string movieString)
        {
            return movieString == ServiceConstants.QueryNotFound ? new Movie { Name = ServiceConstants.QueryNotFound } : new MovieMapper().MapXmlObject(movieString);
        }

        private void HideOrShowSections()
        {
            CharacterSection.Visibility = _movie.Characters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ConceptSection.Visibility = _movie.Concepts.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            LocationSection.Visibility = _movie.Locations.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            TeamSection.Visibility = _movie.Teams.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            WriterSection.Visibility = _movie.Writers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch Methods

        private async Task<Character> FetchCharacter(int characterId)
        {
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
        }

        private async Task<Concept> FetchConcept(int conceptId)
        {
            return GetMappedConcept(await ComicVineSource.GetQuickConceptAsync(conceptId));
        }

        private async Task<Location> FetchLocation(int locationId)
        {
            return GetMappedLocation(await ComicVineSource.GetQuickLocationAsync(locationId));
        }

        private async Task<Team> FetchTeam(int teamId)
        {
            return GetMappedTeam(await ComicVineSource.GetQuickTeamAsync(teamId));
        }

        private async Task<Creator> FetchWriter(int writerId)
        {
            return GetMappedWriter(await ComicVineSource.GetQuickCreatorAsync(writerId));
        }

        #region Fetch First

        private async Task FetchFirstCharacter()
        {
            foreach (var characterId in _movie.CharacterIds.Take(1).Where(characterId => _movie.Characters.All(c => c.UniqueId != characterId)))
            {
                _movie.Characters.Add(await FetchCharacter(characterId));
            }
        }

        private async Task FetchFirstConcept()
        {
            foreach (var conceptId in _movie.ConceptIds.Take(1).Where(conceptId => _movie.Concepts.All(c => c.UniqueId != conceptId)))
            {
                _movie.Concepts.Add(await FetchConcept(conceptId));
            }
        }

        private async Task FetchFirstLocation()
        {
            foreach (var locationId in _movie.LocationIds.Take(1).Where(locationId => _movie.Locations.All(l => l.UniqueId != locationId)))
            {
                _movie.Locations.Add(await FetchLocation(locationId));
            }
        }

        private async Task FetchFirstTeam()
        {
            foreach (var teamId in _movie.TeamIds.Take(1).Where(teamId => _movie.Teams.All(t => t.UniqueId != teamId)))
            {
                _movie.Teams.Add(await FetchTeam(teamId));
            }
        }

        private async Task FetchFirstWriter()
        {
            foreach (var writerId in _movie.WriterIds.Take(1).Where(writerId => _movie.Writers.All(w => w.UniqueId != writerId)))
            {
                _movie.Writers.Add(await FetchWriter(writerId));
            }
        }

        #endregion

        #region Fetch Remaining
       
        private async Task FetchRemainingCharacters()
        {
            var firstId = _movie.Characters.First().UniqueId;
            foreach (int characterId in _movie.CharacterIds.Where(id => id != firstId))
            {
                Character character = await FetchCharacter(characterId);
                if (_movie.Characters.Any(c => c.UniqueId == character.UniqueId)) continue;
                _movie.Characters.Add(character);
            }
        }

        private async Task FetchRemainingConcepts()
        {
            var firstId = _movie.Concepts.First().UniqueId;
            foreach (int conceptId in _movie.ConceptIds.Where(id => id != firstId))
            {
                Concept concept = await FetchConcept(conceptId);
                if (_movie.Concepts.Any(c => c.UniqueId == concept.UniqueId)) continue; 
                _movie.Concepts.Add(concept);
            }
        }

        private async Task FetchRemainingLocations()
        {
            var firstId = _movie.Locations.First().UniqueId;
            foreach (int locationId in _movie.LocationIds.Where(id => id != firstId))
            {
                Location location = await FetchLocation(locationId);
                if (_movie.Locations.Any(l => l.UniqueId == location.UniqueId)) continue;
                _movie.Locations.Add(location);
            }
        }

        private async Task FetchRemainingTeams()
        {
            var firstId = _movie.Teams.First().UniqueId;
            foreach (int teamId in _movie.TeamIds.Where(id => id != firstId))
            {
                Team team = await FetchTeam(teamId);
                if (_movie.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                _movie.Teams.Add(team);
            }
        }

        private async Task FetchRemainingWriters()
        {
            var firstId = _movie.Writers.First().UniqueId;
            foreach (int teamId in _movie.WriterIds.Where(id => id != firstId))
            {
                Creator writer = await FetchWriter(teamId);
                if (_movie.Writers.Any(w => w.UniqueId == writer.UniqueId)) continue;
                _movie.Writers.Add(writer);
            }
        }
        
        #endregion 

        #endregion

        #region Mapping Methods

        private Character GetMappedCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
        }

        private Concept GetMappedConcept(string quickConcept)
        {
            return quickConcept == ServiceConstants.QueryNotFound ? new Concept { Name = "Concept Not Found" } : new ConceptMapper().QuickMapXmlObject(quickConcept);
        }

        private Location GetMappedLocation(string quickLocation)
        {
            return quickLocation == ServiceConstants.QueryNotFound ? new Location { Name = "Location Not Found" } : new LocationMapper().QuickMapXmlObject(quickLocation);
        }

        private Team GetMappedTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private Creator GetMappedWriter(string quickWriter)
        {
            return quickWriter == ServiceConstants.QueryNotFound ? new Creator { Name = "Writer Not Found" } : new CreatorMapper().QuickMapXmlObject(quickWriter);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _movieDescription = await ComicVineSource.FormatDescriptionAsync(_movie.DescriptionString);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _movieDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section, i));
                i++;
            }
        }

        #region Event Handlers

        private void CharacterView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.UniqueId);
        }

        private void ConceptView_ConceptClick(object sender, ItemClickEventArgs e)
        {
            var concept = ((Concept)e.ClickedItem);
            Frame.Navigate(typeof(ConceptPage), concept.UniqueId);
        }

        private void LocationView_LocationClick(object sender, ItemClickEventArgs e)
        {
            var location = ((Location)e.ClickedItem);
            Frame.Navigate(typeof(LocationPage), location.UniqueId);
        }

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team.UniqueId);
        }

        private void WriterView_WriterClick(object sender, ItemClickEventArgs e)
        {
            var writer = ((Creator)e.ClickedItem);
            Frame.Navigate(typeof(CreatorPage), writer.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "Characters" || header != "Concepts" || header != "Locations" || header != "Teams") await this.FormatDescriptionForPage();
            switch (header)
            {
                case "Characters":
                    Frame.Navigate(typeof(CharactersPage), _movie);
                    break;
                case "Concepts":
                    //Frame.Navigate(typeof(ConceptsPage), _movie);
                    break;
                case "Locations":
                    //Frame.Navigate(typeof(LocationsPage), _movie);
                    break;
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _movie);
                    break;
                default:
                    Section section = _movieDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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

        #endregion

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
    }
}
