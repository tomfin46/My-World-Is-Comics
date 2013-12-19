using System.Net.Http;
using System.Threading.Tasks;
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
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Resources;
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
        private NavigationHelper navigationHelper;
        private ObservableDictionary _creatorPageViewModel = new ObservableDictionary();

        private Creator _creator;
        private Description _creatorDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary CreatorPageViewModel
        {
            get { return this._creatorPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public CreatorPage()
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
                await LoadCreator(id);
            }
            catch (HttpRequestException)
            {
                _creator = new Creator { Name = "An internet connection is required here" };
                CreatorPageViewModel["Creator"] = _creator;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Creator = _creator;
        }

        #region Load Creator

        private async Task LoadCreator(int id)
        {
            try
            {
                if (SavedData.Creator != null && SavedData.Creator.UniqueId == id) { _creator = SavedData.Creator; }
                else { _creator = await GetCreator(id); }
                PageTitle.Text = _creator.Name;


                CreatorPageViewModel["Creator"] = _creator;

                if (_creator.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();
                    
                    if (_creator.CreatedCharacters.Count == 0)
                    {
                        await FetchFirstCreatedCharacter();
                    }
                    HideOrShowSections();
                    if (_creator.CreatedCharacterIds.Count > 1) await FetchRemainingCreatedCharacters();
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
            _creatorDescription.UniqueId = _creator.UniqueId;
            CreateDataTemplates();
        }

        private async Task<Creator> GetCreator(int id)
        {
            var creatorString = await ComicVineSource.GetCreatorAsync(id);
            return MapCreator(creatorString);
        }

        private void HideOrShowSections()
        {
            CreatedCharacterSection.Visibility = _creator.CreatedCharacters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstCreatedCharacter()
        {
            foreach (int characterId in _creator.CreatedCharacterIds.Take(1))
            {
                Character character = MapQuickCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
                if (_creator.CreatedCharacters.Any(t => t.UniqueId == character.UniqueId)) continue;
                _creator.CreatedCharacters.Add(character);
            }
        }

        private async Task FetchRemainingCreatedCharacters()
        {
            var firstId = _creator.CreatedCharacterIds.First();
            foreach (int characterId in _creator.CreatedCharacterIds.Where(id => id != firstId).Take(_creator.CreatedCharacterIds.Count - 1))
            {
                Character character = MapQuickCharacter(await ComicVineSource.GetQuickCharacterAsync(characterId));
                if (_creator.CreatedCharacters.Any(t => t.UniqueId == character.UniqueId)) continue;
                _creator.CreatedCharacters.Add(character);
            }
        }
        
        #endregion

        #region Mapping Methods

        private Creator MapCreator(string creatorString)
        {
            return creatorString == ServiceConstants.QueryNotFound ? new Creator { Name = ServiceConstants.QueryNotFound } : new CreatorMapper().MapXmlObject(creatorString);
        }

        private Character MapQuickCharacter(string quickCharacter)
        {
            return quickCharacter == ServiceConstants.QueryNotFound ? new Character { Name = "Character Not Found" } : new CharacterMapper().QuickMapXmlObject(quickCharacter);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _creatorDescription = await ComicVineSource.FormatDescriptionAsync(_creator.DescriptionString);
        }

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _creatorDescription.Sections)
            {
                Hub.Sections.Insert(i, DescriptionMapper.CreateDataTemplate(section));
                i++;
            }
        }

        #region Event Handlers

        private void CreatedCharacterView_CharacterClick (object sender, ItemClickEventArgs e)
        {
            var character = ((Character)e.ClickedItem);
            Frame.Navigate(typeof(CharacterPage), character.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Created Characters") await this.FormatDescriptionForPage();
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
