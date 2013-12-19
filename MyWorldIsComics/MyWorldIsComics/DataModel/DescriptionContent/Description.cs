namespace MyWorldIsComics.DataModel.DescriptionContent
{
    using System.Collections.ObjectModel;

    internal class Description
    {
        public int UniqueId { get; set; }
        public ObservableCollection<Section> Sections { get; set; }
        public Section EmptyHeader { get; set; }

        public Description()
        {
            Sections = new ObservableCollection<Section>();
        }
    }
}