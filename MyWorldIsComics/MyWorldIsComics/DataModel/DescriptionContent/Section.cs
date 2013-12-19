using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.DataModel.DescriptionContent
{
    public class Section : IDescriptionContent
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Uri ImageSource { get; set; }
        public Queue<IDescriptionContent> ContentQueue { get; set; }
        public List<Link> Links { get; set; }
        public string Type { get; set; }

        public Section()
        {
            ContentQueue = new Queue<IDescriptionContent>();
            Links = new List<Link>();
        }

        public override string ToString()
        {
            return Title;
        }

        public string ToPlainString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Title);
            foreach (Section section in ContentQueue)
            {
                sb.Append(section.Title);
                foreach (IDescriptionContent content in section.ContentQueue)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(content.Title);
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
