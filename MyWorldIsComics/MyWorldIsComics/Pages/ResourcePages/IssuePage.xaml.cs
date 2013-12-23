using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;

namespace MyWorldIsComics.Pages.ResourcePages
{
    using MyWorldIsComics.Helpers;

    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class IssuePage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _issuePageViewModel = new ObservableDictionary();

        public static Issue BasicIssue;
        private static Issue FilteredIssue;

        private Issue _basicIssueForPage;
        private Issue _filteredIssueForPage;
        private Issue _nextIssue = new Issue();
        private Issue _previousIssue = new Issue();

        List<string> filters = new List<string>
                {
                    "person_credits",
                    "character_credits",
                    "team_credits",
                    "location_credits",
                    "concept_credits",
                    "object_credits",
                    "story_arc_credits"
                };

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary IssuePageViewModel
        {
            get { return _issuePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public IssuePage()
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
        /// <see cref="Frame.Navigate(Type, ObjectResource)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }

            Issue issue = e.NavigationParameter as Issue;

            if (issue == null)
            {
                int id;
                try
                {
                    id = int.Parse(e.NavigationParameter as string);
                }
                catch (ArgumentNullException)
                {
                    id = (int)e.NavigationParameter;
                }

                BasicIssue = this.GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(id));
            }

            if (BasicIssue != null)
            {
                _basicIssueForPage = BasicIssue;
                _filteredIssueForPage = _basicIssueForPage;
                IssuePageViewModel["Issue"] = _basicIssueForPage;
            }
            else
            {
                _basicIssueForPage = issue;
                _filteredIssueForPage = _basicIssueForPage;
                IssuePageViewModel["Issue"] = _basicIssueForPage;
            }

            try
            {
                await FetchBasicNextIssueResource();
                await FetchBasicPreviousIssueResource();

                InitialiseFlipView();

                await LoadIssue();
                await LoadNextIssue();
                await LoadPreviousIssue();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.Message == "Collection was modified; enumeration operation may not execute.")
                {

                }
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }
            // Save response content so don't have to fetch from api service again
            BasicIssue = _filteredIssueForPage;
            FilteredIssue = _filteredIssueForPage;
        }

        private void InitialiseFlipView()
        {
            FlipView issuesFlipView = new FlipView();
            if (issuesFlipView.Items != null)
            {
                if (_previousIssue.Name != ServiceConstants.QueryNotFound) issuesFlipView.Items.Add(_previousIssue);
                issuesFlipView.Items.Add(_basicIssueForPage);
                if (_nextIssue.Name != ServiceConstants.QueryNotFound) issuesFlipView.Items.Add(_nextIssue);
            }

            issuesFlipView.ItemTemplate = Resources["IssueTemplate"] as DataTemplate;
            issuesFlipView.SelectedItem = _basicIssueForPage;
            issuesFlipView.SelectionChanged += IssueImagesFlipView_SelectionChanged;

            ContentRegion.Children.Insert(0, issuesFlipView);
        }

        #region Load Issues

        private async Task LoadIssue()
        {
            await FetchFilteredIssueResource(filters);

            if (_filteredIssueForPage.Creators.Count != _filteredIssueForPage.PersonIds.Count) await FetchPeople(_filteredIssueForPage);
            if (_filteredIssueForPage.Characters.Count != _filteredIssueForPage.CharacterIds.Count) await FetchCharacters(_filteredIssueForPage);
            if (_filteredIssueForPage.Teams.Count != _filteredIssueForPage.TeamIds.Count) await FetchTeams(_filteredIssueForPage);
            if (_filteredIssueForPage.Locations.Count != _filteredIssueForPage.LocationIds.Count) await FetchLocations(_filteredIssueForPage);
            if (_filteredIssueForPage.Concepts.Count != _filteredIssueForPage.ConceptIds.Count) await FetchConcepts(_filteredIssueForPage);
            if (_filteredIssueForPage.Objects.Count != _filteredIssueForPage.ObjectIds.Count) await FetchObjects(_filteredIssueForPage);
            if (_filteredIssueForPage.StoryArcs.Count != _filteredIssueForPage.StoryArcIds.Count) await FetchStoryArcs(_filteredIssueForPage);
        }

        private async Task LoadNextIssue()
        {
            await FetchNextIssueResource(filters);

            if (_nextIssue.Creators.Count != _nextIssue.PersonIds.Count) await FetchPeople(_nextIssue);
            if (_nextIssue.Characters.Count != _nextIssue.CharacterIds.Count) await FetchCharacters(_nextIssue);
            if (_nextIssue.Teams.Count != _nextIssue.TeamIds.Count) await FetchTeams(_nextIssue);
            if (_nextIssue.Locations.Count != _nextIssue.LocationIds.Count) await FetchLocations(_nextIssue);
            if (_nextIssue.Concepts.Count != _nextIssue.ConceptIds.Count) await FetchConcepts(_nextIssue);
            if (_nextIssue.Objects.Count != _nextIssue.ObjectIds.Count) await FetchObjects(_nextIssue);
            if (_nextIssue.StoryArcs.Count != _nextIssue.StoryArcIds.Count) await FetchStoryArcs(_nextIssue);
        }

        private async Task LoadPreviousIssue()
        {
            await FetchPreviousIssueResource(filters);

            if (_previousIssue.Creators.Count != _previousIssue.PersonIds.Count) await FetchPeople(_previousIssue);
            if (_previousIssue.Characters.Count != _previousIssue.CharacterIds.Count) await FetchCharacters(_previousIssue);
            if (_previousIssue.Teams.Count != _previousIssue.TeamIds.Count) await FetchTeams(_previousIssue);
            if (_previousIssue.Locations.Count != _previousIssue.LocationIds.Count) await FetchLocations(_previousIssue);
            if (_previousIssue.Concepts.Count != _previousIssue.ConceptIds.Count) await FetchConcepts(_previousIssue);
            if (_previousIssue.Objects.Count != _previousIssue.ObjectIds.Count) await FetchObjects(_previousIssue);
            if (_previousIssue.StoryArcs.Count != _previousIssue.StoryArcIds.Count) await FetchStoryArcs(_previousIssue);
        }

        #endregion

        #region Fetch Resources

        private async Task FetchBasicNextIssueResource()
        {
            _nextIssue = GetMappedIssue(await ComicVineSource.GetSpecificIssueAsync(_basicIssueForPage.VolumeId, _basicIssueForPage.IssueNumber + 1));
        }

        private async Task FetchNextIssueResource(List<string> filters)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(_nextIssue.UniqueId, filters);
            foreach (string filter in filters)
            {
                _nextIssue = GetMappedIssueFromFilter(_nextIssue, filteredIssueString, filter);
            }
        }

        private async Task FetchBasicPreviousIssueResource()
        {
            _previousIssue = GetMappedIssue(await ComicVineSource.GetSpecificIssueAsync(_basicIssueForPage.VolumeId, _basicIssueForPage.IssueNumber - 1));
        }

        private async Task FetchPreviousIssueResource(List<string> filters)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(_previousIssue.UniqueId, filters);
            foreach (string filter in filters)
            {
                _previousIssue = GetMappedIssueFromFilter(_previousIssue, filteredIssueString, filter);
            }
        }

        private async Task FetchFilteredIssueResource(List<string> filters)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(_basicIssueForPage.UniqueId, filters);
            foreach (string filter in filters)
            {
                _filteredIssueForPage = GetMappedIssueFromFilter(_basicIssueForPage, filteredIssueString, filter);
            }
        }

        #endregion

        #region General Multiple Fetch Methods

        private async Task FetchPeople(Issue issue)
        {
            foreach (var person in issue.PersonIds)
            {
                Creator creator = await FetchPerson(person);
                if (issue.Creators.Any(c => c.UniqueId == creator.UniqueId)) continue;
                issue.Creators.Add(creator);
            }
        }

        private async Task FetchCharacters(Issue issue)
        {
            foreach (var characterId in issue.CharacterIds)
            {
                Character character = await FetchCharacter(characterId);
                if (issue.Characters.Any(c => c.UniqueId == character.UniqueId)) continue;
                issue.Characters.Add(character);
            }
        }

        private async Task FetchTeams(Issue issue)
        {
            foreach (int teamId in issue.TeamIds)
            {
                Team team = await FetchTeam(teamId);
                if (issue.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                issue.Teams.Add(team);
            }
        }

        private async Task FetchLocations(Issue issue)
        {
            foreach (int locationId in issue.LocationIds)
            {
                Location location = await FetchLocation(locationId);
                if (issue.Locations.Any(l => l.UniqueId == location.UniqueId)) continue;
                issue.Locations.Add(location);
            }
        }

        private async Task FetchConcepts(Issue issue)
        {
            foreach (int conceptId in issue.ConceptIds)
            {
                Concept concept = await FetchConcept(conceptId);
                if (issue.Concepts.Any(c => c.UniqueId == concept.UniqueId)) continue;
                issue.Concepts.Add(concept);
            }
        }

        private async Task FetchObjects(Issue issue)
        {
            foreach (int objectId in issue.ObjectIds)
            {
                ObjectResource mappedObject = await FetchObject(objectId);
                if (issue.Objects.Any(o => o.UniqueId == mappedObject.UniqueId)) continue;
                issue.Objects.Add(mappedObject);
            }
        }

        private async Task FetchStoryArcs(Issue issue)
        {
            foreach (int storyArcId in issue.StoryArcIds)
            {
                StoryArc storyArc = await FetchStoryArc(storyArcId);
                if (issue.StoryArcs.Any(sA => sA.UniqueId == storyArc.UniqueId)) continue;
                issue.StoryArcs.Add(storyArc);
            }
        }

        #endregion

        #region General Singular Fetch Methods

        private async Task<Creator> FetchPerson(KeyValuePair<int, string> person)
        {
            Creator creator = GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(person.Key));
            creator.Role = person.Value;
            return creator;
        }

        private async Task<Character> FetchCharacter(int characterId)
        {
            return GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
        }

        private async Task<Team> FetchTeam(int teamId)
        {
            return GetMappedTeam(await ComicVineSource.GetQuickTeamAsync(teamId));
        }

        private async Task<Location> FetchLocation(int locationId)
        {
            return GetMappedLocation(await ComicVineSource.GetQuickLocationAsync(locationId));
        }

        private async Task<Concept> FetchConcept(int conceptId)
        {
            return GetMappedConcept(await ComicVineSource.GetQuickConceptAsync(conceptId));
        }

        private async Task<ObjectResource> FetchObject(int objectId)
        {
            return GetMappedObject(await ComicVineSource.GetQuickObjectAsync(objectId));
        }

        private async Task<StoryArc> FetchStoryArc(int storyArcId)
        {
            return GetMappedStoryArc(await ComicVineSource.GetQuickStoryArcAsync(storyArcId));
        }

        #endregion

        #region Get Mapping Methods

        private Issue GetMappedIssueFromFilter(Issue issue, string filteredIssueString, string filter)
        {
            return filteredIssueString == ServiceConstants.QueryNotFound ? new Issue() : new IssueMapper().MapFilteredXmlObject(issue, filteredIssueString, filter);
        }

        private Issue GetMappedIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        private Creator GetMappedCreator(string quickCreator)
        {
            return quickCreator == ServiceConstants.QueryNotFound ? new Creator { Name = "Creator Not Found" } : new CreatorMapper().QuickMapXmlObject(quickCreator);
        }

        private Character GetMappedCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
        }

        private Team GetMappedTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private Location GetMappedLocation(string quickLocation)
        {
            return quickLocation == ServiceConstants.QueryNotFound ? new Location { Name = "Location Not Found" } : new LocationMapper().QuickMapXmlObject(quickLocation);
        }

        private Concept GetMappedConcept(string quickConcept)
        {
            return quickConcept == ServiceConstants.QueryNotFound ? new Concept() { Name = "Concept Not Found" } : new ConceptMapper().QuickMapXmlObject(quickConcept);
        }

        private ObjectResource GetMappedObject(string quickObject)
        {
            return quickObject == ServiceConstants.QueryNotFound ? new ObjectResource { Name = "Object Not Found" } : new ObjectMapper().QuickMapXmlObject(quickObject);
        }

        private StoryArc GetMappedStoryArc(string quickStoryArc)
        {
            return quickStoryArc == ServiceConstants.QueryNotFound ? new StoryArc { Name = "Story Arc Not Found" } : new StoryArcMapper().QuickMapXmlObject(quickStoryArc);
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

        #region Event Handlers

        private void CreatorsView_CreatorClick(object sender, ItemClickEventArgs e)
        {
            var creator = ((Creator)e.ClickedItem);
            Frame.Navigate(typeof(CreatorPage), creator.UniqueId);
        }

        private void CharactersView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.UniqueId);
        }

        private void TeamsView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team.UniqueId);
        }

        private void LocationsView_LocationClick(object sender, ItemClickEventArgs e)
        {
            var location = ((Location)e.ClickedItem);
            Frame.Navigate(typeof(LocationPage), location.UniqueId);
        }

        private void ConceptsView_ConceptClick(object sender, ItemClickEventArgs e)
        {
            var concept = ((Concept)e.ClickedItem);
            Frame.Navigate(typeof(ConceptPage), concept.UniqueId);
        }

        private void ObjectsView_ObjectClick(object sender, ItemClickEventArgs e)
        {
            var objectResource = ((ObjectResource)e.ClickedItem);
            Frame.Navigate(typeof(ObjectPage), objectResource.UniqueId);
        }

        private void StoryArcsView_StoryArcClick(object sender, ItemClickEventArgs e)
        {
            var storyArc = ((StoryArc)e.ClickedItem);
            Frame.Navigate(typeof(StoryArcPage), storyArc.UniqueId);
        }

        private async void IssueImagesFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView flipView = sender as FlipView;
            if (flipView != null)
            {
                Issue selectedIssue = flipView.SelectedItem as Issue;
                if (selectedIssue == null) return;
                GridTitles.DataContext = selectedIssue;
                
                ComicVineSource.CancelTask();
                ComicVineSource.ReinstateCts();

                try
                {
                    switch (selectedIssue.IssueNumber - this._basicIssueForPage.IssueNumber)
                    {
                        case 1:
                            this._previousIssue = this._basicIssueForPage;
                            this._basicIssueForPage = this._nextIssue;
                            await this.FetchBasicNextIssueResource();
                            if (flipView.Items != null)
                            {
                                var contains = false;
                                foreach (int issuePos in flipView.Items.Cast<Issue>().Where(issue => issue.UniqueId == this._nextIssue.UniqueId)
                                    .Select(issue => flipView.Items.IndexOf(issue)))
                                {
                                    flipView.Items.RemoveAt(issuePos);
                                    flipView.Items.Insert(issuePos, this._nextIssue);
                                    contains = true;
                                }

                                if (!contains && this._nextIssue.Name != ServiceConstants.QueryNotFound)
                                {
                                    flipView.Items.Add(this._nextIssue);
                                }

                                flipView.SelectedItem = this._basicIssueForPage;
                            }
                            await this.LoadIssue();
                            await this.LoadNextIssue();
                            await this.LoadPreviousIssue();
                            break;

                        case -1:
                            this._nextIssue = this._basicIssueForPage;
                            this._basicIssueForPage = this._previousIssue;
                            await this.FetchBasicPreviousIssueResource();
                            if (flipView.Items != null)
                            {
                                var contains = false;
                                foreach (
                                    int issuePos in
                                        flipView.Items.Cast<Issue>()
                                            .Where(issue => issue.UniqueId == this._previousIssue.UniqueId)
                                            .Select(issue => flipView.Items.IndexOf(issue)))
                                {
                                    flipView.Items.RemoveAt(issuePos);
                                    flipView.Items.Insert(issuePos, this._previousIssue);
                                    contains = true;
                                }

                                if (!contains && this._previousIssue.Name != ServiceConstants.QueryNotFound)
                                {
                                    flipView.Items.Insert(flipView.SelectedIndex, this._previousIssue);
                                }

                                flipView.SelectedItem = this._basicIssueForPage;
                            }
                            await this.LoadIssue();
                            await this.LoadNextIssue();
                            await this.LoadPreviousIssue();
                            break;
                    }
                }
                catch (TaskCanceledException)
                {
                    ComicVineSource.ReinstateCts();
                }
                catch (InvalidOperationException ioe)
                {

                }
            }
        }

        private void VolumeName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof (VolumePage), _basicIssueForPage.VolumeId);
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
