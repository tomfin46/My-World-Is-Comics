using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string Url { get; set; }
        public int Ns { get; set; }
    }


    class TrendingCharactersMapper
    {
        private WikiaResponse wR;

        public void DeserializeJsonContent(string json)
        {
            wR = JsonConvert.DeserializeObject<WikiaResponse>(json);
        }
        
        public string GetResponseTitle(int index)
        {
            if (wR != null && wR.Items != null && GetItemsLength() >= index && wR.Items.ElementAt(index) != null)
            {
                return wR.Items.ElementAt(index).Title;
            }
            return String.Empty;
        }

        public int GetItemsLength()
        {
            return wR.Items.Count;
        }
    }
}
