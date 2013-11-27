using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Enums;

namespace MyWorldIsComics.Mappers
{
    using MyWorldIsComics.DataModel.Resources;
    using System.IO;
    using System.Xml;

    class IssueMapper
    {
        private readonly Issue _issueToMap;

        public IssueMapper()
        {
            _issueToMap = new Issue();
        }
        public Issue QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseCoverDate(reader);
                ParseDescriptionString(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseIssueNumber(reader);
                ParseName(reader);
                ParseStoreDate(reader);
                ParseVolume(reader);
                _issueToMap.ResourceString = xmlString;
            }
            return _issueToMap;
        }

        public Issue MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing("results");
                ParseComicVineApiUrl(reader);
                ParseCharacters(reader);
                ParseConcepts(reader);
                ParseCoverDate(reader);
                ParseDeck(reader);
                ParseDescriptionString(reader);
                ParseStaffReview(reader);
                ParseId(reader);
                ParseImage(reader);
                ParseIssueNumber(reader);
                ParseLocations(reader);
                ParseName(reader);
                ParseObjects(reader);
                ParsePeople(reader);
                ParseComicVineSiteUrl(reader);
                ParseStoreDate(reader);
                ParseStoryArcs(reader);
                ParseTeams(reader);
                ParseVolume(reader);
            }
            return _issueToMap;
        }
        
        private void ParseComicVineApiUrl(XmlReader reader)
        {
            if (reader.Name != "api_detail_url") { reader.ReadToFollowing("api_detail_url"); }
            _issueToMap.ComicVineApiUrl = new Uri(reader.ReadElementContentAsString());
        }

        private void ParseCharacters(XmlReader reader)
        {
            if (reader.Name != "character_credits") { reader.ReadToFollowing("character_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.ConceptIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "character_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseConcepts(XmlReader reader)
        {
            if (reader.Name != "concept_credits") { reader.ReadToFollowing("concept_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "concept" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.ConceptIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "concept_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseCoverDate(XmlReader reader)
        {
            if (reader.Name != "cover_date") { reader.ReadToFollowing("cover_date"); }
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }
            var dateVals = date.Split('-');
            _issueToMap.CoverDate = new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2]));
        }

        private void ParseDeck(XmlReader reader)
        {
            if (reader.Name != "deck") { reader.ReadToFollowing("deck"); }
            _issueToMap.Deck = reader.ReadElementContentAsString();
        }

        private void ParseDescriptionString(XmlReader reader)
        {
            if (reader.Name != "description") { reader.ReadToFollowing("description"); }
            _issueToMap.DescriptionString = reader.ReadElementContentAsString();
        }

        private void ParseStaffReview(XmlReader reader)
        {
            if (reader.Name != "has_staff_review") { reader.ReadToFollowing("has_staff_review"); }
            if (reader.IsEmptyElement) return;

            reader.ReadToDescendant("id");
            _issueToMap.StaffReviewId = reader.ReadElementContentAsInt();
        }
        
        private void ParseId(XmlReader reader)
        {
            if (reader.Name != "id")
            {
                reader.ReadToFollowing("has_staff_review");
                reader.ReadToNextSibling("id");
            }
            _issueToMap.UniqueId = reader.ReadElementContentAsInt();
        }

        private void ParseImage(XmlReader reader)
        {
            if (reader.Name != "image") { reader.ReadToFollowing("image"); }
            if (reader.IsEmptyElement) return;

            reader.ReadToFollowing("super_url");
            _issueToMap.MainImage = new Uri(reader.ReadElementContentAsString());
        }

        private void ParseIssueNumber(XmlReader reader)
        {
            if (reader.Name != "issue_number") { reader.ReadToFollowing("issue_number"); }
            _issueToMap.IssueNumber = reader.ReadElementContentAsInt();
        }

        private void ParseLocations(XmlReader reader)
        {
            if (reader.Name != "location_credits") { reader.ReadToFollowing("location_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "location" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.LocationIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "location_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseName(XmlReader reader)
        {
            if (reader.Name != "name" && reader.Name != "location_credits")
            {
                reader.ReadToFollowing("location_credits");
                reader.ReadToNextSibling("name");
            }
            else
            {
                reader.ReadToNextSibling("name");
            }

            _issueToMap.Name = reader.ReadElementContentAsString();
        }

        private void ParseObjects(XmlReader reader)
        {
            if (reader.Name != "object_credits") { reader.ReadToFollowing("object_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "object" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.ObjectIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "object_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParsePeople(XmlReader reader)
        {
            if (reader.Name != "person_credits") { reader.ReadToFollowing("person_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "person" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    var id = reader.ReadElementContentAsInt();
                    reader.ReadToNextSibling("role");
                    var role = reader.ReadElementContentAsString();
                    _issueToMap.PersonIds.Add(id, role);
                }
                else if (reader.Name == "person_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseComicVineSiteUrl(XmlReader reader)
        {
            if (reader.Name != "site_detail_url") { reader.ReadToFollowing("site_detail_url"); }
            _issueToMap.ComicVineSiteUrl = new Uri(reader.ReadElementContentAsString());
        }

        private void ParseStoreDate(XmlReader reader)
        {
            if (reader.Name != "store_date") { reader.ReadToFollowing("store_date"); }
            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }
            var dateVals = date.Split('-');
            _issueToMap.StoreDate = new DateTime(int.Parse(dateVals[0]), int.Parse(dateVals[1]), int.Parse(dateVals[2]));
        }

        private void ParseStoryArcs(XmlReader reader)
        {
            if (reader.Name != "story_arc_credits") { reader.ReadToFollowing("story_arc_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "story_arc" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.StoryArcIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "story_arc_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseTeams(XmlReader reader)
        {
            if (reader.Name != "team_credits") { reader.ReadToFollowing("team_credits"); }
            if (reader.IsEmptyElement) { return; }
            while (reader.Read())
            {
                if (reader.Name == "team" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _issueToMap.TeamIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "team_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseVolume(XmlReader reader)
        {
            if (reader.Name != "volume") { reader.ReadToFollowing("volume"); }

            reader.ReadToFollowing("id");
            _issueToMap.VolumeId = reader.ReadElementContentAsInt();
            _issueToMap.VolumeName = reader.ReadElementContentAsString();
        }
    }
}
