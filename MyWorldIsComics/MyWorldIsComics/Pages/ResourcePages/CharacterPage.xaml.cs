﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.Common;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Mappers;
using MyWorldIsComics.Pages.CollectionPages;
using Paragraph = MyWorldIsComics.DataModel.DescriptionContent.Paragraph;

namespace MyWorldIsComics.Pages.ResourcePages
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private ObservableDictionary _characterPageViewModel = new ObservableDictionary();

        private Character _basicCharacterForPage;
        private Character _filteredCharacterForPage;
        private Description _characterDescriptionForPage;

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

            string name = e.NavigationParameter as string;

            try
            {
                if (SavedData.BasicCharacter != null && SavedData.BasicCharacter.Name == name)
                {
                    _basicCharacterForPage = SavedData.BasicCharacter;
                    CharacterPageViewModel["BasicCharacter"] = _basicCharacterForPage;
                    BioHubSection.Visibility = Visibility.Visible;
                }
                else
                {
                    await LoadBasicCharacter(name);
                }

                await LoadDescription();

                if (SavedData.Character != null && SavedData.Character.UniqueId == _basicCharacterForPage.UniqueId)
                {
                    _filteredCharacterForPage = SavedData.Character;
                    CharacterPageViewModel["FilteredCharacter"] = _filteredCharacterForPage;
                    HideOrShowFilteredSections();
                }
                else
                {
                    await LoadCharacter();
                }
            }
            catch (HttpRequestException)
            {
                _basicCharacterForPage = new Character();
                _basicCharacterForPage.Name = "An internet connection is required here";
                CharacterPageViewModel["BasicCharacter"] = _basicCharacterForPage;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.BasicCharacter = _basicCharacterForPage;
            SavedData.Character = _filteredCharacterForPage;
        }

        #region Load Character

        private async Task LoadBasicCharacter(string name)
        {
            try
            {
                await SearchForCharacter(name);
                CharacterPageViewModel["BasicCharacter"] = _basicCharacterForPage;
                BioHubSection.Visibility = Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            try
            {
                await FormatDescriptionForPage();
                _characterDescriptionForPage.UniqueId = _basicCharacterForPage.UniqueId;
                CreateDataTemplates();
                CharacterPageViewModel["CharacterDescription"] = _characterDescriptionForPage;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadCharacter()
        {
            try
            {
                List<string> filters = new List<string> { "first_appeared_in_issue", "teams" };

                foreach (string filter in filters)
                {
                    await FetchFilteredCharacterResource(filter);
                    switch (filter)
                    {
                        case "first_appeared_in_issue":
                            await FetchFirstAppearance();
                            break;
                        case "teams":
                            await FetchFirstTeam();
                            break;
                    }
                    CharacterPageViewModel["FilteredCharacter"] = _filteredCharacterForPage;
                    HideOrShowFilteredSections();
                }
                await FetchRemainingTeams();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private void HideOrShowFilteredSections()
        {
            TeamSection.Visibility = _filteredCharacterForPage.TeamIds.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _filteredCharacterForPage.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        private async Task SearchForCharacter(string name)
        {
            var characterSearchString = await ComicVineSource.ExecuteSearchAsync(name);
            _basicCharacterForPage = GetMappedCharacterFromSearch(characterSearchString);
        }

        #region Fetch methods

        private async Task FetchFilteredCharacterResource(string filter)
        {
            string filteredCharacterString = await ComicVineSource.GetFilteredCharacterAsync(_basicCharacterForPage.UniqueId, filter);
            _filteredCharacterForPage = GetMappedCharacterFromFilter(filteredCharacterString, filter);
        }

        private async Task FetchFirstTeam()
        {
            foreach (int teamId in _filteredCharacterForPage.TeamIds.Take(1))
            {
                Team team = GetMappedQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                if (_filteredCharacterForPage.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                _filteredCharacterForPage.Teams.Add(team);
            }
        }

        private async Task FetchRemainingTeams()
        {
            var firstId = _filteredCharacterForPage.TeamIds.First();
            foreach (int teamId in _filteredCharacterForPage.TeamIds.Where(id => id != firstId).Take(_filteredCharacterForPage.TeamIds.Count - 1))
            {
                Team team = GetMappedQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                if (_filteredCharacterForPage.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                _filteredCharacterForPage.Teams.Add(team);
            }
        }

        private async Task FetchFirstAppearance()
        {
            if (_basicCharacterForPage.FirstAppearanceId != 0)
            {
                Issue issue = GetMappedIssue(await ComicVineSource.GetQuickIssueAsync(_basicCharacterForPage.FirstAppearanceId));
                _filteredCharacterForPage.FirstAppearanceIssue = issue;
            }
        }

        #endregion

        #region Get Mappings

        private Character GetMappedCharacterFromSearch(string characterSearchString)
        {
            if (characterSearchString == ServiceConstants.QueryNotFound)
            {
                return new Character { Name = ServiceConstants.QueryNotFound };
            }
            return new CharacterMapper().MapSearchXmlObject(characterSearchString);
        }

        private Character GetMappedCharacterFromFilter(string filteredCharacterString, string filter)
        {
            if (filteredCharacterString == ServiceConstants.QueryNotFound)
            {
                return new Character
                {
                    TeamIds = new List<int>(),
                    Teams = new ObservableCollection<Team>()
                    {
                        new Team
                        {
                            Name = filteredCharacterString
                        }
                    },
                    FirstAppearanceIssue = new Issue
                    {
                        Name = filteredCharacterString
                    }
                };
            }
            return new CharacterMapper().MapFilteredXmlObject(_basicCharacterForPage, filteredCharacterString, filter);
        }

        private Team GetMappedQuickTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private Issue GetMappedIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _characterDescriptionForPage = await ComicVineSource.FormatDescriptionAsync(_basicCharacterForPage.DescriptionString);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            if (_characterDescriptionForPage.CurrentEvents == null || _characterDescriptionForPage.CurrentEvents.ContentQueue.Count == 0) HideHubSection("Current Events");
            else CreateDataTemplate(_characterDescriptionForPage.CurrentEvents);

            if (_characterDescriptionForPage.Origin == null || _characterDescriptionForPage.Origin.ContentQueue.Count == 0) HideHubSection("Origin");
            else CreateDataTemplate(_characterDescriptionForPage.Origin);

            if (_characterDescriptionForPage.Creation == null || _characterDescriptionForPage.Creation.ContentQueue.Count == 0) HideHubSection("Creation");
            else CreateDataTemplate(_characterDescriptionForPage.Creation);

            if (_characterDescriptionForPage.DistinguishingCharacteristics == null || _characterDescriptionForPage.DistinguishingCharacteristics.ContentQueue.Count == 0) HideHubSection("Distinguishing Characteristics");
            else CreateDataTemplate(_characterDescriptionForPage.DistinguishingCharacteristics);

            if (_characterDescriptionForPage.CharacterEvolution == null || _characterDescriptionForPage.CharacterEvolution.ContentQueue.Count == 0) HideHubSection("Character Evolution");
            else CreateDataTemplate(_characterDescriptionForPage.CharacterEvolution);

            if (_characterDescriptionForPage.MajorStoryArcs == null || _characterDescriptionForPage.MajorStoryArcs.ContentQueue.Count == 0) HideHubSection("Major Story Arcs");
            else CreateDataTemplate(_characterDescriptionForPage.MajorStoryArcs);

            if (_characterDescriptionForPage.PowersAndAbilities == null || _characterDescriptionForPage.PowersAndAbilities.ContentQueue.Count == 0) HideHubSection("Powers and Abilities");
            else CreateDataTemplate(_characterDescriptionForPage.PowersAndAbilities);

            if (_characterDescriptionForPage.WeaponsAndEquipment == null || _characterDescriptionForPage.WeaponsAndEquipment.ContentQueue.Count == 0) HideHubSection("Weapons and Equipment");
            else CreateDataTemplate(_characterDescriptionForPage.WeaponsAndEquipment);

            if (_characterDescriptionForPage.AlternateRealities == null || _characterDescriptionForPage.AlternateRealities.ContentQueue.Count == 0) HideHubSection("Alternate Realities");
            else CreateDataTemplate(_characterDescriptionForPage.AlternateRealities);

            if (_characterDescriptionForPage.OtherMedia == null || _characterDescriptionForPage.OtherMedia.ContentQueue.Count == 0) HideHubSection("Other Media");
            else CreateDataTemplate(_characterDescriptionForPage.OtherMedia);
        }

        private void CreateDataTemplate(Section descriptionSection)
        {
            String markup = String.Empty;
            markup += "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:MyWorldIsComics\">";
            markup += "<ScrollViewer VerticalScrollBarVisibility=\"Hidden\">";
            markup += "<RichTextBlock>";

            if (descriptionSection != null)
            {
                while (descriptionSection.ContentQueue.Count > 0)
                {
                    var queuePeekType = descriptionSection.ContentQueue.Peek().GetType();
                    switch (queuePeekType.Name)
                    {
                        case "Paragraph":
                            Paragraph para = descriptionSection.ContentQueue.Dequeue() as Paragraph;
                            if (para != null)
                                markup +=
                                    "<Paragraph FontSize=\"15\" FontFamily=\"Segoe UI Semilight\" Margin=\"0,0,0,10\">" + para.FormatLinks();
                            if (descriptionSection.ContentQueue.Count > 0 && descriptionSection.ContentQueue.Peek().GetType() == typeof(Figure))
                            {
                                Figure paraFig = descriptionSection.ContentQueue.Dequeue() as Figure;
                                if (paraFig != null) markup += "<InlineUIContainer><Image Source=\"" + paraFig.ImageSource + "\" Stretch=\"Fill\"/></InlineUIContainer>" +
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
                            if (fig != null) markup += "<Paragraph>" +
                                                       "<InlineUIContainer><Image Source=\"" + fig.ImageSource + "\" Stretch=\"Fill\"/>" + "</InlineUIContainer>" +
                                                       "</Paragraph>" +
                                                       "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + fig.Text + "</Paragraph>";
                            break;
                        case "List":
                            List list = descriptionSection.ContentQueue.Dequeue() as List;
                            if (list != null)
                            {
                                while (list.ContentQueue.Count > 0)
                                {
                                    Paragraph listItem = list.ContentQueue.Dequeue() as Paragraph;
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
                SetHubSectionContentTemplate(dataTemplate, descriptionSection.Title);
            }
        }

        private static string MarkupSection(Section sectionToMarkup)
        {
            string markup = String.Empty;

            if (sectionToMarkup != null)
            {
                markup += "<Paragraph Margin=\"0,0,0,10\" FontSize=\"17\">";
                if (sectionToMarkup.Type == "h3") markup += "<Bold>" + sectionToMarkup.Title + "</Bold>";
                else if (sectionToMarkup.Type == "h4") markup += "<Underline>" + sectionToMarkup.Title + "</Underline>";
                markup += "</Paragraph>";

                while (sectionToMarkup.ContentQueue.Count > 0)
                {
                    var queuePeekType = sectionToMarkup.ContentQueue.Peek().GetType();
                    switch (queuePeekType.Name)
                    {
                        case "Paragraph":
                            Paragraph para = sectionToMarkup.ContentQueue.Dequeue() as Paragraph;
                            if (para != null)
                                markup += "<Paragraph FontSize=\"15\" FontFamily=\"Segoe UI Semilight\" Margin=\"0,0,0,10\">" + para.FormatLinks();
                            if (sectionToMarkup.ContentQueue.Count > 0 && sectionToMarkup.ContentQueue.Peek().GetType() == typeof(Figure))
                            {
                                Figure paraFig = sectionToMarkup.ContentQueue.Dequeue() as Figure;
                                if (paraFig != null) markup += "<InlineUIContainer><Image Source=\"" + paraFig.ImageSource + "\" Stretch=\"Fill\"/></InlineUIContainer>" +
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
                            if (fig != null) markup += "<Paragraph>" +
                                                       "<InlineUIContainer><Image Source=\"" + fig.ImageSource + "\" Stretch=\"Fill\"/>" + "</InlineUIContainer>" +
                                                       "</Paragraph>" +
                                                       "<Paragraph TextAlignment=\"Center\" Margin=\"0,0,0,10\">" + fig.Text + "</Paragraph>";
                            break;
                        case "List":
                            List list = sectionToMarkup.ContentQueue.Dequeue() as List;
                            if (list != null)
                            {
                                while (list.ContentQueue.Count > 0)
                                {
                                    Paragraph listItem = list.ContentQueue.Dequeue() as Paragraph;
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
            }

            return markup;
        }

        private void HideHubSection(string sectionTitle)
        {
            switch (sectionTitle)
            {
                case "Current Events":
                    CurrentEventsHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Origin":
                case "Origins":
                    OriginHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Creation":
                    CreationHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Distinguishing Characteristics":
                    break;
                case "Character Evolution":
                    CharacterEvolutionHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Major Story Arcs":
                    break;
                case "Powers and Abilities":
                case "Abilties":
                case "Powers":
                case "Powers and Abilties":
                    PowersAndAbilitiesHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Other Versions":
                case "Alternate Realities":
                    break;
                case "Other Media":
                    break;
            }
        }

        private void SetHubSectionContentTemplate(DataTemplate sectionTemplate, string sectionTitle)
        {
            switch (sectionTitle)
            {
                case "Current Events":
                    CurrentEventsHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Origin":
                case "Origins":
                    OriginHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Creation":
                    CreationHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Distinguishing Characteristics":
                    break;
                case "Character Evolution":
                    CharacterEvolutionHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Major Story Arcs":
                    break;
                case "Powers and Abilities":
                case "Abilties":
                case "Powers":
                case "Powers and Abilties":
                    PowersAndAbilitiesHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Weapons and Equipment":
                    break;
                case "Other Versions":
                case "Alternate Realities":
                    break;
                case "Other Media":
                    break;
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

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            Frame.Navigate(typeof(TeamPage), team);
        }

        private void Page_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BackButton.Visibility = BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            switch (e.Section.Header.ToString())
            {
                case "Teams":
                    Frame.Navigate(typeof(TeamsPage), _filteredCharacterForPage);
                    break;
                case "First Appearance":
                    Frame.Navigate(typeof(IssuePage), _filteredCharacterForPage.FirstAppearanceIssue);
                    break;
            }
        }

        #endregion
    }
}
