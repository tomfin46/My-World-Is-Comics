using System;
using System.Xml;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.Mappers
{
    internal static class GenericResourceMapper
    {
        public static IResource ParseDeck(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "deck")
            {
                reader.ReadToFollowing("deck");
            }
            resourceToMap.Deck = reader.ReadElementContentAsString();

            return resourceToMap;
        }

        public static IResource ParseId(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "id")
            {
                reader.ReadToFollowing("id");
            }
            resourceToMap.UniqueId = reader.ReadElementContentAsInt();

            return resourceToMap;
        }

        public static IResource ParseImage(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "image")
            {
                reader.ReadToFollowing("image");
            }
            if (reader.IsEmptyElement) return resourceToMap;

            reader.ReadToFollowing("super_url");
            resourceToMap.MainImage = new Uri(reader.ReadElementContentAsString());

            return resourceToMap;
        }

        public static IResource ParseName(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "name")
            {
                reader.ReadToFollowing("name");
            }
            resourceToMap.Name = reader.ReadElementContentAsString();

            return resourceToMap;
        }

        public static IResource ParseComicVineSiteUrl(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "site_detail_url")
            {
                reader.ReadToFollowing("site_detail_url");
            }
            resourceToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());

            return resourceToMap;
        }

        public static IResource ParseComicVineApiUrl(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "api_detail_url")
            {
                reader.ReadToFollowing("api_detail_url");
            }
            resourceToMap.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());

            return resourceToMap;
        }

        public static IResource ParseDescriptionString(XmlReader reader, IResource resourceToMap)
        {
            if (reader.Name != "description")
            {
                reader.ReadToFollowing("description");
            }
            resourceToMap.DescriptionString = reader.ReadElementContentAsString();

            return resourceToMap;
        }

        public static bool EnsureResultsExist(XmlReader reader)
        {
            reader.ReadToFollowing("number_of_total_results");
            int noOfResults = reader.ReadElementContentAsInt();
            return noOfResults > 0;
        }
    }
}