using MyWorldIsComics.Common;
using System;
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

namespace MyWorldIsComics.Pages.ResourcePages
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class StoryArcPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _storyArcPageViewModel = new ObservableDictionary();

        private StoryArc _storyArc;
        private Description _storyArcDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary StoryArcPageViewModel
        {
            get { return _storyArcPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public StoryArcPage()
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

            StoryArc storyArc = e.NavigationParameter as StoryArc;

            int id;
            if (storyArc != null)
            {
                id = storyArc.Id;
                _storyArc = storyArc;
                PageTitle.Text = _storyArc.Name;
                StoryArcPageViewModel["StoryArc"] = _storyArc;
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
                if (SavedData.StoryArc != null && SavedData.StoryArc.Id == id) { _storyArc = SavedData.StoryArc; }
                else { _storyArc = await GetStoryArc(id); }
            }
            catch (HttpRequestException)
            {
                _storyArc = new StoryArc { Name = "An internet connection is required here" };
                StoryArcPageViewModel["StoryArc"] = _storyArc;
            }

            PageTitle.Text = _storyArc.Name;
            StoryArcPageViewModel["StoryArc"] = _storyArc;
            await LoadStoryArc();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.StoryArc = _storyArc;
        }

        #region Load Story Arc

        private async Task LoadStoryArc()
        {
            try
            {
                if (_storyArc.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_storyArc.First_Appeared_In_Issue == null) await FetchFirstAppearance();
                    HideOrShowSections();

                    if (_storyArc.Issues.Count > 0) await FetchFirstIssue();
                    HideOrShowSections();
                    if (_storyArc.Issues.Count > 1) await FetchRemainingIssues();
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
            _storyArcDescription.UniqueId = _storyArc.Id;
            CreateDataTemplates();
        }

        private async Task<StoryArc> GetStoryArc(int id)
        {
            var storyArcString = await ComicVineSource.GetStoryArcAsync(id);
            return MapStoryArc(storyArcString);
        }

        private void HideOrShowSections()
        {
            IssueSection.Visibility = _storyArc.Issues.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _storyArc.First_Appeared_In_Issue.Image != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstIssue()
        {
            Issue issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_storyArc.Issues[0].Id));
            _storyArc.Issues[0] = issue;
        }

        private async Task FetchRemainingIssues()
        {
            for (int i = 1; i < _storyArc.Issues.Count; ++i)
            {
                Issue issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_storyArc.Issues[i].Id));
                _storyArc.Issues[i] = issue;
            }
        }

        private async Task FetchFirstAppearance()
        {
            if (_storyArc.First_Appeared_In_Issue.Id != 0)
            {
                _storyArc.First_Appeared_In_Issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_storyArc.First_Appeared_In_Issue.Id));
                _storyArc.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_storyArc.First_Appeared_In_Issue);
                StoryArcPageViewModel["StoryArc"] = _storyArc;
            }
        }

        #endregion

        #region Mapping Methods

        private StoryArc MapStoryArc(string storyArcString)
        {
            return storyArcString == ServiceConstants.QueryNotFound
                     ? new StoryArc { Name = "Story Arc Not Found" }
                     : JsonDeserialize.DeserializeJsonString<JsonSingularBaseStoryArc>(storyArcString).Results;
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
            _storyArcDescription = await ComicVineSource.FormatDescriptionAsync(_storyArc.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _storyArcDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }

        #region Event Listners

        private void IssuesView_IssueClick(object sender, ItemClickEventArgs e)
        {
            var issue = ((Issue)e.ClickedItem);
            Frame.Navigate(typeof(IssuePage), issue);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Issues" || e.Section.Header.ToString() != "First Appearance") await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Issues":
                    //TODO: Frame.Navigate(typeof(IssuesPage), _storyArc);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _storyArc.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _storyArc.First_Appeared_In_Issue);
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
            Frame.Navigate(typeof(VolumePage), _storyArc.First_Appeared_In_Issue.Volume);
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
