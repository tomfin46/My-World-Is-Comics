using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using MyWorldIsComics.Pages;
using MyWorldIsComics.Pages.ResourcePages;

namespace MyWorldIsComics.Helpers
{
    class ProtocolNavigation
    {
        private ProtocolActivatedEventArgs _protocolEventArgs = null;

        private Frame rootFrame;

        public ProtocolActivatedEventArgs ProtocolEvent
        {
            get { return _protocolEventArgs; }
            set { _protocolEventArgs = value; }
        }

        public ProtocolNavigation(Frame rootFrame, ProtocolActivatedEventArgs protocolArgs)
        {
            this.rootFrame = rootFrame;
            ProtocolEvent = protocolArgs;
        }

        public void NavigateToProtocolPage()
        {
            var pageType = this.rootFrame.CurrentSourcePageType.Name;
            switch (pageType)
            {
                case "CharacterPage":
                    var characterPage = rootFrame.Content as CharacterPage;
                    if (characterPage != null) NavigateToProtocolPage(characterPage.Frame);
                    break;
                case "IssuePage":
                    var issuePage = rootFrame.Content as IssuePage;
                    if (issuePage != null) NavigateToProtocolPage(issuePage.Frame);
                    break;
                case "TeamPage":
                    var teamPage = rootFrame.Content as TeamPage;
                    if (teamPage != null) NavigateToProtocolPage(teamPage.Frame);
                    break;
                case "DescriptionSectionPage":
                    var descriptionSection = rootFrame.Content as DescriptionSectionPage;
                    if (descriptionSection != null) NavigateToProtocolPage(descriptionSection.Frame);
                    break;
                case "VolumePage":
                    var volumePage = rootFrame.Content as VolumePage;
                    if (volumePage != null) NavigateToProtocolPage(volumePage.Frame);
                    break;
                case "PublisherPage":
                    var publisherPage = rootFrame.Content as PublisherPage;
                    if (publisherPage != null) NavigateToProtocolPage(publisherPage.Frame);
                    break;
            }
        }
        private void NavigateToProtocolPage(Frame pageFrame)
        {

            switch (_protocolEventArgs.Uri.Segments[1].Substring(0, 4))
            {
                case "4005":
                    pageFrame.Navigate(typeof(CharacterPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4060":
                    pageFrame.Navigate(typeof(TeamPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4000":
                    pageFrame.Navigate(typeof(IssuePage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4040":
                    // TODO pageFrame.Navigate(typeof(PersonPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4020":
                    // TODO pageFrame.Navigate(typeof(LocationPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4015":
                    // TODO pageFrame.Navigate(typeof(ConceptPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4055":
                    // TODO pageFrame.Navigate(typeof(ObjectPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4045":
                    // TODO pageFrame.Navigate(typeof(StoryArcPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4050":
                     pageFrame.Navigate(typeof(VolumePage), _protocolEventArgs.Uri.Segments[2]);
                    break;
                case "4010":
                    pageFrame.Navigate(typeof(PublisherPage), _protocolEventArgs.Uri.Segments[2]);
                    break;
            }
        }
    }
}
