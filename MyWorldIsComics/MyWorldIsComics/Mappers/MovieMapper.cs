using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.Mappers
{
    using System.IO;
    using System.Xml;

    using MyWorldIsComics.DataModel.Enums;
    using MyWorldIsComics.DataModel.Resources;
    using MyWorldIsComics.DataSource;

    class MovieMapper
    {
        private Movie _movieToMap;

        public MovieMapper()
        {
            this._movieToMap = new Movie();
        }

        public Movie QuickMapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return this._movieToMap;
                if (reader.Name == "response") reader.ReadToFollowing("results");

                this._movieToMap = GenericResourceMapper.ParseId(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseImage(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseName(reader, this._movieToMap) as Movie;
            }
            return this._movieToMap;
        }

        public Movie MapXmlObject(string xmlString)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(reader)) return new Movie { Name = ServiceConstants.QueryNotFound };

                reader.ReadToFollowing("results");
                ParseBoxOfficeRevenue(reader);
                ParseBudget(reader);
                this._movieToMap = GenericResourceMapper.ParseDeck(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseDescriptionString(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseId(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseImage(reader, this._movieToMap) as Movie;
                this._movieToMap = GenericResourceMapper.ParseName(reader, this._movieToMap) as Movie;
                ParseRating(reader);
                ParseReleaseDate(reader);
                ParseRuntime(reader);
                ParseTotalRevenue(reader);
            }
            return this._movieToMap;
        }

        public Movie MapFilteredXmlObject(Movie basicMovie, string filteredTeamString, string filter)
        {
            using (XmlReader readerInit = XmlReader.Create(new StringReader(filteredTeamString)))
            {
                if (!GenericResourceMapper.EnsureResultsExist(readerInit)) return _movieToMap;
                _movieToMap = basicMovie;
                switch (filter)
                {
                    case "characters":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseCharacters(reader);
                        }
                        break;
                    case "concepts":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseConcepts(reader);
                        }
                        break;
                    case "locations":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseLocations(reader);
                        }
                        break;
                    case "teams":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseTeams(reader);
                        }
                        break;
                    case "writers":
                        using (XmlReader reader = XmlReader.Create(new StringReader(filteredTeamString)))
                        {
                            reader.ReadToFollowing("results");
                            this.ParseWriters(reader);
                        }
                        break;
                }
            }
            return _movieToMap;
        }

        #region Filters

        private void ParseCharacters(XmlReader reader)
        {
            if (reader.Name != "characters") { reader.ReadToFollowing("characters"); }
            if (reader.Name == "characters" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "character" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _movieToMap.CharacterIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "characters" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseConcepts(XmlReader reader)
        {
            if (reader.Name != "concepts") { reader.ReadToFollowing("concepts"); }
            if (reader.Name == "concepts" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "concept" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _movieToMap.ConceptIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "concepts" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseLocations(XmlReader reader)
        {
            if (reader.Name != "locations") { reader.ReadToFollowing("locations"); }
            if (reader.Name == "locations" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "location" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _movieToMap.LocationIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "locations" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseTeams(XmlReader reader)
        {
            if (reader.Name != "teams") { reader.ReadToFollowing("teams"); }
            if (reader.Name == "teams" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "team" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadToDescendant("id");
                    _movieToMap.TeamIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "teams" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void ParseWriters(XmlReader reader)
        {
            if (reader.Name != "writers") { reader.ReadToFollowing("writers"); }
            if (reader.Name == "writers" && reader.IsEmptyElement) return;

            while (reader.Read())
            {
                if (reader.Name == "id" && reader.NodeType != XmlNodeType.EndElement)
                {
                    _movieToMap.WriterIds.Add(reader.ReadElementContentAsInt());
                }
                else if (reader.Name == "writers" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }
            }
        } 

        #endregion

        private void ParseBoxOfficeRevenue(XmlReader reader)
        {
            if (reader.Name != "box_office_revenue") { reader.ReadToFollowing("box_office_revenue"); }
            _movieToMap.BoxOfficeRevenue = reader.ReadElementContentAsInt();
        }

        private void ParseBudget(XmlReader reader)
        {
            if (reader.Name != "budget") { reader.ReadToFollowing("budget"); }
            _movieToMap.Budget = reader.ReadElementContentAsInt();
        }

        private void ParseRating(XmlReader reader)
        {
            if (reader.Name != "rating") { reader.ReadToFollowing("rating"); }
            _movieToMap.Rating = reader.ReadElementContentAsString();
        }

        private void ParseReleaseDate(XmlReader reader)
        {
            if (reader.Name != "release_date") { reader.ReadToFollowing("release_date"); }

            var date = reader.ReadElementContentAsString();

            if (date == String.Empty) { return; }

            var arr = date.Split('-');
            var year = int.Parse(arr[0]);
            var month = int.Parse(arr[1]);
            var day = int.Parse(arr[2].Substring(0, 2));

            _movieToMap.ReleaseDate = new DateTime(year, month, day);
        }

        private void ParseRuntime(XmlReader reader)
        {
            if (reader.Name != "runtime") { reader.ReadToFollowing("runtime"); }
            _movieToMap.Runtime = reader.ReadElementContentAsInt();
        }

        private void ParseTotalRevenue(XmlReader reader)
        {
            if (reader.Name != "total_revenue") { reader.ReadToFollowing("total_revenue"); }
            _movieToMap.TotalRevenue = reader.ReadElementContentAsInt();
        }
    }
}
