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

    using Windows.UI.Xaml.Markup;

    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Helpers;
    using MyWorldIsComics.Mappers;
    using MyWorldIsComics.Pages.CollectionPages;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class LocationPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary locationPageViewModel = new ObservableDictionary();

        private Location _location;

        private Description _locationDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary LocationPageViewModel
        {
            get { return this.locationPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public LocationPage()
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
                await LoadLocation(id);
            }
            catch (HttpRequestException)
            {
                _location = new Location { Name = "An internet connection is required here" };
                LocationPageViewModel["Location"] = _location;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Location = _location;
        }

        #region Load Location

        private async Task LoadLocation(int id)
        {
            try
            {
                if (SavedData.Location != null && SavedData.Location.UniqueId == id) { _location = SavedData.Location; }
                else { _location = await GetLocation(id); }
                PageTitle.Text = _location.Name;

                LocationPageViewModel["Location"] = _location;
                ImageHubSection.Visibility = Visibility.Collapsed;
                BioHubSection.Visibility = Visibility.Visible;

                await LoadDescription();

                if (_location.FirstAppearanceIssue == null) { await FetchFirstAppearance(); }
                HideOrShowSections();

                if (_location.Volumes.Count == 0) { await FetchFirstVolume(); }
                HideOrShowSections();
                if (_location.VolumeIds.Count > 1) await FetchRemainingVolumes();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _locationDescription.UniqueId = _location.UniqueId;
            CreateDataTemplates();
        }

        private void HideOrShowSections()
        {
            VolumeSection.Visibility = _location.VolumeIds.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _location.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task<Location> GetLocation(int id)
        {
            var locationSearchString = await ComicVineSource.GetLocationAsync(id);
            return MapLocation(locationSearchString);
        } 

        #endregion

        #region Fetch methods

        private async Task FetchFirstAppearance()
        {
            if (_location.FirstAppearanceId != 0)
            {
                _location.FirstAppearanceIssue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_location.FirstAppearanceId));
                _location.FirstAppearanceIssue.Description = await ComicVineSource.FormatDescriptionAsync(_location.FirstAppearanceIssue);
            }
        }

        private async Task FetchFirstVolume()
        {
            foreach (int volumeId in _location.VolumeIds.Take(1))
            {
                Volume volume = MapQuickVolume(await ComicVineSource.GetQuickVolumeAsync(volumeId));
                if (_location.Volumes.Any(t => t.UniqueId == volume.UniqueId)) continue;
                _location.Volumes.Add(volume);
            }
        }

        private async Task FetchRemainingVolumes()
        {
            var firstId = _location.VolumeIds.First();
            foreach (int volumeId in _location.VolumeIds.Where(id => id != firstId).Take(_location.VolumeIds.Count - 1))
            {
                Volume volume = MapQuickVolume(await ComicVineSource.GetQuickVolumeAsync(volumeId));
                if (_location.Volumes.Any(t => t.UniqueId == volume.UniqueId)) continue;
                _location.Volumes.Add(volume);
            }
        }

        #endregion

        #region Mapping Methods

        private Location MapLocation(string locationString)
        {
            return locationString == ServiceConstants.QueryNotFound ? new Location { Name = ServiceConstants.QueryNotFound } : new LocationMapper().MapXmlObject(locationString);
        }

        private Volume MapQuickVolume(string quickVolume)
        {
            return quickVolume == ServiceConstants.QueryNotFound ? new Volume { Name = "Volume Not Found" } : new VolumeMapper().QuickMapXmlObject(quickVolume);
        }

        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _locationDescription = await ComicVineSource.FormatDescriptionAsync(_location.DescriptionString);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _locationDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                i++;
            }
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

        #region Event Listners

        private void VolumeView_VolumeClick(object sender, ItemClickEventArgs e)
        {
            var volume = ((Volume)e.ClickedItem);
            Frame.Navigate(typeof(VolumePage), volume.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "Volumes" || header != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Volumes":
                    Frame.Navigate(typeof(VolumesPage), _location);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _location.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _location.FirstAppearanceIssue);
                    break;
                default:
                    Section section = _locationDescription.Sections.First(d => d.Title == header);
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
            Frame.Navigate(typeof(VolumePage), _location.FirstAppearanceIssue.VolumeId);
        }

        #endregion
    }
}
