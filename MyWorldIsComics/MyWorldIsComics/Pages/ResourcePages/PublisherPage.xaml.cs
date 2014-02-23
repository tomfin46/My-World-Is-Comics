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
    public sealed partial class PublisherPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _publisherPageViewModel = new ObservableDictionary();

        private Publisher _publisher;
        private Description _publisherDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary PublisherPageViewModel
        {
            get { return _publisherPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public PublisherPage()
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

            Publisher publisher = e.NavigationParameter as Publisher;

            int id;
            if (publisher != null)
            {
                id = publisher.Id;
                _publisher = publisher;
                PageTitle.Text = _publisher.Name;
                PublisherPageViewModel["Publisher"] = _publisher;
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
                if (SavedData.Publisher != null && SavedData.Publisher.Id == id) { _publisher = SavedData.Publisher; }
                else { _publisher = await GetPublisher(id); }

            }
            catch (HttpRequestException)
            {
                _publisher = new Publisher { Name = "An internet connection is required here" };
                PublisherPageViewModel["Publisher"] = _publisher;
            }

            PublisherPageViewModel["Publisher"] = _publisher;
            PageTitle.Text = _publisher.Name;
            await LoadPublisher();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Publisher = _publisher;
        }

        #region Load Publisher

        private async Task LoadPublisher()
        {
            try
            {
                if (_publisher.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();
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
            _publisherDescription.UniqueId = _publisher.Id;
            CreateDataTemplates();
        }

        private async Task<Publisher> GetPublisher(int id)
        {
            var publisherSearchString = await ComicVineSource.GetPublisherAsync(id);
            return MapPublisher(publisherSearchString);
        }

        #endregion

        #region Mapping Methods

        private Publisher MapPublisher(string publisherString)
        {
            return publisherString == ServiceConstants.QueryNotFound
                ? new Publisher { Name = "Publisher Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBasePublisher>(publisherString).Results;
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _publisherDescription = await ComicVineSource.FormatDescriptionAsync(_publisher.Description);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _publisherDescription.Sections)
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

        #region Event Listeners

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                default:
                    Frame.Navigate(typeof(DescriptionSectionPage), _publisherDescription.Sections.First(d => d.Title == e.Section.Header.ToString()));
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

        #endregion

    }
}
