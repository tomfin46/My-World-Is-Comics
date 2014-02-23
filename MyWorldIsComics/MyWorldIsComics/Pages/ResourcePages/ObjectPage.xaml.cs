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
    public sealed partial class ObjectPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _objectPageViewModel = new ObservableDictionary();

        private ObjectResource _object;
        private Description _objectDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary ObjectPageViewModel
        {
            get { return _objectPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public ObjectPage()
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


            ObjectResource obj = e.NavigationParameter as ObjectResource;

            int id;
            if (obj != null)
            {
                id = obj.Id;
                _object = obj;
                PageTitle.Text = _object.Name;
                ObjectPageViewModel["Object"] = _object;
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
                if (SavedData.Object != null && SavedData.Object.Id == id) { _object = SavedData.Object; }
                else { _object = await GetObject(id); }
                
            }
            catch (HttpRequestException)
            {
                _object = new ObjectResource { Name = "An internet connection is required here" };
                ObjectPageViewModel["Object"] = _object;
            }

            PageTitle.Text = _object.Name;
            ObjectPageViewModel["Object"] = _object;
            await LoadObject();
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Object = _object;
        }

        #region Load Object

        private async Task LoadObject()
        {
            try
            {
                if (_object.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_object.First_Appeared_In_Issue.Volume == null)
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
            _objectDescription.UniqueId = _object.Id;
            CreateDataTemplates();
        }

        private async Task<ObjectResource> GetObject(int id)
        {
            var objectString = await ComicVineSource.GetObjectAsync(id);
            return MapObject(objectString);
        }

        private void HideOrShowSections()
        {
            FirstAppearanceSection.Visibility = _object.First_Appeared_In_Issue.Volume != null ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstAppearance()
        {
            if (_object.First_Appeared_In_Issue.Id != 0)
            {
                _object.First_Appeared_In_Issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_object.First_Appeared_In_Issue.Id));
                _object.First_Appeared_In_Issue.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_object.First_Appeared_In_Issue);
            }
        }

        #endregion

        #region Mapping Methods

        private ObjectResource MapObject(string objectString)
        {
            return objectString == ServiceConstants.QueryNotFound
                ? new ObjectResource { Name = "Object Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseObjectResource>(objectString).Results;
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
            _objectDescription = await ComicVineSource.FormatDescriptionAsync(_object.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 2;
            foreach (Section section in _objectDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }

        #region Event Listners

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "First Appearance") await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "First Appearance":
                    IssuePage.BasicIssue = _object.First_Appeared_In_Issue;
                    Frame.Navigate(typeof(IssuePage), _object.First_Appeared_In_Issue);
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
            Frame.Navigate(typeof(VolumePage), _object.First_Appeared_In_Issue.Volume);
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
