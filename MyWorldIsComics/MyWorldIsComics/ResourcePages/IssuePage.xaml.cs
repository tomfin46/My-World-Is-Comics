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
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.basicIssueForPage = e.NavigationParameter as Issue;
            this.IssuePageViewModel["Issue"] = this.basicIssueForPage;

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem

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
                            await this.FetchPeople();
                            break;
                        case "character_credits":
                            await this.FetchCharacters();
                            break;
                        case "team_credits":
                            await this.FetchTeams();
                            break;
                        case "location_credits":
                            await this.FetchLocations();
                            break;
                        case "concept_credits":
                            await this.FetchConcepts();
                            break;
                        case "object_credits":
                            await this.FetchObjects();
                            break;
                        case "story_arc_credits":
                            await this.FetchStoryArcs();
                            break;
                    }
                    this.IssuePageViewModel["FilteredCharacter"] = this.filteredIssueForPage;
                }
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
        private async Task FetchPeople()
        {
            foreach (var person in this.filteredIssueForPage.PersonIds.Take(1))
            {
                Creator creator = this.GetMappedCreator(await ComicVineSource.GetQuickCreatorAsync(person.Key.ToString()));
                if (this.filteredIssueForPage.Creators.Any(c => c.UniqueId == creator.UniqueId)) continue;
                this.filteredIssueForPage.Creators.Add(creator);
            }
        }

        private Task FetchCharacters()
        {
            throw new NotImplementedException();
        }

        private Task FetchTeams()
        {
            throw new NotImplementedException();
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

        private Issue GetMappedIssueFromFilter(string filteredIssueString, string filter)
        {
            return filteredIssueString == ServiceConstants.QueryNotFound ? new Issue() : new IssueMapper().MapFilteredXmlObject(this.basicIssueForPage, filteredIssueString, filter);
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
    }
}
