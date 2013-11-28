namespace MyWorldIsComics.DataModel.DescriptionContent
{
    class Link
    {
        public string DataRefId { get; set; }
        public string Href { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
