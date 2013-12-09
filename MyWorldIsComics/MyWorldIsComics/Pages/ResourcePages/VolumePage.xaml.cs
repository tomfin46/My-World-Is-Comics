using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229
using MyWorldIsComics.DataModel.Resources;
using MyWorldIsComics.DataSource;
using MyWorldIsComics.Helpers;
using MyWorldIsComics.Mappers;

namespace MyWorldIsComics.Pages.ResourcePages
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class VolumePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary volumePageViewModel = new ObservableDictionary();

        private Volume _volume;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary VolumePageViewModel
        {
            get { return this.volumePageViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public VolumePage()
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
                await LoadVolume(id);
            }
            catch (HttpRequestException)
            {
                _volume = new Volume { Name = "An internet connection is required here" };
                VolumePageViewModel["Volume"] = _volume;
            }
        }

        private async Task LoadVolume(int id)
        {
            try
            {
                if (SavedData.Volume != null && SavedData.Volume.UniqueId == id) { _volume = SavedData.Volume; }
                else
                {
                    _volume = await GetVolume(id);
                    _volume.IssueIds.Reverse();
                }

                VolumePageViewModel["Volume"] = _volume;
                VolumePageViewModel["Issues"] = _volume.Issues;

                await LoadDescription();

                if (_volume.Issues.Count == 0) { await FetchFirstIssue(); }
                if (_volume.IssueIds.Count > 1) await FetchRemainingIssues();
            }
            catch (TaskCanceledException)
            {
                ComicVineSource.ReinstateCts();
            }
        }

        private async Task LoadDescription()
        {
            await FormatDescriptionForPage();
            CreateDataTemplate();
        }

        private async Task<Volume> GetVolume(int id)
        {
            List<string> filters = new List<string> { "count_of_issues", "description", "first_issue", "id", "image", "issues", "name", "publisher", "start_year" };
            var volumeSearchString = await ComicVineSource.GetFilteredVolumeAsync(id, filters);
            return MapVolume(volumeSearchString);
        }

        private async Task FetchFirstIssue()
        {
            Issue issue = MapQuickIssue(await ComicVineSource.GetQuickIssueAsync(_volume.IssueIds.First()));
            if (_volume.Issues.Any(i => i.UniqueId == issue.UniqueId)) return;
            _volume.Issues.Add(issue);

        }

        private async Task FetchRemainingIssues()
        {
            var firstId = _volume.IssueIds.First();
            foreach (int issueId in _volume.IssueIds.Where(id => id != firstId).Take(_volume.IssueIds.Count - 1))
            {
                Issue issue = MapQuickIssue(await ComicVineSource.GetQuickIssueAsync(issueId));
                if (_volume.Issues.Any(i => i.UniqueId == issue.UniqueId)) continue;
                _volume.Issues.Add(issue);

                if (_volume.Issues.Count%30 != 0) continue;
                _volume.Issues = new ObservableCollection<Issue>(_volume.Issues.OrderByDescending(i => i.CoverDate));
                VolumePageViewModel["Issues"] = _volume.Issues;
            }
            _volume.Issues = new ObservableCollection<Issue>(_volume.Issues.OrderByDescending(i => i.CoverDate));
            VolumePageViewModel["Issues"] = _volume.Issues;
        }

        private Volume MapVolume(string volumeString)
        {
            return volumeString == ServiceConstants.QueryNotFound ? new Volume { Name = ServiceConstants.QueryNotFound } : new VolumeMapper().MapXmlObject(volumeString);
        }

        private Issue MapQuickIssue(string issueString)
        {
            return issueString == ServiceConstants.QueryNotFound ? new Issue { Name = "Issue Not Found" } : new IssueMapper().QuickMapXmlObject(issueString);
        }

        private async Task FormatDescriptionForPage()
        {
            _volume.Description = await Task.Run(() => new DescriptionMapper().MapDescription(_volume));
        }

        private void CreateDataTemplate()
        {

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
    }
}
