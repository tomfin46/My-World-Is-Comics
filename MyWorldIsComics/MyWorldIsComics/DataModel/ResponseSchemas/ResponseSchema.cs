using System;
using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    public class ResponseSchema : IResponse
    {
        public string Aliases { get; set; }
        public string AliasesOneLine
        {
            get
            {
                if (Aliases == null) return "None";

                var a = Aliases.Split('\n').ToList();
                return a.Count == 1 && a.First() == String.Empty ? "None" : string.Join(", ", a).Replace("\r", String.Empty);
            }
        }
        public string Api_Detail_Url { get; set; }
        public string Deck { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public Images Image { get; set; }
        public string Name { get; set; }
        public Publisher Publisher { get; set; }
        public string Resource_Type { get; set; }
        public string Site_Detail_Url { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class SearchSchema : ResponseSchema
    {
        public string Issue_Number { get; set; }
        public string IssueNumberFormattedString
        {
            get
            {
                return Issue_Number != null ? "#" + Issue_Number : String.Empty;
            }
        }
        public Volume Volume { get; set; }
    }
    public class Images
    {
        public string Icon_Url { get; set; }
        public string Medium_Url { get; set; }
        public string Screen_Url { get; set; }
        public string Small_Url { get; set; }
        public string Super_Url { get; set; }
        public string Thumb_Url { get; set; }
        public string Tiny_Url { get; set; }

    }

    #region JsonSingularBase

    public class JsonSingularBaseCharacter
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Character Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseTeam
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Team Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseVolume
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Volume Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseIssue
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Issue Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBasePerson
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Person Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseLocation
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Location Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseConcept
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Concept Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseObjectResource
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public ObjectResource Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBaseStoryArc
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public StoryArc Results { get; set; }
        public string Version { get; set; }
    }
    
    public class JsonSingularBaseMovie
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Movie Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonSingularBasePublisher
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public Publisher Results { get; set; }
        public string Version { get; set; }
    }

    #endregion

    #region JsonMultipleBase

    public class JsonMultipleBase
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public List<SearchSchema> Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonMultipleBaseCharacter
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public List<Character> Results { get; set; }
        public string Version { get; set; }
    }

    public class JsonMultipleBaseTeam
    {
        public string Error { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Number_Of_Page_Results { get; set; }
        public int Number_Of_Total_Results { get; set; }
        public int Status_Code { get; set; }
        public List<Team> Results { get; set; }
        public string Version { get; set; }
    }

    #endregion
}
