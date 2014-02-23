using System;
using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataSource;
using Newtonsoft.Json;

namespace MyWorldIsComics.Mappers
{
    class WikiaResponse
    {
        public IList<WikiaResponseItem> Items { get; set; }
        public string BasePath { get; set; }
    }

    class WikiaResponseItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
    }


    class TrendingCharactersMapper
    {
        private WikiaResponse wR;

        public void DeserializeJsonContent(string json)
        {
            wR = JsonDeserialize.DeserializeJsonString<WikiaResponse>(json);
        }
        
        public string GetResponseTitle(int index)
        {
            if (wR != null && wR.Items != null && GetItemsLength() >= index && wR.Items.ElementAt(index) != null)
            {
                return wR.Items.ElementAt(index).Title;
            }
            return String.Empty;
        }

        public string ExtractCurrentAlias(int index)
        {
            string currentAlias = ServiceConstants.AliasNotFound;
            var item = wR.Items.ElementAt(index);
            var startPos = item.Abstract.IndexOf("Current Alias", System.StringComparison.Ordinal);
            var endPos = item.Abstract.IndexOf("Aliases", System.StringComparison.Ordinal);
            var length = endPos - (startPos + 15);

            if (startPos != -1 && endPos != -1)
            {
                currentAlias = item.Abstract.Substring(startPos + 14, length);
            }
            return currentAlias;
        }

        public int GetItemsLength()
        {
            return wR.Items.Count;
        }
    }
}
