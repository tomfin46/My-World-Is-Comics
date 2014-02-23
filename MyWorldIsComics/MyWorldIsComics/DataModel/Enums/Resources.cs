using System;

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
                case ResourcesEnum.Characters:
                    return "characters";
                case ResourcesEnum.Search:
                    return "search";
                case ResourcesEnum.Team:
                    return "team";
                case ResourcesEnum.Teams:
                    return "teams";
                case ResourcesEnum.Issue:
                    return "issue";
                case ResourcesEnum.Issues:
                    return "issues";
                case ResourcesEnum.Person:
                    return "person";
                case ResourcesEnum.Location:
                    return "location";
                case ResourcesEnum.Concept:
                    return "concept";
                case ResourcesEnum.Object:
                    return "object";
                case ResourcesEnum.StoryArc:
                    return "story_arc";
                case ResourcesEnum.Volume:
                    return "volume";
                case ResourcesEnum.Volumes:
                    return "volumes";
                case ResourcesEnum.Publisher:
                    return "publisher";
                case ResourcesEnum.Movie:
                    return "movie";
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
                case ResourcesEnum.Person:
                    return "4040-";
                case ResourcesEnum.Location:
                    return "4020-";
                case ResourcesEnum.Concept:
                    return "4015-";
                case ResourcesEnum.Object:
                    return "4055-";
                case ResourcesEnum.StoryArc:
                    return "4045-";
                case ResourcesEnum.Volume:
                    return "4050-";
                case ResourcesEnum.Publisher:
                    return "4010-";
                case ResourcesEnum.Movie:
                    return "4025-";
                default:
                    return String.Empty;
            }
        }
    }
}
