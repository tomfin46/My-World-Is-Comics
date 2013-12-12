using Windows.UI.Xaml.Documents;
using MyWorldIsComics.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.Pages
{
    using System.ServiceModel.Security;

    using Windows.UI.Text;
    using Windows.UI.Xaml.Media.Imaging;

    using MyWorldIsComics.DataModel.Interfaces;
    using MyWorldIsComics.Helpers;

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DescriptionSectionPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Section _descSection;
        private RichTextBlock richTextBlock = new RichTextBlock();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }


        public DescriptionSectionPage()
        {
            InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (SavedData.DescriptionSection != null && SavedData.DescriptionSection.Title == pageTitle.Text) { _descSection = SavedData.DescriptionSection; }
            else { _descSection = e.NavigationParameter as Section; }
            
            if (_descSection == null) return;
            pageTitle.Text = _descSection.Title;

            PopulateRichTextBlock();
            RichTextBlockOverflow richTextBlockOverflow1 = new RichTextBlockOverflow {Margin = new Thickness(20)};
            richTextBlock.Margin = new Thickness(20);
            richTextBlock.OverflowContentTarget = richTextBlockOverflow1;

            DescriptionGrid.Children.Add(richTextBlock);
            Grid.SetColumn(richTextBlockOverflow1, 0);
            DescriptionGrid.Children.Add(richTextBlockOverflow1);
            Grid.SetColumn(richTextBlockOverflow1, 1);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.DescriptionSection = _descSection;
        }

        private void PopulateRichTextBlock()
        {
            if (_descSection == null) return;
            while (_descSection.ContentQueue.Count > 0)
            {
                var queuePeekType = _descSection.ContentQueue.Peek().GetType();
                switch (queuePeekType.Name)
                {
                    case "DescriptionParagraph":
                        DescriptionParagraph para = _descSection.ContentQueue.Dequeue() as DescriptionParagraph;
                        this.CreateParagraph(para);
                        break;
                    case "Figure":
                        Figure paraFig = _descSection.ContentQueue.Dequeue() as Figure;
                        this.CreateFigure(paraFig, DefaultParagraph());
                        break;
                    case "List":
                        List list = _descSection.ContentQueue.Dequeue() as List;
                        this.CreateList(list);
                        break;
                    case "Quote":
                        Quote quote = _descSection.ContentQueue.Dequeue() as Quote;
                        this.CreateQuote(quote);
                        break;
                    case "Section":
                        Section section = _descSection.ContentQueue.Dequeue() as Section;
                        this.CreateSection(section);
                        break;
                }
            }
        }

        #region Create Methods

        private void CreateSection(Section sectionToCreate)
        {
            if (sectionToCreate == null) return;

            Paragraph header = new Paragraph { Margin = new Thickness(0, 0, 0, 10), FontSize = 17 };
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
            }

            this.richTextBlock.Blocks.Add(header);

            while (sectionToCreate.ContentQueue.Count > 0)
            {
                var queuePeekType = sectionToCreate.ContentQueue.Peek().GetType();
                switch (queuePeekType.Name)
                {
                    case "DescriptionParagraph":
                        DescriptionParagraph para = sectionToCreate.ContentQueue.Dequeue() as DescriptionParagraph;
                        this.CreateParagraph(para);
                        break;
                    case "Figure":
                        Figure figure = sectionToCreate.ContentQueue.Dequeue() as Figure;
                        this.CreateFigure(figure, DefaultParagraph());
                        break;
                    case "List":
                        List list = sectionToCreate.ContentQueue.Dequeue() as List;
                        this.CreateList(list);
                        break;
                    case "Quote":
                        Quote quote = sectionToCreate.ContentQueue.Dequeue() as Quote;
                        this.CreateQuote(quote);
                        break;
                    case "Section":
                        Section section = sectionToCreate.ContentQueue.Dequeue() as Section;
                        this.CreateSection(section);
                        break;
                }
            }
        }

        private void CreateParagraph(DescriptionParagraph para)
        {
            if (para == null) return;
            Paragraph paragraph = DefaultParagraph();

            paragraph = FormatLinks(para, paragraph);

            if (_descSection.ContentQueue.Count > 0 && _descSection.ContentQueue.Peek().GetType() == typeof(Figure))
            {
                Figure paraFig = _descSection.ContentQueue.Dequeue() as Figure;
                this.CreateFigure(paraFig, paragraph);
            }
            else { this.richTextBlock.Blocks.Add(paragraph); }
        }

        private void CreateFigure(Figure figure, Paragraph paragraph)
        {
            if (figure == null) return;

            Image figImage = new Image { Source = new BitmapImage(figure.ImageSource), Stretch = Stretch.Uniform };

            if (figure.Text != null)
            {
                Run figCaptionRun = new Run { Text = figure.Text };
                Paragraph figCaption = new Paragraph
                                       {
                                           TextAlignment = TextAlignment.Center,
                                           Margin = new Thickness(0, 0, 0, 10)
                                       };
                figCaption.Inlines.Add(figCaptionRun);
                InlineUIContainer inlineUiContainer = new InlineUIContainer { Child = figImage };
                paragraph.Inlines.Add(inlineUiContainer);
                paragraph.TextAlignment = TextAlignment.Center;
                
                this.richTextBlock.Blocks.Add(paragraph);
                this.richTextBlock.Blocks.Add(figCaption);
            }
            else
            {
                InlineUIContainer inlineUiContainer = new InlineUIContainer { Child = figImage };
                paragraph.Inlines.Add(inlineUiContainer);

                this.richTextBlock.Blocks.Add(paragraph);
            }
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
                this.richTextBlock.Blocks.Add(paragraph);
            }
        }

        private void CreateQuote(Quote quote)
        {
            if (quote == null) return;

            Paragraph paragraph = new Paragraph { Margin = new Thickness(10), FontWeight = FontWeights.Bold };
            paragraph = FormatLinks(quote, paragraph);
            this.richTextBlock.Blocks.Add(paragraph);
        }

        #endregion

        #region Util Methods

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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Grid_LayoutUpdated(object sender, object e)
        {
            while ((this.DescriptionGrid.Children.Last() as RichTextBlockOverflow) != null && ((RichTextBlockOverflow)this.DescriptionGrid.Children.Last()).HasOverflowContent)
            {
                RichTextBlockOverflow lastTextBlockOverflow = this.DescriptionGrid.Children.Last() as RichTextBlockOverflow;
                if (lastTextBlockOverflow == null) return;

                int columnCount = DescriptionGrid.ColumnDefinitions.Count;
                ColumnDefinition newColumn = new ColumnDefinition { Width = new GridLength(520) };
                DescriptionGrid.ColumnDefinitions.Add(newColumn);

                RichTextBlockOverflow richTextBlockOverflow = new RichTextBlockOverflow { Margin = new Thickness(20) };
                lastTextBlockOverflow.OverflowContentTarget = richTextBlockOverflow;

                DescriptionGrid.Children.RemoveAt(columnCount-1);
                DescriptionGrid.Children.Add(lastTextBlockOverflow);
                DescriptionGrid.Children.Add(richTextBlockOverflow);
                Grid.SetColumn(richTextBlockOverflow, columnCount);
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
