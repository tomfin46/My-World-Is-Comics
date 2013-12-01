using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MyWorldIsComics.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;

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
            this.basicIssueForPage = e.NavigationParameter as Issue;
            this.IssuePageViewModel["Issue"] = this.basicIssueForPage;

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem

            await this.LoadIssue();
        }

        private async Task LoadIssue()
        {
            try
            {
                List<string> filters = new List<string> { "person_credits", "character_credits", "team_credits", "location_credits",
                    "concept_credits", "object_credits", "story_arc_credits" };

                foreach (string filter in filters)
                {
                    await this.FetchFilteredIssueResource(filter);
                    switch (filter)
                    {
                        case "person_credits":
                            await this.FetchFirstPerson();
                            break;
                        case "character_credits":
                            await this.FetchCharacters();
                            break;
                        case "team_credits":
                            await this.FetchFirstTeam();
                            break;
                        case "location_credits":
                            //await this.FetchLocations();
                            break;
                        case "concept_credits":
                            //await this.FetchConcepts();
                            break;
                        case "object_credits":
                            //await this.FetchObjects();
                            break;
                        case "story_arc_credits":
                            //await this.FetchStoryArcs();
                            break;
                    }
                    this.IssuePageViewModel["FilteredCharacter"] = this.filteredIssueForPage;
                }
                await this.FetchRemainingPeople();
                await this.FetchRemainingCharacters();
                await this.FetchRemainingTeams();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        #region Fetch Methods
        private async Task FetchFilteredIssueResource(string filter)
        {
            string filteredIssueString = await ComicVineSource.GetFilteredIssueAsync(this.basicIssueForPage.UniqueId, filter);
            this.filteredIssueForPage = this.GetMappedIssueFromFilter(filteredIssueString, filter);
        }
        private async Task FetchFirstPerson()
        {
            foreach (var person in this.filteredIssueForPage.PersonIds.Take(1))
            {
                Creator creator = this.GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(person.Key.ToString()));
                creator.Role = person.Value;
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == creator.UniqueId)) continue;
                this.filteredIssueForPage.Creators.Add(creator);
            }
        }

        private async Task FetchRemainingPeople()
        {
            var firstId = this.filteredIssueForPage.PersonIds.First();
            foreach (var person in this.filteredIssueForPage.PersonIds.Where(p => p.Key != firstId.Key).Take(this.filteredIssueForPage.PersonIds.Count - 1))
            {
                Creator creator = this.GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(person.Key.ToString()));
                creator.Role = person.Value;
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == creator.UniqueId)) continue;
                this.filteredIssueForPage.Creators.Add(creator);
            }
        }

        private async Task FetchCharacters()
        {
            foreach (var characterId in this.filteredIssueForPage.CharacterIds.Take(1))
            {
                Character character = this.GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId.ToString()));
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == character.UniqueId)) continue;
                this.filteredIssueForPage.Characters.Add(character);
            }
        }

        private async Task FetchRemainingCharacters()
        {
            var firstId = this.filteredIssueForPage.CharacterIds.First();
            foreach (var characterId in this.filteredIssueForPage.CharacterIds.Where(c => c != firstId).Take(this.filteredIssueForPage.CharacterIds.Count - 1))
            {
                Character character = this.GetMappedCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId.ToString()));
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == character.UniqueId)) continue;
                this.filteredIssueForPage.Characters.Add(character);
            }
        }

        private async Task FetchFirstTeam()
        {
            foreach (int teamId in this.filteredIssueForPage.TeamIds.Take(1))
            {
                Team team = this.GetMappedTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                if (this.filteredIssueForPage.TeamIds.Any(t => t == team.UniqueId)) continue;
                this.filteredIssueForPage.Teams.Add(team);
            }
        }

        private async Task FetchRemainingTeams()
        {
            var firstId = this.filteredIssueForPage.TeamIds.First();
            foreach (int teamId in this.filteredIssueForPage.TeamIds.Where(t => t != firstId).Take(this.filteredIssueForPage.TeamIds.Count - 1))
            {
                Team team = this.GetMappedTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                if (this.filteredIssueForPage.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                this.filteredIssueForPage.Teams.Add(team);
            }
        }

        private Task FetchLocations()
        {
            throw new NotImplementedException();
        }

        private Task FetchConcepts()
        {
            throw new NotImplementedException();
        }

        private Task FetchObjects()
        {
            throw new NotImplementedException();
        }

        private Task FetchStoryArcs()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Get Mappins Methods

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
            throw new NotImplementedException();
        }

        private void StoryArcsView_StoryArcClick(object sender, ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
