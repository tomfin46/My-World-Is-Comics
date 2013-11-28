using Windows.ApplicationModel.Store;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Paragraph = MyWorldIsComics.DataModel.DescriptionContent.Paragraph;

namespace MyWorldIsComics.ResourcePages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Navigation;

    using MyWorldIsComics.Common;
    using MyWorldIsComics.DataModel;
    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Interfaces;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Mappers;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CharacterPage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private ObservableDictionary _defaultViewModel = new ObservableDictionary();
        private Character _character;
        private Description _description;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this._defaultViewModel; }
            set { this._defaultViewModel = value; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }

        public CharacterPage()
        {
            this.InitializeComponent();

            this._character = new Character();

            this._navigationHelper = new NavigationHelper(this);
            this._navigationHelper.LoadState += this.navigationHelper_LoadState;
            this._navigationHelper.SaveState += this.navigationHelper_SaveState;
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
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            string name = e.NavigationParameter as string;

            string prevName = String.Empty;
            if (SavedData.QuickCharacter != null)
            {
                prevName = SavedData.QuickCharacter.Name;
            }

            try
            {
                await this.LoadQuickCharacter(name);

                this.BioHubSection.Visibility = Visibility.Visible;

                await this.LoadDescription(this.DefaultViewModel["QuickCharacter"] as Character);

                this.CreateDataTemplates();

                await this.LoadCharacter(this.DefaultViewModel["QuickCharacter"] as Character);

                if (_character.FirstAppearanceId != 0) { this.TeamSection.IsHeaderInteractive = true; }

                await this.LoadFirstAppearance(this.DefaultViewModel["Character"] as Character, prevName);

                if (_character.FirstAppearanceId != 0) { this.FirstAppearanceSection.IsHeaderInteractive = true; }

                await this.LoadQuickTeams(this.DefaultViewModel["Character"] as Character, prevName);
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private void CreateDataTemplates()
        {
            if (_description.CurrentEvents == null) this.HideHubSection("Current Events");
            else this.CreateDataTemplate(_description.CurrentEvents);

            if (_description.Origin == null) this.HideHubSection("Origin");
            else this.CreateDataTemplate(_description.Origin);

            if (_description.Creation == null) this.HideHubSection("Creation");
            else this.CreateDataTemplate(_description.Creation);

            if (_description.DistinguishingCharacteristics == null) this.HideHubSection("Distinguishing Characteristics");
            else this.CreateDataTemplate(_description.DistinguishingCharacteristics);

            if (_description.CharacterEvolution == null) this.HideHubSection("Character Evolution");
            else this.CreateDataTemplate(_description.CharacterEvolution);

            if (_description.MajorStoryArcs == null) this.HideHubSection("Major Story Arcs");
            else this.CreateDataTemplate(_description.MajorStoryArcs);

            if (_description.PowersAndAbilities == null) this.HideHubSection("Powers and Abilities");
            else this.CreateDataTemplate(_description.PowersAndAbilities);

            if (_description.WeaponsAndEquipment == null) this.HideHubSection("Weapons and Equipment");
            else this.CreateDataTemplate(_description.WeaponsAndEquipment);

            if (_description.AlternateRealities == null) this.HideHubSection("Alternate Realities");
            else this.CreateDataTemplate(_description.AlternateRealities);

            if (_description.OtherMedia == null) this.HideHubSection("Other Media");
            else this.CreateDataTemplate(_description.OtherMedia);
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
                this.SetHubSectionContentTemplate(dataTemplate, descriptionSection.Title);
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
                    this.CurrentEventsHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Origin":
                case "Origins":
                    this.OriginHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Creation":
                    this.CreationHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Distinguishing Characteristics":
                    break;
                case "Character Evolution":
                    this.CharacterEvolutionHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Major Story Arcs":
                    break;
                case "Powers and Abilities":
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
                    this.CurrentEventsHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Origin":
                case "Origins":
                    this.OriginHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Creation":
                    this.CreationHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Distinguishing Characteristics":
                    break;
                case "Character Evolution":
                    this.CharacterEvolutionHubSection.ContentTemplate = sectionTemplate;
                    break;
                case "Major Story Arcs":
                    break;
                case "Powers and Abilities":
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

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (this.Frame.CurrentSourcePageType.Name == "HubPage")
            {
                this._character = null;
            }
            else
            {
                // Save response content so don't have to fetch from api service again
                SavedData.QuickCharacter = this.DefaultViewModel["QuickCharacter"] as Character;
                SavedData.Character = this.DefaultViewModel["Character"] as Character;
                SavedData.FirstAppearance = this.DefaultViewModel["FirstAppearance"] as Issue;
            }
        }

        private async Task LoadQuickCharacter(string name)
        {
            if (SavedData.QuickCharacter != null && SavedData.QuickCharacter.Name == name)
            {
                this.DefaultViewModel["QuickCharacter"] = SavedData.QuickCharacter;
            }
            else
            {
                var quickCharacterString = await ComicVineSource.ExecuteSearchAsync(name);
                this._character = this.MapQuickCharacter(quickCharacterString);
                this.DefaultViewModel["QuickCharacter"] = this._character;
            }
        }

        private Character MapQuickCharacter(string quickCharacterString)
        {
            if (quickCharacterString == ServiceConstants.QueryNotFound) return new Character();
            Character quickCharacter;
            CharacterMapper.QuickMapXmlObject(quickCharacterString, out quickCharacter);
            return quickCharacter;
        }

        private async Task LoadDescription(Character quickCharacter)
        {
            var characterDescription = await ComicVineSource.FormatDescriptionAsync(quickCharacter.DescriptionString);
            _description = characterDescription;
            this.DefaultViewModel["CharacterDescription"] = characterDescription;
        }

        private async Task LoadCharacter(Character quickCharacter)
        {
            if (SavedData.Character != null && SavedData.Character.Name == quickCharacter.Name)
            {
                this._character = SavedData.Character;
                this.DefaultViewModel["Character"] = this._character;
            }
            else
            {
                var characterString = await ComicVineSource.GetCharacterAsync(quickCharacter.UniqueId);
                this._character = this.MapCharacter(characterString);
                this.DefaultViewModel["Character"] = this._character;
            }
        }

        private Character MapCharacter(string characterString)
        {
            if (characterString == ServiceConstants.QueryNotFound)
            {
                return new Character
                {
                    TeamIds = new List<int>(),
                    Teams = new ObservableCollection<Team>()
                    {
                        new Team
                        {
                            Name = characterString
                        }
                    }
                };
            }

            Character characterToMap;
            CharacterMapper.MapXmlObject(characterString, out characterToMap);
            return characterToMap;
        }

        private async Task LoadQuickTeams(Character character, string prevName)
        {
            if (SavedData.Character == null || character.Name != prevName || SavedData.Character.Teams == null)
            {
                foreach (int teamId in character.TeamIds.Take(6))
                {
                    Team team = this.MapQuickTeam(await ComicVineSource.GetQuickTeamAsync(teamId.ToString()));
                    if (character.Teams.Any(t => t.UniqueId == team.UniqueId)) continue;
                    character.Teams.Add(team);
                }
            }
        }

        private Team MapQuickTeam(string quickTeam)
        {
            return quickTeam == ServiceConstants.QueryNotFound ? new Team { Name = "Team Not Found" } : new TeamMapper().QuickMapXmlObject(quickTeam);
        }

        private async Task LoadFirstAppearance(Character character, string prevName)
        {
            if (SavedData.FirstAppearance != null && SavedData.Character != null &&
                SavedData.Character.FirstAppearanceId == SavedData.FirstAppearance.UniqueId)
            {
                this.DefaultViewModel["FirstAppearance"] = SavedData.FirstAppearance;
            }
            else
            {
                if (character.FirstAppearanceId != 0)
                {
                    Issue issue = this.MapIssue(await ComicVineSource.GetIssueAsync(character.FirstAppearanceId.ToString()));
                    character.FirstAppearanceIssue = issue;
                    this.DefaultViewModel["FirstAppearance"] = character.FirstAppearanceIssue;
                }
            }
        }

        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
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
            this._navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this._navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        #region Event Listners

        private void TeamView_TeamClick(object sender, ItemClickEventArgs e)
        {
            var team = ((Team)e.ClickedItem);
            this.Frame.Navigate(typeof(TeamPage), team);
        }

        private void Page_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.BackButton.Visibility = this.BackButton.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            var characterToSend = this._character;
            this.Frame.Navigate(typeof(TeamsPage), characterToSend);
        }

        #endregion
    }
}
