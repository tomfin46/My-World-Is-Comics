namespace MyWorldIsComics.DataModel.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;

    using Interfaces;

    using MyWorldIsComics.DataModel.Enums;

    public class Movie : IResource
    {
        #region IResource Fields

        public int UniqueId { get; set; }
        public string Name { get; set; }
        public Uri ComicVineApiUrl { get; set; }
        public Uri ComicVineSiteUrl { get; set; }
        public string Deck { get; set; }
        public string DescriptionString { get; set; }
        public Uri MainImage { get; set; }
        public Uri AvatarImage
        {
            get
            {
                Uri uri = default(Uri);
                if (MainImage != null)
                {
                    uri =
                        new Uri(
                            MainImage.AbsoluteUri.Replace(
                                ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.ScaleLarge),
                                ImageTypes.GetImageType(ImageTypes.ImageTypesEnum.SquareAvatar)));
                }

                return uri;
            }
        }

        public string PublisherName { get; set; }

        #endregion

        public string Rating { get; set; }
        public int BoxOfficeRevenue { get; set; }
        public string BoxOfficeRevenueString
        {
            get
            {
                return BoxOfficeRevenue == 0 ? "N/A" : BoxOfficeRevenue.ToString("C0", new CultureInfo("en-US"));
            }
        }
        public int TotalRevenue { get; set; }
        public string TotalRevenueString
        {
            get
            {
                return TotalRevenue == 0 ? "N/A" : TotalRevenue.ToString("C0", new CultureInfo("en-US"));
            }
        }
        public int Budget { get; set; }
        public string BudgetString
        {
            get
            {
                return Budget == 0 ? "N/A" : Budget.ToString("C0", new CultureInfo("en-US"));
            }
        }
        public string Distributor { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseDateString
        {
            get
            {
                return ReleaseDate == default(DateTime) ? "Unknown" : ReleaseDate.ToString("d MMM yyyy");
            }
        }
        public int Runtime { get; set; }

        #region Collections

        public List<int> CharacterIds { get; set; }
        public ObservableCollection<Character> Characters { get; set; }
        public List<int> ConceptIds { get; set; }
        public ObservableCollection<Concept> Concepts { get; set; }
        public List<int> LocationIds { get; set; }
        public ObservableCollection<Location> Locations { get; set; }
        public List<int> TeamIds { get; set; }
        public ObservableCollection<Team> Teams { get; set; }
        public List<int> WriterIds { get; set; }
        public ObservableCollection<Creator> Writers { get; set; } 

        #endregion

        public Movie()
        {
            CharacterIds = new List<int>();
            Characters = new ObservableCollection<Character>();
            ConceptIds = new List<int>();
            Concepts = new ObservableCollection<Concept>();
            LocationIds = new List<int>();
            Locations = new ObservableCollection<Location>();
            TeamIds = new List<int>();
            Teams = new ObservableCollection<Team>();
            WriterIds = new List<int>();
            Writers = new ObservableCollection<Creator>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
