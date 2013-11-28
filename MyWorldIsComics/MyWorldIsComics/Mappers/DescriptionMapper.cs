using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataModel.DescriptionContent;
using MyWorldIsComics.DataModel.Interfaces;

namespace MyWorldIsComics.Mappers
{
    #region usings

    using System;
    using System.Xml;
    using HtmlAgilityPack;
    using DataModel;

    #endregion

    class DescriptionMapper
    {
        private static Description descriptionToReturn = new Description();

        public static Description MapDescription(string htmlString)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            HtmlNodeCollection collection = document.DocumentNode.ChildNodes;
            foreach (HtmlNode link in collection.Where(link => link.Name == "h2"))
            {
                switch (link.InnerText)
                {
                    case "Current Events":
                        descriptionToReturn.CurrentEvents = ProcessSection(link);
                        break;
                    case "Origin":
                    case "Origins":
                        descriptionToReturn.Origin = ProcessSection(link);
                        break;
                    case "Creation":
                        descriptionToReturn.Creation = ProcessSection(link);
                        break;
                    case "Distinguishing Characteristics":
                        descriptionToReturn.DistinguishingCharacteristics = ProcessSection(link);
                        break;
                    case "Character Evolution":
                        descriptionToReturn.CharacterEvolution = ProcessSection(link);
                        break;
                    case "Major Story Arcs":
                        descriptionToReturn.MajorStoryArcs = ProcessSection(link);
                        break;
                    case "Powers and Abilities":
                    case "Powers and Abilties":
                        descriptionToReturn.PowersAndAbilities = ProcessSection(link);
                        break;
                    case "Weapons and Equipment":
                        descriptionToReturn.WeaponsAndEquipment = ProcessSection(link);
                        break;
                    case "Other Versions":
                    case "Alternate Realities":
                        descriptionToReturn.AlternateRealities = ProcessSection(link);
                        break;
                    case "Other Media":
                        descriptionToReturn.OtherMedia = ProcessSection(link);
                        break;
                }
            }
            return descriptionToReturn;
        }

        private static Section ProcessSection(HtmlNode link)
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
                        while (nextSibling != null && (!nextSibling.Name.StartsWith("h") || int.Parse(nextSibling.Name.Substring(1)) < int.Parse(link.Name.Substring(1))))
                        {
                            nextSibling = nextSibling.NextSibling;
                        }

                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && (!nextSibling.Name.StartsWith("h") || int.Parse(nextSibling.Name.Substring(1)) < int.Parse(link.Name.Substring(1))))
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
        
        private static Paragraph ProcessParagraph(HtmlNode paragraphNode)
        {
            Paragraph paragraph = new Paragraph
            {
                Text = paragraphNode.InnerText,
                Links = new List<Link>()
            };

            if (paragraphNode.HasChildNodes)
            {
                var nodes = paragraphNode.ChildNodes;
                foreach (HtmlNode htmlNode in nodes.Where(htmlNode => htmlNode.Name == "a").Where(htmlNode => htmlNode.GetAttributeValue("rel", String.Empty) != "nofollow"))
                {
                    if (paragraph.Links.All(link => link.Href != htmlNode.GetAttributeValue("href", String.Empty)))
                    {
                        paragraph.Links.Add(new Link
                        {
                            Href = htmlNode.GetAttributeValue("href", String.Empty),
                            Text = htmlNode.InnerText
                        });
                    }
                }
            }

            return paragraph;
        }

        private static Figure ProcessFigure(HtmlNode figureNode)
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

        private static List ProcessList(HtmlNode listNode)
        {
            List list = new List();
            foreach (HtmlNode childNode in listNode.ChildNodes.Where(childNode => childNode.Name == "li"))
            {
                Paragraph para = ProcessParagraph(childNode);
                list.ContentQueue.Enqueue(para);
            }
            return list;
        }

        private static Quote ProcessQuote(HtmlNode quoteNode)
        {
            Paragraph para = ProcessParagraph(quoteNode);
            Quote quote = new Quote
            {
                Text = para.Text,
                Links = para.Links
            };
            return quote;
        }
        
        private static Section ProcessSubSection(HtmlNode subSectionNode)
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
                        while (nextSibling != null && !nextSibling.Name.StartsWith("h"))
                        {
                            nextSibling = nextSibling.NextSibling;
                        }
                        if (nextSibling != null) nextSibling = nextSibling.PreviousSibling;
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));

                        nextSibling = nextSibling.NextSibling;
                        while (nextSibling != null && !nextSibling.Name.StartsWith("h"))
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
