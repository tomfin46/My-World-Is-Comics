using System;
using System.Collections.ObjectModel;
using MyWorldIsComics.DataSource;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class Publisher : ResponseSchema
    {
        public ObservableCollection<Character> Characters { get; set; }
        public string Location_Address { get; set; }
        public string Location_City { get; set; }
        public string Location_State { get; set; }
        public ObservableCollection<StoryArc> Story_Arcs { get; set; }
        public ObservableCollection<Team> Teams { get; set; }
        public ObservableCollection<Volume> Volumes { get; set; }

        public string InAppUrl
        {
            get
            {
                var resId = Enums.Resources.GetResourceId(Enums.Resources.ResourcesEnum.Publisher);
                resId = resId.Replace("-", String.Empty);
                return ServiceConstants.AppUri + ":///" + resId + "/" + Id;
            }
        }

        public Publisher()
        {
            Characters = new ObservableCollection<Character>();
            Story_Arcs = new ObservableCollection<StoryArc>();
            Teams = new ObservableCollection<Team>();
            Volumes = new ObservableCollection<Volume>();
        }
    }
}
