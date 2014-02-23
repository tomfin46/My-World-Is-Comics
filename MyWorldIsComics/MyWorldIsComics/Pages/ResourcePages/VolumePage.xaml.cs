using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MyWorldIsComics.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyWorldIsComics.DataModel.ResponseSchemas;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Mappers;

namespace MyWorldIsComics.Pages.ResourcePages
{
    using Windows.UI.Text;
    using Windows.UI.Xaml.Documents;

    using DataModel.DescriptionContent;
    using DataModel.Interfaces;

    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class VolumePage : Page
    {
        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _volumePageViewModel = new ObservableDictionary();

        private Volume _volume;

        private readonly RichTextBlock _volumeDescription = new RichTextBlock();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary VolumePageViewModel
        {
            get { return _volumePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }


        public VolumePage()
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

            Volume volume = e.NavigationParameter as Volume;

            int id;
            if (volume != null)
            {
                id = volume.Id;
                _volume = volume;

                if (volume.Issues != null)
                {
                    _volume.Issues = new ObservableCollection<Issue>(_volume.Issues.OrderByDescending(i => i.IssueNumberInteger));
                }

                VolumePageViewModel["Volume"] = _volume;
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
                if (SavedData.Volume != null && SavedData.Volume.Id == id && SavedData.Volume.Image != null) { _volume = SavedData.Volume; }
                else
                {
                    _volume = await GetVolume(id);
                    _volume.Issues = new ObservableCollection<Issue>(_volume.Issues.OrderByDescending(i => i.IssueNumberInteger));
                }
            }
            catch (HttpRequestException)
            {
                _volume = new Volume { Name = "An internet connection is required here" };
                VolumePageViewModel["Volume"] = _volume;
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }

            VolumePageViewModel["Volume"] = _volume;
            await LoadVolume();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Volume = _volume;
        }

        #region Load Volume
        private async Task LoadVolume()
        {
            try
            {
                await LoadDescription();

                if (_volume.Issues.Count > 0) { await FetchFirstIssue(); }
                if (_volume.Issues.Count > 1) await FetchRemainingIssues();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            CreateRichTextBlock();
            if (_volumeDescription.Blocks.Count > 0) VolumeStackPanel.Children.Add(_volumeDescription);
        } 

        #endregion

        private async Task<Volume> GetVolume(int id)
        {
            List<string> filters = new List<string> { "count_of_issues", "description", "first_issue", "id", "image", "issues", "name", "publisher", "start_year" };
            var volumeSearchString = await ComicVineSource.GetFilteredVolumeAsync(id, filters);
            return MapVolume(volumeSearchString);
        }

        #region Fetch Methods

        private async Task FetchFirstIssue()
        {
            Issue issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_volume.Issues.First().Id));
            if (_volume.Issues.Any(i => i.Id == issue.Id)) return;
            _volume.Issues.Add(issue);
        }

        private async Task FetchRemainingIssues()
        {
            for (int j = 0; j < _volume.Issues.Count; ++j)
            {
                Issue issue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_volume.Issues[j].Id));
                _volume.Issues[j] = issue;
            }
            _volume.Issues = new ObservableCollection<Issue>(_volume.Issues.OrderByDescending(i => i.CoverDateDateTime));
            VolumePageViewModel["Volume"] = _volume;
        } 

        #endregion

        #region Map Methods
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
            _volume.DescriptionSection = await ComicVineSource.FormatDescriptionAsync(_volume);
        }
        
        #region Create RichTextBlock

        private void CreateRichTextBlock()
        {
            CreateSection(_volume.DescriptionSection);
        }

        private void CreateSection(Section sectionToCreate)
        {
            if (sectionToCreate.ContentQueue.Count == 0 && sectionToCreate.Title == null) return;

            if (sectionToCreate.Title != null)
            {
                Paragraph header = DefaultParagraph();
                Run headerRun = new Run { Text = sectionToCreate.Title };

                switch (sectionToCreate.Type)
                {
                    case "h3":
                        Bold h3Bold = new Bold();
                        h3Bold.Inlines.Add(headerRun);
                        header.Inlines.Add(h3Bold);
                        break;
                    case "h4":
                        Underline h4Underline = new Underline();
                        h4Underline.Inlines.Add(headerRun);
                        header.Inlines.Add(h4Underline);
                        break;
                    default:
                        header.Inlines.Add(headerRun);
                        break;
                }

                _volumeDescription.Blocks.Add(header); 
            }

            while (sectionToCreate.ContentQueue.Count > 0)
            {
                var queuePeekType = sectionToCreate.ContentQueue.Peek().GetType();
                switch (queuePeekType.Name)
                {
                    case "DescriptionParagraph":
                        DescriptionParagraph para = sectionToCreate.ContentQueue.Dequeue() as DescriptionParagraph;
                        CreateParagraph(para);
                        break;
                    case "List":
                        List list = sectionToCreate.ContentQueue.Dequeue() as List;
                        CreateList(list);
                        break;
                    case "Quote":
                        Quote quote = sectionToCreate.ContentQueue.Dequeue() as Quote;
                        CreateQuote(quote);
                        break;
                    case "Section":
                        Section section = sectionToCreate.ContentQueue.Dequeue() as Section;
                        CreateSection(section);
                        break;
                }
            }
        }

        private void CreateParagraph(DescriptionParagraph para)
        {
            if (para == null) return;
            Paragraph paragraph = DefaultParagraph();

            paragraph = FormatLinks(para, paragraph);
            _volumeDescription.Blocks.Add(paragraph);
        }

        private void CreateList(List list)
        {
            if (list == null) return;

            while (list.ContentQueue.Count > 0)
            {
                DescriptionParagraph listItem = list.ContentQueue.Dequeue() as DescriptionParagraph;
                if (listItem == null) continue;

                Paragraph paragraph = new Paragraph
                {
                    Margin = new Thickness(25, 0, 0, 16),
                    TextIndent = -25
                };
                paragraph = FormatLinks(listItem, paragraph);
                _volumeDescription.Blocks.Add(paragraph);
            }
        }

        private void CreateQuote(Quote quote)
        {
            if (quote == null) return;

            Paragraph paragraph = new Paragraph { Margin = new Thickness(10), FontWeight = FontWeights.Bold };
            paragraph = FormatLinks(quote, paragraph);
            _volumeDescription.Blocks.Add(paragraph);
        }

        private static Paragraph DefaultParagraph()
        {
            return new Paragraph
            {
                FontSize = 15,
                FontFamily = new FontFamily("Segoe UI Semilight"),
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        private static Paragraph FormatLinks(IDescriptionContent descriptionContent, Paragraph paragraph)
        {
            var text = descriptionContent.Text;
            foreach (Link link in descriptionContent.Links)
            {
                if (link.Text == String.Empty) continue;
                Link linkCopy = link;
                Boolean contains = false;
                foreach (
                    Link link2 in
                        descriptionContent.Links.Where(link1 => link1.Text != linkCopy.Text)
                            .Where(link2 => link2.Text.Contains(linkCopy.Text)))
                {
                    contains = true;
                }

                if (contains) continue;

                text = text.Replace(link.Text, "<" + link.Text + ">");
            }

            var splitString = text.Split('<');
            paragraph.Inlines.Add(new Run { Text = splitString[0] });

            foreach (string s in splitString.Where(s => s != splitString.First()))
            {
                Hyperlink hyperlink = new Hyperlink();
                Run hyperlinkRun = new Run { Text = s.Substring(0, s.IndexOf('>')) };
                Link link = descriptionContent.Links.FirstOrDefault(l => l.Text == hyperlinkRun.Text);
                if (link != null)
                {
                    var ids = link.DataRefId.Split('-');
                    if (ids.Length < 2) continue;
                    hyperlink.NavigateUri = new Uri("myworldiscomics:///" + ids[0] + "/" + ids[1]);
                }
                hyperlink.Inlines.Add(hyperlinkRun);

                paragraph.Inlines.Add(hyperlink);

                paragraph.Inlines.Add(new Run { Text = s.Substring(s.IndexOf('>') + 1) });
            }

            return paragraph;
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

        private void IssueView_IssueClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(IssuePage), e.ClickedItem);
        }
    }
}
