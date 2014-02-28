using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using MyWorldIsComics.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Interfaces;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics.Pages
{
    using Helpers;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class SearchResultsPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();
        private string _query;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get
            {
                return _navigationHelper;
            }
        }

        public SearchResultsPage()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
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
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }

            _query = e.NavigationParameter as string;
            try
            {
                FetchResults();
            }
            catch (HttpRequestException)
            {
                pageTitle.Text = "An internet connection is required here";
            }
        }

        private async void FetchResults()
        {
            try
            {
                SearchResultsMapper searchResultsMapper = new SearchResultsMapper();
                searchResultsMapper.MapSearchResults(await ComicVineSource.ExecuteSearchAsync(_query));
                pageTitle.Text = "Results";
                Dictionary<string, bool> isEmptyDictionary =
                    searchResultsMapper.Results.ToDictionary(results => results.Name,
                        results => results.ResultsList.Count == 0);

                bool isEmpty = true;
                foreach (
                    KeyValuePair<string, bool> keyValuePair in
                        isEmptyDictionary.Where(keyValuePair => keyValuePair.Value == false))
                {
                    isEmpty = false;
                }

                if (!isEmpty)
                {
                    DefaultViewModel["SearchResults"] = searchResultsMapper.Results;
                }
                else
                {
                    DefaultViewModel["SearchResults"] = new ObservableCollection<Results>
                    {
                        new Results
                        {
                            Name = "No results",
                            ResultsList =
                                new ObservableCollection<IResponse> {new ResponseSchema {Name = "Please search again."}}
                        }
                    };
                }
            }
            catch (HttpRequestException)
            {
                pageTitle.Text = "An internet connection is required here";
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async void FetchResults(string tag)
        {
            try
            {
                pageTitle.Text = "Loading...";
                SearchResultsMapper searchResultsMapper = new SearchResultsMapper();
                DefaultViewModel["SearchResults"] = searchResultsMapper.Results;
                searchResultsMapper.MapSearchResults(await ComicVineSource.ExecuteSearchAsync(_query, tag));
                pageTitle.Text = "Results";
                Dictionary<string, bool> isEmptyDictionary = searchResultsMapper.Results.ToDictionary(results => results.Name, results => results.ResultsList.Count == 0);

                bool isEmpty = true;
                foreach (KeyValuePair<string, bool> keyValuePair in isEmptyDictionary.Where(keyValuePair => keyValuePair.Value == false)) { isEmpty = false; }

                if (!isEmpty)
                {
                    DefaultViewModel["SearchResults"] = searchResultsMapper.Results;
                }
                else
                {
                    DefaultViewModel["SearchResults"] = new ObservableCollection<Results> { new Results { Name = "No results", ResultsList = new ObservableCollection<IResponse> { new ResponseSchema { Name = "Please search again." } } } };
                }
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
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
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Item_Clicked(object sender, ItemClickEventArgs e)
        {
            int id = ((IResponse)e.ClickedItem).Id;
            if (e.ClickedItem as Character != null)
            {
                Frame.Navigate(typeof(CharacterPage), e.ClickedItem as Character);
            }
            else if (e.ClickedItem as Concept != null)
            {
                Frame.Navigate(typeof(ConceptPage), e.ClickedItem as Concept);
            }
            else if (e.ClickedItem as Person != null)
            {
                Frame.Navigate(typeof(CreatorPage), e.ClickedItem as Person);
            }
            else if (e.ClickedItem as Issue != null)
            {
                IssuePage.BasicIssue = e.ClickedItem as Issue;
                Frame.Navigate(typeof(IssuePage), e.ClickedItem as Issue);
            }
            else if (e.ClickedItem as Location != null)
            {
                Frame.Navigate(typeof(LocationPage), e.ClickedItem as Location);
            }
            else if (e.ClickedItem as Movie != null)
            {
                Frame.Navigate(typeof(MoviePage), e.ClickedItem as Movie);
            }
            else if (e.ClickedItem as ObjectResource != null)
            {
                Frame.Navigate(typeof(ObjectPage), e.ClickedItem as ObjectResource);
            }
            else if (e.ClickedItem as Publisher != null)
            {
                Frame.Navigate(typeof(PublisherPage), e.ClickedItem as Publisher);
            }
            else if (e.ClickedItem as StoryArc != null)
            {
                Frame.Navigate(typeof(StoryArcPage), e.ClickedItem as StoryArc);
            }
            else if (e.ClickedItem as Team != null)
            {
                Frame.Navigate(typeof(TeamPage), e.ClickedItem as Team);
            }
            else if (e.ClickedItem as Volume != null)
            {
                Frame.Navigate(typeof(VolumePage), e.ClickedItem as Volume);
            }
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

        private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem selectedItem = sender as MenuFlyoutItem;

            if (selectedItem != null)
            {
                var tag = selectedItem.Tag.ToString();
                FetchResults(tag);
            }
        }
    }
}
