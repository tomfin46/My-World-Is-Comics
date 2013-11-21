using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.Mappers
{
    using System.Xml.Serialization;

    using Windows.Data.Json;

    using MyWorldIsComics.DataModel;

    using Newtonsoft.Json;

    class CharacterMapper
    {
        //private static IJsonValue parser;

        public static void MapJsonObject(string jsonString, out Character character)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Character));
            //character = serializer.Deserialize(jsonString);
            character = JsonConvert.DeserializeObject<Character>(jsonString);
            var obj = JsonConvert.DeserializeObjectAsync(jsonString);
        }

        private static string ParseName()
        {
            return "";
            //return parser["name"].GetString();
        }

        private static List<string> ParseAliases()
        {
            return new List<string>();
            //string aliases = parser["aliases"].GetString();
            //return aliases.Split('\n').ToList();
        }
    }
}
