using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MyWorldIsComics.Pages.ResourcePages
{
    using Helpers;

    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class IssuePage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _issuePageViewModel = new ObservableDictionary();

        public static Issue BasicIssue;

        private Issue _issue;
        private Issue _nextIssue = new Issue();
        private Issue _previousIssue = new Issue();

        readonly List<string> _filters = new List<string>
                {
                    "Person_Credits",
                    "Character_Credits",
                    "Team_Credits",
                    "Location_Credits",
                    "Concept_Credits",
                    "Object_Credits",
                    "Story_Arc_Credits"
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

            int id;
            if (issue != null)
            {
                id = issue.Id;
                _issue = issue;
            }
            else if (BasicIssue != null)
            {
                id = BasicIssue.Id;
                _issue = BasicIssue;
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
                _issue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(id));
                _issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_issue);

                IssuePageViewModel["Issue"] = _issue;
                
                if (_issue.Volume != null)
                {
                    Volume currentVol = JsonDeserialize.DeserializeJsonString<JsonSingularBaseVolume>(
                    await ComicVineSource.GetFilteredVolumeAsync(_issue.Volume.Id, new List<string> { "first_issue", "issues", "last_issue" })).Results;
                    await FetchBasicNextIssueResource(currentVol);
                    await FetchBasicPreviousIssueResource(currentVol);
                }
                else
                {
                    _issue.Name = "Issue Not Found";
                    _nextIssue.Name = "Issue Not Found";
                    _previousIssue.Name = "Issue Not Found";
                }

                InitialiseFlipView();

                if (_issue.Name != "Issue Not Found" && _issue.Name != ServiceConstants.QueryNotFound) { await LoadIssue(); }
                if (_nextIssue.Name != "Issue Not Found" && _nextIssue.Name != ServiceConstants.QueryNotFound) { await LoadNextIssue(); }
                if (_previousIssue.Name != "Issue Not Found" && _previousIssue.Name != ServiceConstants.QueryNotFound) { await LoadPreviousIssue(); }
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
            BasicIssue = _issue;
        }

        private void InitialiseFlipView()
        {
            FlipView issuesFlipView = new FlipView();
            if (issuesFlipView.Items != null)
            {
                if (_previousIssue.Name != ServiceConstants.QueryNotFound) issuesFlipView.Items.Add(_previousIssue);
                issuesFlipView.Items.Add(_issue);
                if (_nextIssue.Name != ServiceConstants.QueryNotFound) issuesFlipView.Items.Add(_nextIssue);
            }

            issuesFlipView.ItemTemplate = Resources["IssueTemplate"] as DataTemplate;
            issuesFlipView.SelectedItem = _issue;
            issuesFlipView.SelectionChanged += IssueImagesFlipView_SelectionChanged;

            ContentRegion.Children.Insert(0, issuesFlipView);
        }

        #region Load Issue

        private async Task LoadIssue()
        {
            if (_issue.Name != ServiceConstants.QueryNotFound)
            {
                await FetchFilteredIssueResource(_issue);

                if (_issue.Person_Credits.Count > 0) await FetchPeople(_issue);
                if (_issue.Character_Credits.Count > 0) await FetchCharacters(_issue);
                if (_issue.Team_Credits.Count > 0) await FetchTeams(_issue);
                if (_issue.Location_Credits.Count > 0) await FetchLocations(_issue);
                if (_issue.Concept_Credits.Count > 0) await FetchConcepts(_issue);
                if (_issue.Object_Credits.Count > 0) await FetchObjects(_issue);
                if (_issue.Story_Arc_Credits.Count > 0) await FetchStoryArcs(_issue);
            }
        }

        private async Task LoadNextIssue()
        {
            if (_nextIssue.Name != ServiceConstants.QueryNotFound)
            {
                await FetchFilteredIssueResource(_nextIssue);

                if (_nextIssue.Person_Credits.Count > 0) await FetchPeople(_nextIssue);
                if (_nextIssue.Character_Credits.Count > 0) await FetchCharacters(_nextIssue);
                if (_nextIssue.Team_Credits.Count > 0) await FetchTeams(_nextIssue);
                if (_nextIssue.Location_Credits.Count > 0) await FetchLocations(_nextIssue);
                if (_nextIssue.Concept_Credits.Count > 0) await FetchConcepts(_nextIssue);
                if (_nextIssue.Object_Credits.Count > 0) await FetchObjects(_nextIssue);
                if (_nextIssue.Story_Arc_Credits.Count > 0) await FetchStoryArcs(_nextIssue);
            }
        }

        private async Task LoadPreviousIssue()
        {
            if (_previousIssue.Name != ServiceConstants.QueryNotFound)
            {
                await FetchFilteredIssueResource(_previousIssue);

                if (_previousIssue.Person_Credits.Count > 0) await FetchPeople(_previousIssue);
                if (_previousIssue.Character_Credits.Count > 0) await FetchCharacters(_previousIssue);
                if (_previousIssue.Team_Credits.Count > 0) await FetchTeams(_previousIssue);
                if (_previousIssue.Location_Credits.Count > 0) await FetchLocations(_previousIssue);
                if (_previousIssue.Concept_Credits.Count > 0) await FetchConcepts(_previousIssue);
                if (_previousIssue.Object_Credits.Count > 0) await FetchObjects(_previousIssue);
                if (_previousIssue.Story_Arc_Credits.Count > 0) await FetchStoryArcs(_previousIssue);
            }
        }

        #endregion

        #region Fetch Resources

        private async Task FetchBasicNextIssueResource(Volume currentVol)
        {
            if (_issue.Issue_Number == currentVol.First_Issue.Issue_Number)
            {
                if (currentVol.Issues.Count > 1)
                {
                    await MapNextIssue(currentVol.Issues[1]);
                }
                else
                {
                    _nextIssue = new Issue
                    {
                        Name = ServiceConstants.QueryNotFound
                    };
                }
            }
            else if (_issue.Issue_Number == currentVol.Last_Issue.Issue_Number)
            {
                _nextIssue = new Issue
                {
                    Name = ServiceConstants.QueryNotFound
                };
            }
            else
            {
                for (int i = 0; i < currentVol.Issues.Count; ++i)
                {
                    if (currentVol.Issues[i].Issue_Number == _issue.Issue_Number)
                    {
                        await MapNextIssue(currentVol.Issues[i + 1]);
                    }
                }
            }
        }

        private async Task MapNextIssue(Issue issue)
        {
            _nextIssue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(issue.Id));
            _nextIssue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_nextIssue);
        }

        private async Task FetchBasicPreviousIssueResource(Volume currentVol)
        {
            if (_issue.Issue_Number == currentVol.First_Issue.Issue_Number)
            {
                _previousIssue = new Issue
                {
                    Name = ServiceConstants.QueryNotFound
                };
            }
            else if (_issue.Issue_Number == currentVol.Last_Issue.Issue_Number)
            {
                if (currentVol.Last_Issue.Id != currentVol.First_Issue.Id)
                {
                    await MapPrevIssue(currentVol.Issues[currentVol.Issues.Count - 1]);
                }
                else
                {
                    _previousIssue = new Issue
                    {
                        Name = ServiceConstants.QueryNotFound
                    };
                }
            }
            else
            {
                for (int i = 0; i < currentVol.Issues.Count; ++i)
                {
                    if (currentVol.Issues[i].Issue_Number == _issue.Issue_Number)
                    {
                        await MapPrevIssue(currentVol.Issues[i - 1]);
                    }
                }
            }
        }

        private async Task MapPrevIssue(Issue issue)
        {
            _previousIssue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(issue.Id));
            _previousIssue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_previousIssue);
        }

        private async Task FetchFilteredIssueResource(Issue issue)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(issue.Id, _filters);
            var filterObj = JsonDeserialize.DeserializeJsonString<JsonSingularBaseIssue>(filteredIssueString);

            foreach (var filter in _filters)
            {
                var issueType = issue.GetType();
                var issueProp = issueType.GetRuntimeProperty(filter);

                switch (filter)
                {
                    case "Person_Credits":
                        var personResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<Person>;
                        if (personResults == null) continue;

                        foreach (var result in personResults)
                        {
                            Person person = result;
                            if (issue.Person_Credits.Any(p => p.Id == person.Id)) continue;
                            issue.Person_Credits.Add(person);
                        }
                        break;
                    case "Character_Credits":
                        var characterResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<Character>;
                        if (characterResults == null) continue;

                        foreach (var result in characterResults)
                        {
                            Character character = result;
                            if (issue.Character_Credits.Any(c => c.Id == character.Id)) continue;
                            issue.Character_Credits.Add(character);
                        }
                        break;
                    case "Team_Credits":
                        var teamResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<Team>;
                        if (teamResults == null) continue;

                        foreach (var result in teamResults)
                        {
                            Team team = result;
                            if (issue.Team_Credits.Any(t => t.Id == team.Id)) continue;
                            issue.Team_Credits.Add(team);
                        }
                        break;
                    case "Location_Credits":
                        var locationResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<Location>;
                        if (locationResults == null) continue;

                        foreach (var result in locationResults)
                        {
                            Location location = result;
                            if (issue.Location_Credits.Any(l => l.Id == location.Id)) continue;
                            issue.Location_Credits.Add(location);
                        }
                        break;
                    case "Concept_Credits":
                        var conceptResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<Concept>;
                        if (conceptResults == null) continue;

                        foreach (var result in conceptResults)
                        {
                            Concept concept = result;
                            if (issue.Concept_Credits.Any(c => c.Id == concept.Id)) continue;
                            issue.Concept_Credits.Add(concept);
                        }
                        break;
                    case "Object_Credits":
                        var objectResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<ObjectResource>;
                        if (objectResults == null) continue;

                        foreach (var result in objectResults)
                        {
                            ObjectResource objectRes = result;
                            if (issue.Object_Credits.Any(o => o.Id == objectRes.Id)) continue;
                            issue.Object_Credits.Add(objectRes);
                        }
                        break;
                    case "Story_Arc_Credits":
                        var storyArcResults = issueProp.GetValue(filterObj.Results) as ObservableCollection<StoryArc>;
                        if (storyArcResults == null) continue;

                        foreach (var result in storyArcResults)
                        {
                            StoryArc storyArc = result;
                            if (issue.Story_Arc_Credits.Any(s => s.Id == storyArc.Id)) continue;
                            issue.Story_Arc_Credits.Add(storyArc);
                        }
                        break;
                }
            }
        }

        #endregion

        #region General Multiple Fetch Methods

        private async Task FetchPeople(Issue issue)
        {
            for (int i = 0; i < issue.Person_Credits.Count; ++i)
            {
                Person creator = issue.Person_Credits[i];
                if (issue.Person_Credits[i].Image == null)
                {
                    creator = await FetchPerson(issue.Person_Credits[i].Id);
                    creator.Role = issue.Person_Credits[i].Role;
                }

                issue.Person_Credits.RemoveAt(i);
                issue.Person_Credits.Insert(i, creator);
            }
        }

        private async Task FetchCharacters(Issue issue)
        {
            for (int i = 0; i < issue.Character_Credits.Count; ++i)
            {
                Character character = issue.Character_Credits[i];
                if (issue.Character_Credits[i].Image == null)
                {
                    character = await FetchCharacter(issue.Character_Credits[i].Id);
                }
                issue.Character_Credits.RemoveAt(i);
                issue.Character_Credits.Insert(i, character);
            }
        }

        private async Task FetchTeams(Issue issue)
        {
            for (int i = 0; i < issue.Team_Credits.Count; ++i)
            {
                Team team = issue.Team_Credits[i];
                if (issue.Team_Credits[i].Image == null)
                {
                    team = await FetchTeam(issue.Team_Credits[i].Id);
                }
                issue.Team_Credits.RemoveAt(i);
                issue.Team_Credits.Insert(i, team);
            }
        }

        private async Task FetchLocations(Issue issue)
        {
            for (int i = 0; i < issue.Location_Credits.Count; ++i)
            {
                Location location = issue.Location_Credits[i];
                if (issue.Location_Credits[i].Image == null)
                {
                    location = await FetchLocation(issue.Location_Credits[i].Id);
                }
                issue.Location_Credits.RemoveAt(i);
                issue.Location_Credits.Insert(i, location);
            }
        }

        private async Task FetchConcepts(Issue issue)
        {
            for (int i = 0; i < issue.Concept_Credits.Count; ++i)
            {
                Concept concept = issue.Concept_Credits[i];
                if (issue.Concept_Credits[i].Image == null)
                {
                    concept = await FetchConcept(issue.Concept_Credits[i].Id);
                }
                issue.Concept_Credits.RemoveAt(i);
                issue.Concept_Credits.Insert(i, concept);
            }
        }

        private async Task FetchObjects(Issue issue)
        {
            for (int i = 0; i < issue.Object_Credits.Count; ++i)
            {
                ObjectResource objectResource = issue.Object_Credits[i];
                if (issue.Object_Credits[i].Image == null)
                {
                    objectResource = await FetchObject(issue.Object_Credits[i].Id);
                }
                issue.Object_Credits.RemoveAt(i);
                issue.Object_Credits.Insert(i, objectResource);
            }
        }

        private async Task FetchStoryArcs(Issue issue)
        {
            for (int i = 0; i < issue.Story_Arc_Credits.Count; ++i)
            {
                StoryArc storyArc = issue.Story_Arc_Credits[i];
                if (issue.Story_Arc_Credits[i].Image == null)
                {
                    storyArc = await FetchStoryArc(issue.Story_Arc_Credits[i].Id);
                }
                issue.Story_Arc_Credits.RemoveAt(i);
                issue.Story_Arc_Credits.Insert(i, storyArc);
            }
        }

        #endregion

        #region General Singular Fetch Methods

        private async Task<Person> FetchPerson(int personId)
        {
            return GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(personId));
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

        private Issue GetMappedIssue(string issueString)
        {
            return issueString == ServiceConstants.QueryNotFound
                ? new Issue { Name = "Issue Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseIssue>(issueString).Results;
        }

        private Person GetMappedCreator(string creatorString)
        {
            return creatorString == ServiceConstants.QueryNotFound
                ? new Person { Name = "Creator Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBasePerson>(creatorString).Results;
        }

        private Character GetMappedCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound
                ? new Character { Name = "Character Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
        }

        private Team GetMappedTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound
                ? new Team { Name = "Team Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseTeam>(teamString).Results;
        }

        private Location GetMappedLocation(string locationString)
        {
            return locationString == ServiceConstants.QueryNotFound
                ? new Location { Name = "Location Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseLocation>(locationString).Results;
        }

        private Concept GetMappedConcept(string conceptString)
        {
            return conceptString == ServiceConstants.QueryNotFound
                ? new Concept { Name = "Concept Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseConcept>(conceptString).Results;
        }

        private ObjectResource GetMappedObject(string objectString)
        {
            return objectString == ServiceConstants.QueryNotFound
                ? new ObjectResource { Name = "Object Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseObjectResource>(objectString).Results;
        }

        private StoryArc GetMappedStoryArc(string storyArcString)
        {
            return storyArcString == ServiceConstants.QueryNotFound
                ? new StoryArc { Name = "Story Arc Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseStoryArc>(storyArcString).Results;
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
            var creator = ((Person)e.ClickedItem);
            Frame.Navigate(typeof(CreatorPage), creator);
        }

        private void CharactersView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character);
        }

        private void TeamsView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team);
        }

        private void LocationsView_LocationClick(object sender, ItemClickEventArgs e)
        {
            var location = ((Location)e.ClickedItem);
            Frame.Navigate(typeof(LocationPage), location);
        }

        private void ConceptsView_ConceptClick(object sender, ItemClickEventArgs e)
        {
            var concept = ((Concept)e.ClickedItem);
            Frame.Navigate(typeof(ConceptPage), concept);
        }

        private void ObjectsView_ObjectClick(object sender, ItemClickEventArgs e)
        {
            var objectResource = ((ObjectResource)e.ClickedItem);
            Frame.Navigate(typeof(ObjectPage), objectResource);
        }

        private void StoryArcsView_StoryArcClick(object sender, ItemClickEventArgs e)
        {
            var storyArc = ((StoryArc)e.ClickedItem);
            Frame.Navigate(typeof(StoryArcPage), storyArc);
        }

        private async void IssueImagesFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView flipView = sender as FlipView;
            if (flipView == null) return;


            Issue selectedIssue = flipView.SelectedItem as Issue;
            if (selectedIssue == null) return;
            GridTitles.DataContext = selectedIssue;

            ComicVineSource.CancelTask();
            ComicVineSource.ReinstateCts();

            try
            {
                Volume currentVol = JsonDeserialize.DeserializeJsonString<JsonSingularBaseVolume>(
                    await ComicVineSource.GetFilteredVolumeAsync(_issue.Volume.Id, new List<string> { "first_issue", "issues", "last_issue" })).Results;

                var diff = selectedIssue.IssueNumberInteger - _issue.IssueNumberInteger;
                if (diff >= 1)
                {
                    _previousIssue = _issue;
                    _issue = _nextIssue;
                    await FetchBasicNextIssueResource(currentVol);
                    if (flipView.Items != null)
                    {
                        var contains = false;
                        foreach (int issuePos in flipView.Items.Cast<Issue>().Where(issue => issue.Id == _nextIssue.Id)
                            .Select(issue => flipView.Items.IndexOf(issue)))
                        {
                            flipView.Items.RemoveAt(issuePos);
                            flipView.Items.Insert(issuePos, _nextIssue);
                            contains = true;
                        }

                        if (!contains && _nextIssue.Name != ServiceConstants.QueryNotFound)
                        {
                            flipView.Items.Add(_nextIssue);
                        }

                        flipView.SelectedItem = _issue;
                    }
                    await LoadIssue();
                    await LoadNextIssue();
                    await LoadPreviousIssue();
                }
                else if (diff <= 0)
                {
                    _nextIssue = _issue;
                    _issue = _previousIssue;
                    await FetchBasicPreviousIssueResource(currentVol);
                    if (flipView.Items != null)
                    {
                        var contains = false;
                        foreach (int issuePos in flipView.Items.Cast<Issue>().Where(issue => issue.Id == _previousIssue.Id).Select(issue => flipView.Items.IndexOf(issue)))
                        {
                            flipView.Items.RemoveAt(issuePos);
                            flipView.Items.Insert(issuePos, _previousIssue);
                            contains = true;
                        }

                        if (!contains && _previousIssue.Name != ServiceConstants.QueryNotFound)
                        {
                            flipView.Items.Insert(flipView.SelectedIndex, _previousIssue);
                        }

                        flipView.SelectedItem = _issue;
                    }
                    await LoadIssue();
                    await LoadNextIssue();
                    await LoadPreviousIssue();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
            catch (InvalidOperationException)
            {

            }
        }

        private void VolumeName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VolumePage), _issue.Volume);
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
