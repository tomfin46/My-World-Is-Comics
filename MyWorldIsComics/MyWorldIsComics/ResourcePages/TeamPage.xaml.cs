using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.Mappers;

namespace MyWorldIsComics.ResourcePages
{
    #region usings

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    using Common;
    using DataSource;

    #endregion

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class TeamPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary teamPageViewModel = new ObservableDictionary();

        private Team _team;

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
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            _team = e.NavigationParameter as Team;
            TeamPageViewModel["Team"] = _team;
            await LoadTeam();
        }
        private async Task LoadTeam()
        {
            try
            {
                List<string> filters = new List<string> { "first_appeared_in_issue" };

                foreach (string filter in filters)
                {
                    await FetchFilteredTeamResource(filter);
                    switch (filter)
                    {
                        case "first_appeared_in_issue":
                            await FetchFirstAppearance();
                            break;
                    }
                    TeamPageViewModel["Team"] = _team;
                    HideOrShowFilteredSections();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

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

        private Team MapTeam(string teamString)
        {
            return teamString == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().MapXmlObject(teamString);
        }

        private void HideOrShowFilteredSections()
        {
            FirstAppearanceSection.Visibility = _team.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
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
                    Frame.Navigate(typeof(IssuePage), _team.FirstAppearanceIssue);
                    break;
            }
        }
    }
}
