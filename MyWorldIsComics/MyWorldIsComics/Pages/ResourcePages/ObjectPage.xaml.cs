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
    public sealed partial class ObjectPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary objectPageViewModel = new ObservableDictionary();

        private ObjectResource _object;
        private Description _objectDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary ObjectPageViewModel
        {
            get { return this.objectPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObjectPage()
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
                await this.LoadObject(id);
            }
            catch (HttpRequestException)
            {
                _object = new ObjectResource { Name = "An internet connection is required here" };
                ObjectPageViewModel["Object"] = _object;
            }
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Object = _object;
        }

        #region Load Object

        private async Task LoadObject(int id)
        {
            try
            {
                if (SavedData.Object != null && SavedData.Object.UniqueId == id) { _object = SavedData.Object; }
                else { _object = await this.GetObject(id); }
                PageTitle.Text = _object.Name;


                ObjectPageViewModel["Object"] = _object;

                if (_object.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_object.FirstAppearanceIssue == null)
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
            _objectDescription.UniqueId = _object.UniqueId;
            CreateDataTemplates();
        }

        private async Task<ObjectResource> GetObject(int id)
        {
            var objectString = await ComicVineSource.GetObjectAsync(id);
            return this.MapObject(objectString);
        }

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _object.FirstAppearanceIssue != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods
        
        private async Task FetchFirstAppearance()
        {
            if (_object.FirstAppearanceId != 0)
            {
                _object.FirstAppearanceIssue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_object.FirstAppearanceId));
                _object.FirstAppearanceIssue.Description = await ComicVineSource.FormatDescriptionAsync(_object.FirstAppearanceIssue);
            }
        }

        #endregion

        #region Mapping Methods

        private ObjectResource MapObject(string objectString)
        {
            return objectString == ServiceConstants.QueryNotFound ? new ObjectResource { Name = ServiceConstants.QueryNotFound } : new ObjectMapper().MapXmlObject(objectString);
        }

       private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _objectDescription = await ComicVineSource.FormatDescriptionAsync(_object.DescriptionString);
        }

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _objectDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                i++;
            }
        }

        #region Event Listners

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "First Appearance":
                    IssuePage.BasicIssue = _object.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _object.FirstAppearanceIssue);
                    break;
                default:
                    Section section = _objectDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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
            Frame.Navigate(typeof(VolumePage), _object.FirstAppearanceIssue.VolumeId);
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
