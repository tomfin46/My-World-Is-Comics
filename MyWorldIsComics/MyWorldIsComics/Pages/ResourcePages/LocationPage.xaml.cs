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
    using MyWorldIsComics.Pages.CollectionPages;

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class LocationPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary locationPageViewModel = new ObservableDictionary();

        private Location _location;

        private Description _locationDescription;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary LocationPageViewModel
        {
            get { return this.locationPageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public LocationPage()
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
                await LoadLocation(id);
            }
            catch (HttpRequestException)
            {
                _location = new Location { Name = "An internet connection is required here" };
                LocationPageViewModel["Location"] = _location;
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Frame.CurrentSourcePageType.Name == "HubPage") { return; }

            // Save response content so don't have to fetch from api service again
            SavedData.Location = _location;
        }

        private async Task LoadLocation(int id)
        {
            try
            {
                if (SavedData.Location != null && SavedData.Location.UniqueId == id) { _location = SavedData.Location; }
                else { _location = await GetLocation(id); }

                LocationPageViewModel["Location"] = _location;
                ImageHubSection.Visibility = Visibility.Collapsed;
                BioHubSection.Visibility = Visibility.Visible;

                await LoadDescription();

                if (_location.FirstAppearanceIssue == null) { await FetchFirstAppearance(); }
                HideOrShowSections();

                if (_location.Volumes.Count == 0) { await FetchFirstVolume(); }
                HideOrShowSections();
                if (_location.VolumeIds.Count > 1) await FetchRemainingVolumes();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            _locationDescription.UniqueId = _location.UniqueId;
            CreateDataTemplates();
        }

        private void HideOrShowSections()
        {
            VolumeSection.Visibility = _location.VolumeIds.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FirstAppearanceSection.Visibility = _location.FirstAppearanceIssue.UniqueId != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task<Location> GetLocation(int id)
        {
            var locationSearchString = await ComicVineSource.GetLocationAsync(id);
            return MapLocation(locationSearchString);
        }

        #region Fetch methods

        private async Task FetchFirstAppearance()
        {
            if (_location.FirstAppearanceId != 0)
            {
                _location.FirstAppearanceIssue = MapIssue(await ComicVineSource.GetQuickIssueAsync(_location.FirstAppearanceId));
                //this.FormatIssueDescriptionForPage();
            }
        }

        private async Task FetchFirstVolume()
        {
            foreach (int volumeId in _location.VolumeIds.Take(1))
            {
                Volume volume = MapQuickVolume(await ComicVineSource.GetQuickVolumeAsync(volumeId));
                if (_location.Volumes.Any(t => t.UniqueId == volume.UniqueId)) continue;
                _location.Volumes.Add(volume);
            }
        }

        private async Task FetchRemainingVolumes()
        {
            var firstId = _location.VolumeIds.First();
            foreach (int volumeId in _location.VolumeIds.Where(id => id != firstId).Take(_location.VolumeIds.Count - 1))
            {
                Volume volume = MapQuickVolume(await ComicVineSource.GetQuickVolumeAsync(volumeId));
                if (_location.Volumes.Any(t => t.UniqueId == volume.UniqueId)) continue;
                _location.Volumes.Add(volume);
            }
        }

        #endregion

        #region Mapping Methods

        private Location MapLocation(string locationString)
        {
            return locationString == ServiceConstants.QueryNotFound ? new Location { Name = ServiceConstants.QueryNotFound } : new LocationMapper().MapXmlObject(locationString);
        }

        private Volume MapQuickVolume(string quickVolume)
        {
            return quickVolume == ServiceConstants.QueryNotFound ? new Volume { Name = "Volume Not Found" } : new VolumeMapper().QuickMapXmlObject(quickVolume);
        }

        private Issue MapIssue(string issue)
        {
            return issue == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issue);
        }

        #endregion

        private async Task FormatDescriptionForPage()
        {
            _locationDescription = await ComicVineSource.FormatDescriptionAsync(_location.DescriptionString);
        }

        #region DataTemplate Creation

        private void CreateDataTemplates()
        {
            if (_locationDescription.Overview == null || _locationDescription.Overview.ContentQueue.Count == 0) HideHubSection("Overview");
            else CreateDataTemplate(_locationDescription.Overview);

            if (_locationDescription.OtherMedia == null || _locationDescription.OtherMedia.ContentQueue.Count == 0) HideHubSection("Other Media");
            else CreateDataTemplate(_locationDescription.OtherMedia);

            if (_locationDescription.EmptyHeader == null || _locationDescription.EmptyHeader.ContentQueue.Count == 0) HideHubSection(String.Empty);
            else CreateDataTemplate(_locationDescription.EmptyHeader);
        }

        private void CreateDataTemplate(Section descriptionSection)
        {
            String markup = String.Empty;
            markup += "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:MyWorldIsComics\">";
            markup += "<ScrollViewer VerticalScrollBarVisibility=\"Hidden\">";
            markup += "<RichTextBlock>";

            if (descriptionSection == null) return;

            if (descriptionSection.Type == "p")
            {
                DescriptionParagraph para = new DescriptionParagraph { Text = descriptionSection.Title };
                markup += "<Paragraph FontSize=\"15\" FontFamily=\"Segoe UI Semilight\" Margin=\"0,0,0,10\">" + para.FormatLinks() + "</Paragraph>";
            }

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
            SetHubSectionContentTemplate(dataTemplate, descriptionSection.Title);
        }

        private static string MarkupSection(Section sectionToMarkup)
        {
            string markup = String.Empty;

            if (sectionToMarkup == null) return markup;

            markup += "<Paragraph Margin=\"0,0,0,10\" FontSize=\"17\">";
            switch (sectionToMarkup.Type)
            {
                case "h3":
                    markup += "<Bold>" + sectionToMarkup.Title + "</Bold>";
                    break;
                case "h4":
                    markup += "<Underline>" + sectionToMarkup.Title + "</Underline>";
                    break;
            }
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

        private void HideHubSection(string sectionTitle)
        {
            switch (sectionTitle)
            {
                case "Overview":
                    OverviewHubSection.Visibility = Visibility.Collapsed;
                    break;
                case "Other Media":
                    OtherMediaHubSection.Visibility = Visibility.Collapsed;
                    break;
                default:
                    DescriptionHubSection.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void SetHubSectionContentTemplate(DataTemplate sectionTemplate, string sectionTitle)
        {
            switch (sectionTitle)
            {
                case "Overview":
                    OverviewHubSection.ContentTemplate = sectionTemplate;
                    OverviewHubSection.Visibility = Visibility.Visible;
                    break;
                case "Other Media":
                    OtherMediaHubSection.ContentTemplate = sectionTemplate;
                    OtherMediaHubSection.Visibility = Visibility.Visible;
                    break;
                default:
                    DescriptionHubSection.ContentTemplate = sectionTemplate;
                    DescriptionHubSection.Visibility = Visibility.Visible;
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Listners

        private void VolumeView_VolumeClick(object sender, ItemClickEventArgs e)
        {
            var volume = ((Volume)e.ClickedItem);
            Frame.Navigate(typeof(VolumePage), volume.UniqueId);
        }

        private async void HubSection_HeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            if (e == null) return;
            if (e.Section.Header == null) return;
            if (e.Section.Header.ToString() != "Volumes" || e.Section.Header.ToString() != "First Appearance") await this.FormatDescriptionForPage();
            switch (e.Section.Header.ToString())
            {
                case "Volumes":
                    Frame.Navigate(typeof(VolumesPage), _location);
                    break;
                case "First Appearance":
                    IssuePage.BasicIssue = _location.FirstAppearanceIssue;
                    Frame.Navigate(typeof(IssuePage), _location.FirstAppearanceIssue);
                    break;
                case "Overview":
                    Frame.Navigate(typeof(DescriptionSectionPage), _locationDescription.Overview);
                    break;
                case "Other Media":
                    Frame.Navigate(typeof(DescriptionSectionPage), _locationDescription.OtherMedia);
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
            Frame.Navigate(typeof(VolumePage), _location.FirstAppearanceIssue.VolumeId);
        }

        #endregion
    }
}
