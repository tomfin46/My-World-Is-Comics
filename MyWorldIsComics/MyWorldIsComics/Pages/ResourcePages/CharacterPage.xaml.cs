using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;

namespace MyWorldIsComics.Pages.ResourcePages
{
    using Windows.ApplicationModel.Activation;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private ObservableDictionary _characterPageViewModel = new ObservableDictionary();

        private Character _character;
        private Description _characterDescription;

        public ObservableDictionary CharacterPageViewModel
        {
            get { return _characterPageViewModel; }
            set { _characterPageViewModel = value; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public CharacterPage()
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
                await LoadCharacter(id);
            }
            catch (HttpRequestException)
            {
                _character = new Character { Name = "An internet connection is required here" };
                CharacterPageViewModel["Character"] = _character;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Character = _character;
        }

        #region Load Character

        private async Task LoadCharacter(int id)
        {
            try
            {
                if (SavedData.Character != null && SavedData.Character.UniqueId == id) { _character = SavedData.Character; }
                else { _character = await GetCharacter(id); }
                PageTitle.Text = _character.Name;


                CharacterPageViewModel["Character"] = _character;

                if (_character.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();

                    if (_character.FirstAppearanceIssue == null)
                    {
                        await FetchFirstAppearance();
                    }
                    HideOrShowSections();

                    if (_character.Teams.Count == 0)
                    {
                        await FetchFirstTeam();
                    }
                    HideOrShowSections();
                    if (_character.TeamIds.Count > 1) await FetchRemainingTeams();
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
            _characterDescription.UniqueId = _character.UniqueId;
            CreateDataTemplates();
        }

        private async Task<Character> GetCharacter(int id)
        {
            var characterSearchString = await ComicVineSource.GetCharacterAsync(id);
            return MapCharacter(characterSearchString);
        }

        private void HideOrShowSections()
        {
            TeamSection.Visibility = _character.TeamIds.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _character.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Fetch methods

        private async Task FetchFirstTeam()
        {
            foreach (int teamId in _character.TeamIds.Take(1))
            {
                Team team = MapQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId));
                if (_character.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                _character.Teams.Add(team);
            }
        }

        private async Task FetchRemainingTeams()
        {
            var firstId = _character.TeamIds.First();
            foreach (int teamId in _character.TeamIds.Where(id => id != firstId).Take(_character.TeamIds.Count - 1))
            {
                Team team = MapQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId));
                if (_character.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                _character.Teams.Add(team);
            }
        }

        private async Task FetchFirstAppearance()
        {
            if (_character.FirstAppearanceId != 0)
            {
                _character.FirstAppearanceIssue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_character.FirstAppearanceId));
                //this.FormatIssueDescriptionForPage();
            }
        }

        #endregion

        #region Mapping Methods

        private Character MapCharacter(string characterString)
        {
            return characterString == ServiceConstants.QueryNotFound ? new Character { Name = ServiceConstants.QueryNotFound } : new CharacterMapper().MapCharacterXmlObject(characterString);
        }

        private Team MapQuickTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _characterDescription = await ComicVineSource.FormatDescriptionAsync(_character.DescriptionString);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _characterDescription.Sections)
            {
                this.CreateDataTemplate(section, i);
                i++;
            }
        }

        private void CreateDataTemplate(Section descriptionSection, int i)
        {
            String markup = String.Empty;
            markup += "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:MyWorldIsComics\">";
            markup += "<ScrollViewer VerticalScrollBarVisibility=\"Hidden\">";
            markup += "<RichTextBlock>";

            if (descriptionSection == null) return;

            while (descriptionSection.ContentQueue.Count > 0)
            {
                var queuePeekType = descriptionSection.ContentQueue.Peek().GetType();
                switch (queuePeekType.Name)
                {
                    case "DescriptionParagraph":
                        DescriptionParagraph para = descriptionSection.ContentQueue.Dequeue() as DescriptionParagraph;
                        if (para != null)
                            markup +=
                                "<Paragraph FontSize=\"15\" FontFamily=\"Segoe UI Semilight\" Margin=\"0,0,0,10\">" + para.FormatLinks();
                        if (descriptionSection.ContentQueue.Count > 0 && descriptionSection.ContentQueue.Peek().GetType() == typeof(Figure))
                        {
                            Figure paraFig = descriptionSection.ContentQueue.Dequeue() as Figure;
                            if (paraFig != null) markup += "</Paragraph><Paragraph></Paragraph><Paragraph TextAlignment=\"Center\">"
                                                           + "<InlineUIContainer><Image Source=\"" + paraFig.ImageSource + "\" Stretch=\"Uniform\"/></InlineUIContainer>" +
                                                           "</Paragraph>" +
                                                           "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + paraFig.Text + "</Paragraph>";
                        }
                        else
                        {
                            markup += "</Paragraph>";
                        }
                        break;
                    case "Figure":
                        Figure fig = descriptionSection.ContentQueue.Dequeue() as Figure;
                        if (fig != null) markup += "<Paragraph></Paragraph><Paragraph TextAlignment=\"Center\">" +
                                                   "<InlineUIContainer><Image Source=\"" + fig.ImageSource + "\" Stretch=\"Uniform\"/>" + "</InlineUIContainer>" +
                                                   "</Paragraph>" +
                                                   "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + fig.Text + "</Paragraph>";
                        break;
                    case "List":
                        List list = descriptionSection.ContentQueue.Dequeue() as List;
                        if (list != null)
                        {
                            while (list.ContentQueue.Count > 0)
                            {
                                DescriptionParagraph listItem = list.ContentQueue.Dequeue() as DescriptionParagraph;
                                if (listItem != null) markup += "<Paragraph Margin=\"25,0,0,16\" TextIndent=\"-25\">> " + listItem.FormatLinks() + "</Paragraph>";
                            }
                        }
                        break;
                    case "Quote":
                        Quote quote = descriptionSection.ContentQueue.Dequeue() as Quote;
                        if (quote != null) markup += "<Paragraph Margin=\"10\"><Bold>" + quote.FormatLinks() + "</Bold></Paragraph>";
                        break;
                    case "Section":
                        Section section = descriptionSection.ContentQueue.Dequeue() as Section;
                        if (section != null) markup += MarkupSection(section);
                        break;
                }
            }

            markup += "</RichTextBlock>";
            markup += "</ScrollViewer>";
            markup += "</DataTemplate>";

            DataTemplate dataTemplate = (DataTemplate)XamlReader.Load(markup);
            HubSection hubSection = new HubSection
                                    {
                                        ContentTemplate = dataTemplate,
                                        Width = 520,
                                        IsHeaderInteractive = true,
                                        Header = descriptionSection.Title
                                    };
            Hub.Sections.Insert(i, hubSection);
        }

        private static string MarkupSection(Section sectionToMarkup)
        {
            string markup = String.Empty;

            if (sectionToMarkup == null) return markup;

            markup += "<Paragraph Margin=\"0,0,0,10\" FontSize=\"17\">";
            if (sectionToMarkup.Type == "h3") markup += "<Bold>" + sectionToMarkup.Title + "</Bold>";
            else if (sectionToMarkup.Type == "h4") markup += "<Underline>" + sectionToMarkup.Title + "</Underline>";
            markup += "</Paragraph>";

            while (sectionToMarkup.ContentQueue.Count > 0)
            {
                var queuePeekType = sectionToMarkup.ContentQueue.Peek().GetType();
                switch (queuePeekType.Name)
                {
                    case "DescriptionParagraph":
                        DescriptionParagraph para = sectionToMarkup.ContentQueue.Dequeue() as DescriptionParagraph;
                        if (para != null)
                            markup += "<Paragraph FontSize=\"15\" FontFamily=\"Segoe UI Semilight\" Margin=\"0,0,0,10\">" + para.FormatLinks();
                        if (sectionToMarkup.ContentQueue.Count > 0 && sectionToMarkup.ContentQueue.Peek().GetType() == typeof(Figure))
                        {
                            Figure paraFig = sectionToMarkup.ContentQueue.Dequeue() as Figure;
                            if (paraFig != null) markup += "</Paragraph><Paragraph></Paragraph><Paragraph TextAlignment=\"Center\">"
                                                           + "<InlineUIContainer><Image Source=\"" + paraFig.ImageSource + "\" Stretch=\"Uniform\"/></InlineUIContainer>" +
                                                           "</Paragraph>" +
                                                           "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + paraFig.Text + "</Paragraph>";
                        }
                        else
                        {
                            markup += "</Paragraph>";
                        }
                        break;
                    case "Figure":
                        Figure fig = sectionToMarkup.ContentQueue.Dequeue() as Figure;
                        if (fig != null) markup += "<Paragraph></Paragraph><Paragraph TextAlignment=\"Center\">" +
                                                   "<InlineUIContainer><Image Source=\"" + fig.ImageSource + "\" Stretch=\"Uniform\"/>" + "</InlineUIContainer>" +
                                                   "</Paragraph>" +
                                                   "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + fig.Text + "</Paragraph>";
                        break;
                    case "List":
                        List list = sectionToMarkup.ContentQueue.Dequeue() as List;
                        if (list != null)
                        {
                            while (list.ContentQueue.Count > 0)
                            {
                                DescriptionParagraph listItem = list.ContentQueue.Dequeue() as DescriptionParagraph;
                                if (listItem != null) markup += "<Paragraph Margin=\"25,0,0,16\" TextIndent=\"-25\">> " + listItem.FormatLinks() + "</Paragraph>";
                            }
                        }
                        break;
                    case "Quote":
                        Quote quote = sectionToMarkup.ContentQueue.Dequeue() as Quote;
                        if (quote != null) markup += "<Paragraph Margin=\"10\"><Bold>" + quote.FormatLinks() + "</Bold></Paragraph>";
                        break;
                    case "Section":
                        Section section = sectionToMarkup.ContentQueue.Dequeue() as Section;
                        if (section != null) markup += MarkupSection(section);
                        break;
                }
            }

            return markup;
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

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Teams" || e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _character);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _character.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _character.FirstAppearanceIssue);
                    break;
                default:
                    Section section = _characterDescription.Sections.First(d => d.Title == e.Section.Header.ToString());
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
            Frame.Navigate(typeof(VolumePage), _character.FirstAppearanceIssue.VolumeId);
        }

        #endregion
    }
}
