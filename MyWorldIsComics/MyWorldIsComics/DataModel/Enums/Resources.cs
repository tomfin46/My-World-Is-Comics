using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.Enums
{
    class Resources
    {
        public enum ResourcesEnum
        {
            Character,

            Characters,

            Chat,

            Chats,

            Concept,

            Concepts,

            Episode,

            Episodes,

            Issue,

            Issues,

            Location,

            Locations,

            Movie,

            Movies,

            Object,

            Objects,

            Origin,

            Origins,

            Person,

            People,

            Power,

            Powers,

            Promo,

            Promos,

            Publisher,

            Publishers,

            Series,

            SeriesList,

            Search,

            StoryArc,

            StoryArcs,

            Team,

            Teams,

            Types,

            Video,

            Videos,

            VideoType,

            VideoTypes,

            Volume,

            Volumes
        }

        public static string GetResourceTerm(ResourcesEnum resourcesEnum)
        {
            switch (resourcesEnum)
            {
                case ResourcesEnum.Character:
                    return "character";
                case ResourcesEnum.Search:
                    return "search";
                case ResourcesEnum.Team:
                    return "team";
                case ResourcesEnum.Issue:
                    return "issue";
                default:
                    return String.Empty;
            }
        }

        public static string GetResourceId(ResourcesEnum resourcesEnum)
        {
            switch (resourcesEnum)
            {
                case ResourcesEnum.Search:
                    return "?query=";
                case ResourcesEnum.Character:
                    return "4005-";
                case ResourcesEnum.Team:
                    return "4060-";
                case ResourcesEnum.Issue:
                    return "4000-";
                default:
                    return String.Empty;
            }
        }
    }
}
