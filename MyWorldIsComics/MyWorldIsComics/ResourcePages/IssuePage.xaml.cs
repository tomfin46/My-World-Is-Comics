using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyWorldIsComics.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using Object = MyWorldIsComics.DataModel.Resources.Object;

namespace MyWorldIsComics.ResourcePages
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class IssuePage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _issuePageViewModel = new ObservableDictionary();

        private static Issue BasicIssue;
        private static Issue FilteredIssue;

        private Issue basicIssueForPage;
        private Issue filteredIssueForPage;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary IssuePageViewModel
        {
            get { return this._issuePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }

        public IssuePage()
        {
            this.InitializeComponent();
            this._navigationHelper = new NavigationHelper(this);
            this._navigationHelper.LoadState += navigationHelper_LoadState;
            this._navigationHelper.SaveState += navigationHelper_SaveState;
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
            Issue issue = e.NavigationParameter as Issue;
            if (issue != null && BasicIssue != null && BasicIssue.UniqueId == issue.UniqueId)
            {
                this.basicIssueForPage = BasicIssue;
                this.filteredIssueForPage = FilteredIssue;
                this.IssuePageViewModel["Issue"] = this.basicIssueForPage;
            }
            else
            {
                this.basicIssueForPage = issue;
                this.filteredIssueForPage = this.basicIssueForPage;
                this.IssuePageViewModel["Issue"] = this.basicIssueForPage;
                await this.LoadIssue();
                
            }

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem

        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (this.Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            BasicIssue = this.basicIssueForPage;
            FilteredIssue = this.filteredIssueForPage;
        }

        private async Task LoadIssue()
        {
            try
            {
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
                await this.FetchFilteredIssueResource(filters);

                await this.FetchPeople();
                await this.FetchCharacters();
                await this.FetchTeams();
                await this.FetchLocations();
                await this.FetchConcepts();
                await this.FetchObjects();
                await this.FetchStoryArcs();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task FetchFilteredIssueResource(List<string> filters)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(this.basicIssueForPage.UniqueId, filters);
            foreach (string filter in filters)
            {
                this.filteredIssueForPage = this.GetMappedIssueFromFilter(filteredIssueString, filter);
            }
        }

        #region Fetch Methods

        private async Task FetchPeople()
        {
            foreach (var person in this.filteredIssueForPage.PersonIds)
            {
                Creator creator = this.GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(person.Key.ToString()));
                creator.Role = person.Value;
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == creator.UniqueId)) continue;
                this.filteredIssueForPage.Creators.Add(creator);
            }
        }

        private async Task FetchCharacters()
        {
            foreach (var characterId in this.filteredIssueForPage.CharacterIds)
            {
                Character character = this.GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId.ToString()));
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == character.UniqueId)) continue;
                this.filteredIssueForPage.Characters.Add(character);
            }
        }

        private async Task FetchTeams()
        {
            foreach (int teamId in this.filteredIssueForPage.TeamIds)
            {
                Team team = this.GetMappedTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                if (this.filteredIssueForPage.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                this.filteredIssueForPage.Teams.Add(team);
            }
        }

        private async Task FetchLocations()
        {
            foreach (int locationId in this.filteredIssueForPage.LocationIds)
            {
                Location location = this.GetMappedLocation(await ComicVineSource.GetQuickLocationAsync(locationId.ToString()));
                if (this.filteredIssueForPage.Locations.Any(l => l.UniqueId == location.UniqueId)) continue;
                this.filteredIssueForPage.Locations.Add(location);
            }
        }

        private async Task FetchConcepts()
        {
            foreach (int conceptId in this.filteredIssueForPage.ConceptIds)
            {
                Concept concept = this.GetMappedConcept(await ComicVineSource.GetQuickConceptAsync(conceptId.ToString()));
                if (this.filteredIssueForPage.Concepts.Any(c => c.UniqueId == concept.UniqueId)) continue;
                this.filteredIssueForPage.Concepts.Add(concept);
            }
        }

        private async Task FetchObjects()
        {
            foreach (int objectId in this.filteredIssueForPage.ObjectIds)
            {
                Object mappedObject = this.GetMappedObject(await ComicVineSource.GetQuickObjectAsync(objectId.ToString()));
                if (this.filteredIssueForPage.Objects.Any(o => o.UniqueId == mappedObject.UniqueId)) continue;
                this.filteredIssueForPage.Objects.Add(mappedObject);
            }
        }

        private async Task FetchStoryArcs()
        {
            foreach (int storyArcId in this.filteredIssueForPage.StoryArcIds)
            {
                StoryArc storyArc = this.GetMappedStoryArc(await ComicVineSource.GetQuickStoryArcAsync(storyArcId.ToString()));
                if (this.filteredIssueForPage.StoryArcs.Any(sA => sA.UniqueId == storyArc.UniqueId)) continue;
                this.filteredIssueForPage.StoryArcs.Add(storyArc);
            }
        }

        #endregion

        #region Get Mapping Methods

        private Issue GetMappedIssueFromFilter(string filteredIssueString, string filter)
        {
            return filteredIssueString == ServiceConstants.QueryNotFound ? new Issue() : new IssueMapper().MapFilteredXmlObject(this.basicIssueForPage, filteredIssueString, filter);
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

        private Object GetMappedObject(string quickObject)
        {
            return quickObject == ServiceConstants.QueryNotFound ? new Object { Name = "Object Not Found" } : new ObjectMapper().QuickMapXmlObject(quickObject);
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
            //throw new NotImplementedException();
        }

        private void CharactersView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.Name);
        }

        private void TeamsView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team);
        }

        private void LocationsView_LocationClick(object sender, ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ConceptsView_ConceptClick(object sender, ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ObjectsView_ObjectClick(object sender, ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void StoryArcsView_StoryArcClick(object sender, ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
