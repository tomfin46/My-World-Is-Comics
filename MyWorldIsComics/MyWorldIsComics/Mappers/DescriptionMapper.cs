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
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;
            foreach (HtmlNode link in collection.Where(link => link.Name == "h2"))
            {
                switch (link.InnerText)
                {
                    case "Current Events":
                        this.descriptionToMap.CurrentEvents = ProcessSection(link);
                        break;
                    case "Origin":
                    case "Origins":
                        this.descriptionToMap.Origin = ProcessSection(link);
                        break;
                    case "Creation":
                        this.descriptionToMap.Creation = ProcessSection(link);
                        break;
                    case "Distinguishing Characteristics":
                        this.descriptionToMap.DistinguishingCharacteristics = ProcessSection(link);
                        break;
                    case "Character Evolution":
                        this.descriptionToMap.CharacterEvolution = ProcessSection(link);
                        break;
                    case "Major Story Arcs":
                        this.descriptionToMap.MajorStoryArcs = ProcessSection(link);
                        break;
                    case "Miscellaneous":
                        this.descriptionToMap.Miscellaneous = ProcessSection(link);
                        break;
                    case "Hulk's Incarnations":
                        this.descriptionToMap.HulksIncarnations = ProcessSection(link);
                        break;
                    case "Powers and Abilities":
                    case "Abilties":
                    case "Powers":
                    case "Powers and Abilties":
                        this.descriptionToMap.PowersAndAbilities = ProcessSection(link);
                        break;
                    case "Weapons and Equipment":
                        this.descriptionToMap.WeaponsAndEquipment = ProcessSection(link);
                        break;
                    case "Other Versions":
                    case "Alternate Realities":
                        this.descriptionToMap.AlternateRealities = ProcessSection(link);
                        break;
                    case "Other Media":
                        this.descriptionToMap.OtherMedia = ProcessSection(link);
                        break;
                }
            }
            return this.descriptionToMap;
        }

        public Description MapDescription(Issue issue)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(issue.DescriptionString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;

            foreach (HtmlNode link in collection)
            {
                switch (link.Name)
                {
                    case "p":
                        this.ProcessParagraph(link);
                        break;
                }
            }

            return this.descriptionToMap;
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
        
        private Paragraph ProcessParagraph(HtmlNode paragraphNode)
        {
            Paragraph paragraph = new Paragraph
            {
                Text = paragraphNode.InnerText,
                Links = new List<Link>()
            };

            if (paragraphNode.HasChildNodes)
            {
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
            }

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
                Paragraph para = ProcessParagraph(childNode);
                list.ContentQueue.Enqueue(para);
            }
            return list;
        }

        private Quote ProcessQuote(HtmlNode quoteNode)
        {
            Paragraph para = ProcessParagraph(quoteNode);
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
