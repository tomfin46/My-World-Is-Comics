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
        private Issue _issueToMap;

        public IssueMapper()
        {
            _issueToMap = new Issue();
        }

        public Issue QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return _issueToMap;

                reader.ReadToFollowing("results");
                ParseCoverDate(reader);
                _issueToMap = GenericResourceMapper.ParseDescriptionString(reader, _issueToMap) as Issue;
                _issueToMap = GenericResourceMapper.ParseId(reader, _issueToMap) as Issue;
                _issueToMap = GenericResourceMapper.ParseImage(reader, _issueToMap) as Issue;
                ParseIssueNumber(reader);
                _issueToMap = GenericResourceMapper.ParseName(reader, _issueToMap) as Issue;
                ParseStoreDate(reader);
                ParseVolume(reader);
                _issueToMap.ResourceString = xmlString;
            }
            return _issueToMap;
        }

        public Issue MapFilteredXmlObject(Issue basicIssue, string filteredIssueString, string filter)
        {
            using (XmlReader readerInit = XmlReader.Create(new StringReader(filteredIssueString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(readerInit)) return _issueToMap;

                _issueToMap = basicIssue;
                switch (filter)
                {
                    case "person_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParsePeople(reader);
                        }
                        break;
                    case "character_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseCharacters(reader);
                        }
                        break;
                    case "team_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseTeams(reader);
                        }
                        break;
                    case "location_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseLocations(reader);
                        }
                        break;
                    case "concept_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseConcepts(reader);
                        }
                        break;
                    case "object_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseObjects(reader);
                        }
                        break;
                    case "story_arc_credits":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredIssueString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseStoryArcs(reader);
                        }
                        break;
                }
            }
            return _issueToMap;
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
                    _issueToMap.CharacterIds.Add(reader.ReadElementContentAsInt());
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

        private void ParseStaffReview(XmlReader reader)
        {
            if (reader.Name != "has_staff_review") { reader.ReadToFollowing("has_staff_review"); }
            if (reader.IsEmptyElement) return;

            reader.ReadToDescendant("id");
            _issueToMap.StaffReviewId = reader.ReadElementContentAsInt();
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
            else if (reader.Name != "name")
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
                    if (_issueToMap.PersonIds.Any(p => p.Key == id)) continue;
                    _issueToMap.PersonIds.Add(id, role);
                }
                else if (reader.Name == "person_credits" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
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
