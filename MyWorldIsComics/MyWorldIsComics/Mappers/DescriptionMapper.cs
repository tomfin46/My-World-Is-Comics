using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataModel.DescriptionContent;

namespace MyWorldIsComics.Mappers
{
    #region usings

    using System;

    using HtmlAgilityPack;

    using MyWorldIsComics.DataModel.Resources;

    #endregion

    class DescriptionMapper
    {
        private readonly Description descriptionToMap;

        public DescriptionMapper()
        {
            this.descriptionToMap = new Description();
        }

        public Description MapDescription(string htmlString)
        {
            if(htmlString == null) return new Description();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;
            foreach (HtmlNode link in collection.Where(link => link.Name == "h2"))
            {
                this.descriptionToMap.Sections.Add(this.ProcessSection(link));
            }

            if (collection.Count > 0 && collection.First().Name == "p")
            {
                this.descriptionToMap.EmptyHeader = this.ProcessSection(collection.First());
            }
            return this.descriptionToMap;
        }

        public Section MapDescription(Issue issue)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(issue.DescriptionString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;

            HtmlNode link = collection.First();
            return ProcessSection(link);
        }

        public Section MapDescription(Volume volume)
        {
            HtmlDocument document = new HtmlDocument();
            Section sectionToMap = new Section();
            document.LoadHtml(volume.DescriptionString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;
            if (collection.Count == 1)
            {
                sectionToMap = this.ProcessSection(collection.First());
            }
            else
            {
                foreach (HtmlNode link in collection)
                {
                    switch (link.Name)
                    {
                        case "p":
                            if (link.FirstChild.Name == "b")
                            {
                                sectionToMap.Title = link.InnerText;
                                sectionToMap.Type = "h4";
                            }
                            else
                            {
                                sectionToMap.ContentQueue.Enqueue(ProcessSection(link));
                            }
                            break;
                    }
                }
            }

            return sectionToMap;
        }

        private Section ProcessSection(HtmlNode link)
        {
            Section section = new Section
            {
                Title = link.InnerText,
                Type = link.Name
            };
            var nextSibling = link.NextSibling;

            while (nextSibling != null && nextSibling.Name != link.Name)
            {
                switch (nextSibling.Name)
                {
                    case "p":
                        section.ContentQueue.Enqueue(ProcessParagraph(nextSibling));
                        break;
                    case "a":
                        section.ContentQueue.Enqueue(ProcessLinkBeginningParagraph(nextSibling, out nextSibling));
                        break;
                    case "figure":
                        section.ContentQueue.Enqueue(ProcessFigure(nextSibling));
                        break;
                    case "ul":
                        section.ContentQueue.Enqueue(ProcessList(nextSibling));
                        break;
                    case "blockquote":
                        section.ContentQueue.Enqueue(ProcessQuote(nextSibling));
                        break;
                    case "h3":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && nextSibling.Name != "h3" && nextSibling.Name != "h2")
                        {
                            nextSibling = nextSibling.NextSibling;
                        }

                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && nextSibling.Name != "h4" && nextSibling.Name != "h3" && nextSibling.Name != "h2")
                        {
                            nextSibling = nextSibling.NextSibling;
                        }
                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                }

                if (nextSibling != null) nextSibling = nextSibling.NextSibling;
            }

            return section;
        }

        private DescriptionParagraph ProcessParagraph(HtmlNode paragraphNode)
        {
            DescriptionParagraph paragraph = new DescriptionParagraph
            {
                Text = paragraphNode.InnerText,
                Links = new List<Link>()
            };

            if (!paragraphNode.HasChildNodes) return paragraph;

            var nodes = paragraphNode.ChildNodes;
            foreach (HtmlNode htmlNode in nodes.Where(htmlNode => htmlNode.Name == "a")
                .Where(htmlNode => htmlNode.GetAttributeValue("rel", String.Empty) != "nofollow"))
            {
                if (paragraph.Links.Any(link => link.Text == htmlNode.InnerText)) { continue; }

                if (paragraph.Links.All(link => link.Href != htmlNode.GetAttributeValue("href", String.Empty)))
                {
                    paragraph.Links.Add(new Link
                                        {
                                            Href = htmlNode.GetAttributeValue("href", String.Empty),
                                            DataRefId = htmlNode.GetAttributeValue("data-ref-id", String.Empty),
                                            Text = htmlNode.InnerText
                                        });
                }
            }

            return paragraph;
        }

        private DescriptionParagraph ProcessLinkBeginningParagraph(HtmlNode paragraphNode, out HtmlNode nextSibling)
        {
            nextSibling = paragraphNode.NextSibling;
            
            if (!paragraphNode.HasChildNodes) return new DescriptionParagraph {Text = paragraphNode.InnerText};
            DescriptionParagraph paragraph = new DescriptionParagraph
            {
                Text = paragraphNode.FirstChild.InnerText,
                Links = new List<Link> { 
                    new Link { 
                        Href = paragraphNode.GetAttributeValue("href", String.Empty),
                        DataRefId = paragraphNode.GetAttributeValue("data-ref-id", String.Empty),
                        Text = paragraphNode.InnerText
                    }
                }
            };
            
            do
            {
                switch (nextSibling.Name)
                {
                    case "#text":
                        paragraph.Text += nextSibling.InnerText;
                        break;
                    case "a":
                        paragraph.Text += nextSibling.InnerText;
                        paragraph.Links.Add(
                            new Link
                            {
                                Href = nextSibling.GetAttributeValue("href", String.Empty),
                                DataRefId = nextSibling.GetAttributeValue("data-ref-id", String.Empty),
                                Text = nextSibling.InnerText
                            });
                        break;

                }
                nextSibling = nextSibling.NextSibling;
            }
            while (nextSibling != null && (nextSibling.Name == "a" || nextSibling.Name == "#text"));
            
            return paragraph;
        }

        private Figure ProcessFigure(HtmlNode figureNode)
        {
            Figure figure = new Figure
            {
                ImageSource = new Uri(figureNode.GetAttributeValue("data-img-src", String.Empty)),
            };
            foreach (HtmlNode childNode in figureNode.ChildNodes.Where(childNode => childNode.Name == "figcaption"))
            {
                figure.Text = childNode.InnerText;

            }
            return figure;
        }

        private List ProcessList(HtmlNode listNode)
        {
            List list = new List();
            foreach (HtmlNode childNode in listNode.ChildNodes.Where(childNode => childNode.Name == "li"))
            {
                DescriptionParagraph para = ProcessParagraph(childNode);
                list.ContentQueue.Enqueue(para);
            }
            return list;
        }

        private Quote ProcessQuote(HtmlNode quoteNode)
        {
            DescriptionParagraph para = ProcessParagraph(quoteNode);
            Quote quote = new Quote
            {
                Text = para.Text,
                Links = para.Links
            };
            return quote;
        }

        private Section ProcessSubSection(HtmlNode subSectionNode)
        {
            Section section = new Section
            {
                Title = subSectionNode.InnerText,
                Type = subSectionNode.Name
            };

            var nextSibling = subSectionNode.NextSibling;
            while (nextSibling != null && nextSibling.Name != subSectionNode.Name)
            {
                if (nextSibling.Name.StartsWith("h") && int.Parse(nextSibling.Name.Substring(1)) < int.Parse(subSectionNode.Name.Substring(1))) return section;
                switch (nextSibling.Name)
                {
                    case "p":
                        section.ContentQueue.Enqueue(ProcessParagraph(nextSibling));
                        break;
                    case "figure":
                        section.ContentQueue.Enqueue(ProcessFigure(nextSibling));
                        break;
                    case "ul":
                        section.ContentQueue.Enqueue(ProcessList(nextSibling));
                        break;
                    case "blockquote":
                        section.ContentQueue.Enqueue(ProcessQuote(nextSibling));
                        break;
                    case "h3":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && nextSibling.Name != "h3" && nextSibling.Name != "h2")
                        {
                            nextSibling = nextSibling.NextSibling;
                        }
                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && nextSibling.Name != "h4" && nextSibling.Name != "h3" && nextSibling.Name != "h2")
                        {
                            nextSibling = nextSibling.NextSibling;
                        }
                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                }
                if (nextSibling != null) nextSibling = nextSibling.NextSibling;
            }
            return section;
        }
    }
}
