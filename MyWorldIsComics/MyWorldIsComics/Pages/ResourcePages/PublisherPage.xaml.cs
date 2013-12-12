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

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

namespace MyWorldIsComics.Pages.ResourcePages
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Windows.UI.Xaml.Markup;

    using MyWorldIsComics.DataModel.DescriptionContent;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;
    using MyWorldIsComics.Helpers;
    using MyWorldIsComics.Mappers;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class PublisherPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary publisherPageViewModel = new ObservableDictionary();

        private Publisher _publisher;

        private Description _publisherDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary PublisherPageViewModel
        {
            get { return this.publisherPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public PublisherPage()
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
                await LoadPublisher(id);
            }
            catch (HttpRequestException)
            {
                _publisher = new Publisher { Name = "An internet connection is required here" };
                PublisherPageViewModel["Publisher"] = _publisher;
            }
        }
        
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Publisher = _publisher;
        }

        #region Load Publisher

        private async Task LoadPublisher(int id)
        {
            try
            {
                if (SavedData.Publisher != null && SavedData.Publisher.UniqueId == id) { _publisher = SavedData.Publisher; }
                else { _publisher = await this.GetPublisher(id); }

                PublisherPageViewModel["Publisher"] = _publisher;
                PageTitle.Text = _publisher.Name;

                if (_publisher.Name != ServiceConstants.QueryNotFound)
                {
                    ImageHubSection.Visibility = Visibility.Collapsed;
                    BioHubSection.Visibility = Visibility.Visible;

                    await LoadDescription();
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
            _publisherDescription.UniqueId = _publisher.UniqueId;
            CreateDataTemplates();
        }

        private async Task<Publisher> GetPublisher(int id)
        {
            var publisherSearchString = await ComicVineSource.GetPublisherAsync(id);
            return this.MapPublisher(publisherSearchString);
        }
        
        #endregion

        #region Mapping Methods

        private Publisher MapPublisher(string publisherString)
        {
            return publisherString == ServiceConstants.QueryNotFound ? new Publisher { Name = ServiceConstants.QueryNotFound } : new PublisherMapper().MapXmlObject(publisherString);
        }
    
        #endregion

        private async Task FormatDescriptionForPage()
        {
            _publisherDescription = await ComicVineSource.FormatDescriptionAsync(_publisher.DescriptionString);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            int i = 3;
            foreach (Section section in _publisherDescription.Sections)
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Listeners

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                default:
                    Frame.Navigate(typeof(DescriptionSectionPage), _publisherDescription.Sections.First(d => d.Title == e.Section.Header.ToString()));
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
        
    }
}
