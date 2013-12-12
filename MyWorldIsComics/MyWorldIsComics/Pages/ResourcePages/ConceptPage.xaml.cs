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

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

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
    public sealed partial class ConceptPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary conceptPageViewModel = new ObservableDictionary();

        private Concept _concept;

        private Description _conceptDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary ConceptPageViewModel
        {
            get { return this.conceptPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ConceptPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
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
                await LoadConcept(id);
            }
            catch (HttpRequestException)
            {
                _concept = new Concept { Name = "An internet connection is required here" };
                ConceptPageViewModel["Concept"] = _concept;
            }
        }

        #region Load Character

        private async Task LoadConcept(int id)
        {
            try
            {
                if (SavedData.Concept != null && SavedData.Concept.UniqueId == id) { _concept = SavedData.Concept; }
                else { _concept = await this.GetConcept(id); }
                PageTitle.Text = _concept.Name;


                ConceptPageViewModel["Concept"] = _concept;

                if (_concept.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_concept.FirstAppearanceIssue == null)
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

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _conceptDescription.UniqueId = _concept.UniqueId;
            CreateDataTemplates();
        }

        private async Task FormatDescriptionForPage()
        {
            _conceptDescription = await ComicVineSource.FormatDescriptionAsync(_concept.DescriptionString);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _conceptDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section, i));
                i++;
            }
        }

        private async Task<Concept> GetConcept(int id)
        {
            var conceptString = await ComicVineSource.GetConceptAsync(id);
            return this.MapConcept(conceptString);
        }

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _concept.FirstAppearanceIssue != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods
        
        private async Task FetchFirstAppearance()
        {
            if (_concept.FirstAppearanceId != 0)
            {
                _concept.FirstAppearanceIssue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_concept.FirstAppearanceId));
            }
        }

        #endregion

        #region Mapping Methods

        private Concept MapConcept(string conceptString)
        {
            return conceptString == ServiceConstants.QueryNotFound ? new Concept { Name = ServiceConstants.QueryNotFound } : new ConceptMapper().MapXmlObject(conceptString);
        }
        
        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
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

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Teams" || e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _concept);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _concept.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _concept.FirstAppearanceIssue);
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
            Frame.Navigate(typeof(VolumePage), _concept.FirstAppearanceIssue.VolumeId);
        }
    }
}
