using System.Net.Http;
using System.Threading.Tasks;
using MyWorldIsComics.Common;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
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
    public sealed partial class CreatorPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _creatorPageViewModel = new ObservableDictionary();

        private Person _creator;
        private Description _creatorDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary CreatorPageViewModel
        {
            get { return _creatorPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public CreatorPage()
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

            Person creator = e.NavigationParameter as Person;

            int id;
            if (creator != null)
            {
                id = creator.Id;
                _creator = creator;
                PageTitle.Text = _creator.Name;
                CreatorPageViewModel["Creator"] = _creator;
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
                if (SavedData.Creator != null && SavedData.Creator.Id == id) { _creator = SavedData.Creator; }
                else { _creator = await GetCreator(id); }
                }
            catch (HttpRequestException)
            {
                _creator = new Person { Name = "An internet connection is required here" };
                CreatorPageViewModel["Creator"] = _creator;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }

            PageTitle.Text = _creator.Name;
            CreatorPageViewModel["Creator"] = _creator;
            await LoadCreator();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SavedData.Creator = _creator;
        }

        #region Load Creator

        private async Task LoadCreator()
        {
            try
            {
                if (_creator.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_creator.Created_Characters.Count > 0)
                    {
                        await FetchFirstCreatedCharacter();
                    }
                    HideOrShowSections();
                    if (_creator.Created_Characters.Count > 1) await FetchRemainingCreatedCharacters();
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
            _creatorDescription.UniqueId = _creator.Id;
            CreateDataTemplates();
        }

        private async Task<Person> GetCreator(int id)
        {
            var creatorString = await ComicVineSource.GetCreatorAsync(id);
            return MapCreator(creatorString);
        }

        private void HideOrShowSections()
        {
            CreatedCharacterSection.Visibility = _creator.Created_Characters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstCreatedCharacter()
        {
            foreach (var c in _creator.Created_Characters.Take(1))
            {
                Character character = MapCharacter(await ComicVineSource.GetQuickCharacterAsync(c.Id));
                _creator.Created_Characters[0] = character;
            }
        }

        private async Task FetchRemainingCreatedCharacters()
        {
            for (int i = 1; i < _creator.Created_Characters.Count; ++i)
            {
                Character character = MapCharacter(await ComicVineSource.GetQuickCharacterAsync(_creator.Created_Characters[i].Id));
                _creator.Created_Characters[i] = character;
            }
        }

        #endregion

        #region Mapping Methods

        private Person MapCreator(string creatorString)
        {
            return creatorString == ServiceConstants.QueryNotFound
                ? new Person { Name = "Person Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBasePerson>(creatorString).Results;
        }

        private Character MapCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound
                ? new Character { Name = "Character Not Found" }
                : JsonDeserialize.DeserializeJsonString<JsonSingularBaseCharacter>(characterString).Results;
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _creatorDescription = await ComicVineSource.FormatDescriptionAsync(_creator.Description);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _creatorDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                ++i;
            }
        }

        #region Event Handlers

        private void CreatedCharacterView_CharacterClick(object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Created Characters") await FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Created Characters":
                    Frame.Navigate(typeof(CharactersPage), _creator);
                    break;
                default:
                    Section section = _creatorDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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
