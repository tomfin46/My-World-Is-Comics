using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MyWorldIsComics.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231
using MyWorldIsComics.DataModel;
using MyWorldIsComics.DataModel.Interfaces;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics.Pages
{
    using MyWorldIsComics.Helpers;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class SearchResultsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get
            {
                return this.navigationHelper;
            }
        }

        public SearchResultsPage()
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
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (ComicVineSource.IsCanceled()) { ComicVineSource.ReinstateCts(); }

            string query = e.NavigationParameter as string;
            FetchResults(query);
        }

        private async void FetchResults(string query)
        {
            try
            {
                SearchResultsMapper searchResultsMapper = new SearchResultsMapper();
                searchResultsMapper.MapSearchResults(await ComicVineSource.ExecuteSearchAsync(query));
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
                    DefaultViewModel["SearchResults"] = new ObservableCollection<Results> { new Results { Name = "No results", ResultsList = new ObservableCollection<IResource> { new ObjectResource { Name = "Please search again." } } } };
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Item_Clicked(object sender, ItemClickEventArgs e)
        {
            int id = ((IResource) e.ClickedItem).UniqueId;
            if (e.ClickedItem as Character != null)
            {
                Frame.Navigate(typeof (CharacterPage), id);
            }
            else if (e.ClickedItem as Concept != null)
            {
                Frame.Navigate(typeof(ConceptPage), id);
            }
            else if (e.ClickedItem as Creator != null)
            {
                Frame.Navigate(typeof(CreatorPage), id);
            }
            else if (e.ClickedItem as Issue != null)
            {
                IssuePage.BasicIssue = new Issue {UniqueId = id};
                Frame.Navigate(typeof(IssuePage), id);
            }
            else if (e.ClickedItem as Location != null)
            {
                Frame.Navigate(typeof(LocationPage), id);
            }
            else if (e.ClickedItem as Movie != null)
            {
                Frame.Navigate(typeof(MoviePage), id);
            }
            else if (e.ClickedItem as ObjectResource != null)
            {
                //Frame.Navigate(typeof(ObjectPage), id);
            }
            else if (e.ClickedItem as Publisher != null)
            {
                Frame.Navigate(typeof(PublisherPage), id);
            }
            else if (e.ClickedItem as StoryArc != null)
            {
                //Frame.Navigate(typeof(StoryArcPage), id);
            }
            else if (e.ClickedItem as Team != null)
            {
                Frame.Navigate(typeof(TeamPage), id);
            }
            else if (e.ClickedItem as Volume != null)
            {
                Frame.Navigate(typeof(VolumePage), id);
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
    }
}
