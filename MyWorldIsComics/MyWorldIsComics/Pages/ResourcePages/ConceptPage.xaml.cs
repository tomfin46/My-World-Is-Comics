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
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{


    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class ConceptPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _conceptPageViewModel = new ObservableDictionary();

        private Concept _concept;
        private Description _conceptDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary ConceptPageViewModel
        {
            get { return _conceptPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public ConceptPage()
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

            Concept concept = e.NavigationParameter as Concept;

            int id;
            if (concept != null)
            {
                id = concept.Id;
                _concept = concept;
                PageTitle.Text = _concept.Name;
                ConceptPageViewModel["Concept"] = _concept;
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
                if (SavedData.Concept != null && SavedData.Concept.Id == id) { _concept = SavedData.Concept; }
                else { _concept = await GetConcept(id); }
            }
            catch (HttpRequestException)
            {
                _concept = new Concept { Name = "An internet connection is required here" };
                ConceptPageViewModel["Concept"] = _concept;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }

            PageTitle.Text = _concept.Name;
            ConceptPageViewModel["Concept"] = _concept;
            await LoadConcept();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Concept = _concept;
        }

        #region Load Concept
        
        private async Task<Concept> GetConcept(int id)
        {
            var conceptString = await ComicVineSource.GetConceptAsync(id);
            return MapConcept(conceptString);
        }

        private async Task LoadConcept()
        {
            try
            {
                if (_concept.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_concept.First_Appeared_In_Issue.Volume == null)
                    {
                        await FetchFirstAppearance();
                    }
                    HideOrShowSections();
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        #endregion

        #region Description Mapping

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _conceptDescription.UniqueId = _concept.Id;
            CreateDataTemplates();
        }

        private async Task FormatDescriptionForPage()
        {
            _conceptDescription = await ComicVineSource.FormatDescriptionAsync(_concept.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _conceptDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }
      
        #endregion

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _concept.First_Appeared_In_Issue.Volume != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Fetch methods

        private async Task FetchFirstAppearance()
        {
            if (_concept.First_Appeared_In_Issue.Id != 0)
            {
                _concept.First_Appeared_In_Issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_concept.First_Appeared_In_Issue.Id));
                _concept.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_concept.First_Appeared_In_Issue);
            }
        }

        #endregion

        #region Mapping Methods

        private Concept MapConcept(string conceptString)
        {
            return conceptString == ServiceConstants.QueryNotFound
                ? new Concept { Name = "Concept Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseConcept>(conceptString).Results;
        }

        private Issue MapIssue(string issueString)
        {
            return issueString == ServiceConstants.QueryNotFound
                ? new Issue { Name = "Issue Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseIssue>(issueString).Results;
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

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Teams" || e.Section.Header.ToString() != "First Appearance") await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _concept);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _concept.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _concept.First_Appeared_In_Issue);
                    break;
                default:
                    Section section = _conceptDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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
            Frame.Navigate(typeof(VolumePage), _concept.First_Appeared_In_Issue.Volume.Id);
        }
    }
}
