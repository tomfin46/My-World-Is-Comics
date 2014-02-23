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
    public sealed partial class LocationPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _locationPageViewModel = new ObservableDictionary();

        private Location _location;

        private Description _locationDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary LocationPageViewModel
        {
            get { return _locationPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public LocationPage()
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

            Location location = e.NavigationParameter as Location;

            int id;
            if (location != null)
            {
                id = location.Id;
                _location = location;
                PageTitle.Text = _location.Name;
                LocationPageViewModel["Location"] = _location;
                ImageHubSection.Visibility = Visibility.Collapsed;
                BioHubSection.Visibility = Visibility.Visible;
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
                if (SavedData.Location != null && SavedData.Location.Id == id) { _location = SavedData.Location; }
                else { _location = await GetLocation(id); }
            }
            catch (HttpRequestException)
            {
                _location = new Location { Name = "An internet connection is required here" };
                LocationPageViewModel["Location"] = _location;
            }

            PageTitle.Text = _location.Name;

            LocationPageViewModel["Location"] = _location;
            ImageHubSection.Visibility = Visibility.Collapsed;
            BioHubSection.Visibility = Visibility.Visible;
            await LoadLocation();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Location = _location;
        }

        #region Load Location

        private async Task LoadLocation()
        {
            try
            {
                await LoadDescription();

                if (_location.First_Appeared_In_Issue.Volume == null) { await FetchFirstAppearance(); }
                HideOrShowSections();

                if (_location.Volume_Credits.Count > 0) { await FetchFirstVolume(); }
                HideOrShowSections();
                if (_location.Volume_Credits.Count > 1) await FetchRemainingVolumes();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _locationDescription.UniqueId = _location.Id;
            CreateDataTemplates();
        }

        private void HideOrShowSections()
        {
            VolumeSection.Visibility = _location.Volume_Credits.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _location.First_Appeared_In_Issue.Volume != null ? Visibility.Visible : Visibility.Collapsed;
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
            if (_location.First_Appeared_In_Issue.Id != 0)
            {
                _location.First_Appeared_In_Issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_location.First_Appeared_In_Issue.Id));
                _location.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_location.First_Appeared_In_Issue);
            }
        }

        private async Task FetchFirstVolume()
        {
            foreach (var v in _location.Volume_Credits.Take(1))
            {
                Volume volume = MapVolume(await ComicVineSource.GetQuickVolumeAsync(v.Id));
                _location.Volume_Credits[0] = volume;
            }
        }

        private async Task FetchRemainingVolumes()
        {
            for (int i = 1; i < _location.Volume_Credits.Count; ++i)
            {
                Volume volume = MapVolume(await ComicVineSource.GetQuickVolumeAsync(_location.Volume_Credits[i].Id));
                _location.Volume_Credits[i] = volume;
            }
        }

        #endregion

        #region Mapping Methods

        private Location MapLocation(string locationString)
        {
            return locationString == ServiceConstants.QueryNotFound
                ? new Location { Name = "Location Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseLocation>(locationString).Results;
        }

        private Volume MapVolume(string volumeString)
        {
            return volumeString == ServiceConstants.QueryNotFound
                ? new Volume { Name = "Volume Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseVolume>(volumeString).Results;
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
            _locationDescription = await ComicVineSource.FormatDescriptionAsync(_location.Description);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _locationDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
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
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Listners

        private void VolumeView_VolumeClick(object sender, ItemClickEventArgs e)
        {
            var volume = ((Volume)e.ClickedItem);
            Frame.Navigate(typeof(VolumePage), volume);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            var header = e.Section.Header.ToString();
            if (header != "Volumes" || header != "First Appearance") await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Volumes":
                    Frame.Navigate(typeof(VolumesPage), _location);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _location.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _location.First_Appeared_In_Issue);
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
            Frame.Navigate(typeof(VolumePage), _location.First_Appeared_In_Issue.Volume);
        }

        #endregion
    }
}
