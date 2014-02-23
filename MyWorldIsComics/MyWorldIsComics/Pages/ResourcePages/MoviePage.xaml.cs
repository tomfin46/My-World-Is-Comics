using System.Reflection;
using MyWorldIsComics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Threading.Tasks;
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
    public sealed partial class MoviePage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _moviePageViewModel = new ObservableDictionary();

        private Movie _movie;
        private Description _movieDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary MoviePageViewModel
        {
            get { return _moviePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public MoviePage()
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

            Movie movie = e.NavigationParameter as Movie;

            int id;
            if (movie != null)
            {
                id = movie.Id;
                _movie = movie;
                PageTitle.Text = _movie.Name;
                MoviePageViewModel["Movie"] = _movie;
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
                if (SavedData.Movie != null && SavedData.Movie.Id == id) { _movie = SavedData.Movie; }
                else { _movie = await GetMovie(id); }
            }
            catch (HttpRequestException)
            {
                _movie = new Movie { Name = "An internet connection is required here" };
                MoviePageViewModel["Movie"] = _movie;
            }

            PageTitle.Text = _movie.Name;
            MoviePageViewModel["Movie"] = _movie;
            await LoadMovie();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Movie = _movie;
        }

        #region Load Movie

        private async Task LoadMovie()
        {
            try
            {
                if (_movie.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    await LoadFilters();

                    if (_movie.Characters.Count > 1) await FetchRemainingCharacters();
                    HideOrShowSections();
                    if (_movie.Concepts.Count > 1) await FetchRemainingConcepts();
                    HideOrShowSections();
                    if (_movie.Locations.Count > 1) await FetchRemainingLocations();
                    HideOrShowSections();
                    if (_movie.Teams.Count > 1) await FetchRemainingTeams();
                    HideOrShowSections();
                    if (_movie.Writers.Count > 1) await FetchRemainingWriters();
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
            _movieDescription.UniqueId = _movie.Id;
            CreateDataTemplates();
        }

        private async Task<Movie> GetMovie(int id)
        {
            var movieSearchString = await ComicVineSource.GetMovieAsync(id);
            return MapMovie(movieSearchString);
        }

        private async Task LoadFilters()
        {
            List<string> filters = new List<string> { "Characters", "Concepts", "Locations", "Teams", "Writers" };

            foreach (string filter in filters)
            {
                var filteredMovieString = await ComicVineSource.GetFilteredMovieAsync(_movie.Id, filter.ToLowerInvariant());
                var filterObj = JsonDeserialize.DeserializeJsonString<JsonSingularBaseMovie>(filteredMovieString);

                var movieType = _movie.GetType();
                var prop = movieType.GetRuntimeProperty(filter);
                prop.SetValue(_movie, prop.GetValue(filterObj.Results));

                if (_movie.Characters.Count > 0) await FetchFirstCharacter();
                if (_movie.Concepts.Count > 0) await FetchFirstConcept();
                if (_movie.Locations.Count > 0) await FetchFirstLocation();
                if (_movie.Teams.Count > 0) await FetchFirstTeam();
                if (_movie.Writers.Count > 0) await FetchFirstWriter();
                HideOrShowSections();
            }
        }

        private Movie MapMovie(string movieString)
        {
            return movieString == ServiceConstants.QueryNotFound
                ? new Movie { Name = "Movie Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseMovie>(movieString).Results;
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

        private async Task<Person> FetchWriter(int writerId)
        {
            return GetMappedWriter(await ComicVineSource.GetQuickCreatorAsync(writerId));
        }

        #region Fetch First

        private async Task FetchFirstCharacter()
        {
            Character character = await FetchCharacter(_movie.Characters[0].Id);
            _movie.Characters[0] = character;
        }

        private async Task FetchFirstConcept()
        {
            Concept concept = await FetchConcept(_movie.Concepts[0].Id);
            _movie.Concepts[0] = concept;
        }

        private async Task FetchFirstLocation()
        {
            Location location = await FetchLocation(_movie.Locations[0].Id);
            _movie.Locations[0] = location;
        }

        private async Task FetchFirstTeam()
        {
            Team team = await FetchTeam(_movie.Teams[0].Id);
            _movie.Teams[0] = team;
        }

        private async Task FetchFirstWriter()
        {
            Person writer = await FetchWriter(_movie.Writers[0].Id);
            _movie.Writers[0] = writer;
        }

        #endregion

        #region Fetch Remaining

        private async Task FetchRemainingCharacters()
        {
            for (int i = 1; i < _movie.Characters.Count; ++i)
            {
                Character character = await FetchCharacter(_movie.Characters[i].Id);
                _movie.Characters[i] = character;
            }
        }

        private async Task FetchRemainingConcepts()
        {
            for (int i = 1; i < _movie.Concepts.Count; ++i)
            {
                Concept concept = await FetchConcept(_movie.Concepts[i].Id);
                _movie.Concepts[i] = concept;
            }
        }

        private async Task FetchRemainingLocations()
        {
            for (int i = 1; i < _movie.Locations.Count; ++i)
            {
                Location location = await FetchLocation(_movie.Locations[i].Id);
                _movie.Locations[i] = location;
            }
        }

        private async Task FetchRemainingTeams()
        {
            for (int i = 1; i < _movie.Teams.Count; ++i)
            {
                Team team = await FetchTeam(_movie.Teams[i].Id);
                _movie.Teams[i] = team;
            }
        }

        private async Task FetchRemainingWriters()
        {
            for (int i = 1; i < _movie.Writers.Count; ++i)
            {
                Person writer = await FetchWriter(_movie.Writers[i].Id);
                _movie.Writers[i] = writer;
            }
        }

        #endregion

        #endregion

        #region Mapping Methods

        private Character GetMappedCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound
                ? new Character { Name = "Character Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
        }

        private Concept GetMappedConcept(string conceptString)
        {
            return conceptString == ServiceConstants.QueryNotFound
                ? new Concept { Name = "Concept Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseConcept>(conceptString).Results;
        }

        private Location GetMappedLocation(string locationString)
        {
            return locationString == ServiceConstants.QueryNotFound
                 ? new Location { Name = "Location Not Found" }
                 : JsonDeserialize.DeserializeJsonString<JsonSingularBaseLocation>(locationString).Results;
        }

        private Team GetMappedTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound
                      ? new Team { Name = "Team Not Found" }
                      : JsonDeserialize.DeserializeJsonString<JsonSingularBaseTeam>(teamString).Results;
        }

        private Person GetMappedWriter(string writerString)
        {
            return writerString == ServiceConstants.QueryNotFound
                      ? new Person { Name = "Writer Not Found" }
                      : JsonDeserialize.DeserializeJsonString<JsonSingularBasePerson>(writerString).Results;
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _movieDescription = await ComicVineSource.FormatDescriptionAsync(_movie.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _movieDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }

        #region Event Handlers

        private void CharacterView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character);
        }

        private void ConceptView_ConceptClick(object sender, ItemClickEventArgs e)
        {
            var concept = ((Concept)e.ClickedItem);
            Frame.Navigate(typeof(ConceptPage), concept);
        }

        private void LocationView_LocationClick(object sender, ItemClickEventArgs e)
        {
            var location = ((Location)e.ClickedItem);
            Frame.Navigate(typeof(LocationPage), location);
        }

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team);
        }

        private void WriterView_WriterClick(object sender, ItemClickEventArgs e)
        {
            var writer = ((Person)e.ClickedItem);
            Frame.Navigate(typeof(CreatorPage), writer);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "Characters" || header != "Concepts" || header != "Locations" || header != "Teams") await FormatDescriptionForPage();
            switch (header)
            {
                case "Characters":
                    Frame.Navigate(typeof(CharactersPage), _movie);
                    break;
                case "Concepts":
                    //TODO: Frame.Navigate(typeof(ConceptsPage), _movie);
                    break;
                case "Locations":
                    //TODO: Frame.Navigate(typeof(LocationsPage), _movie);
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
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
