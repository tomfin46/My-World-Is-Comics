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
    public sealed partial class StoryArcPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary storyArcPageViewModel = new ObservableDictionary();

        private StoryArc _storyArc;
        private Description _storyArcDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary StoryArcPageViewModel
        {
            get { return this.storyArcPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public StoryArcPage()
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
                await LoadStoryArc(id);
            }
            catch (HttpRequestException)
            {
                _storyArc = new StoryArc { Name = "An internet connection is required here" };
                StoryArcPageViewModel["StoryArc"] = _storyArc;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.StoryArc = _storyArc;
        }

        #region Load Story Arc

        private async Task LoadStoryArc(int id)
        {
            try
            {
                if (SavedData.StoryArc != null && SavedData.StoryArc.UniqueId == id) { _storyArc = SavedData.StoryArc; }
                else { _storyArc = await this.GetStoryArc(id); }
                PageTitle.Text = _storyArc.Name;


                StoryArcPageViewModel["StoryArc"] = _storyArc;

                if (_storyArc.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_storyArc.FirstAppearanceIssue == null)
                    {
                        await FetchFirstAppearance();
                    }
                    HideOrShowSections();

                    if (_storyArc.Issues.Count == 0)
                    {
                        await this.FetchFirstIssue();
                    }
                    HideOrShowSections();
                    if (_storyArc.IssueIds.Count > 1) await this.FetchRemainingIssues();
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
            _storyArcDescription.UniqueId = _storyArc.UniqueId;
            CreateDataTemplates();
        }

        private async Task<StoryArc> GetStoryArc(int id)
        {
            var storyArcString = await ComicVineSource.GetStoryArcAsync(id);
            return this.MapStoryArc(storyArcString);
        }

        private void HideOrShowSections()
        {
            IssueSection.Visibility = _storyArc.IssueIds.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _storyArc.FirstAppearanceIssue != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstIssue()
        {
            foreach (int issueId in _storyArc.IssueIds.Take(1))
            {
                Issue issue = this.MapQuickIssue(await ComicVineSource.GetQuickIssueAsync(issueId));
                if (_storyArc.Issues.Any(i => i.UniqueId == issue.UniqueId)) continue;
                _storyArc.Issues.Add(issue);
            }
        }

        private async Task FetchRemainingIssues()
        {
            var firstId = _storyArc.IssueIds.First();
            foreach (int issueId in _storyArc.IssueIds.Where(id => id != firstId).Take(_storyArc.IssueIds.Count - 1))
            {
                Issue issue = this.MapQuickIssue(await ComicVineSource.GetQuickIssueAsync(issueId));
                if (_storyArc.Issues.Any(i => i.UniqueId == issue.UniqueId)) continue;
                _storyArc.Issues.Add(issue);
            }
        }

        private async Task FetchFirstAppearance()
        {
            if (_storyArc.FirstAppearanceId != 0)
            {
                _storyArc.FirstAppearanceIssue = this.MapQuickIssue(await ComicVineSource.GetQuickIssueAsync(_storyArc.FirstAppearanceId));
                _storyArc.FirstAppearanceIssue.Description = await ComicVineSource.FormatDescriptionAsync(_storyArc.FirstAppearanceIssue);
            }
        }

        #endregion

        #region Mapping Methods

        private StoryArc MapStoryArc(string storyArcString)
        {
            return storyArcString == ServiceConstants.QueryNotFound ? new StoryArc { Name = ServiceConstants.QueryNotFound } : new StoryArcMapper().MapXmlObject(storyArcString);
        }

        private Issue MapQuickIssue(string quickIssue)
        {
            return quickIssue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(quickIssue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _storyArcDescription = await ComicVineSource.FormatDescriptionAsync(_storyArc.DescriptionString);
        }

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _storyArcDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                i++;
            }
        }

        #region Event Listners

        private void IssuesView_IssueClick(object sender, ItemClickEventArgs e)
        {
            var issue = ((Issue)e.ClickedItem);
            Frame.Navigate(typeof(IssuePage), issue.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Issues" || e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Issues":
                    //Frame.Navigate(typeof(IssuesPage), _storyArc);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _storyArc.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _storyArc.FirstAppearanceIssue);
                    break;
                default:
                    Section section = _storyArcDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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
            Frame.Navigate(typeof(VolumePage), _storyArc.FirstAppearanceIssue.VolumeId);
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
