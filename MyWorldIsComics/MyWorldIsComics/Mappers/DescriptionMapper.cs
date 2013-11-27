using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataModel.DescriptionContent;

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
                        descriptionToReturn.Origin = ProcessSection(link);
                        break;
                    case "Creation":
                        descriptionToReturn.Creation = ProcessSection(link);
                        break;
                    case "Character Evolution":
                        descriptionToReturn.CharacterEvolution = ProcessSection(link);
                        break;
                    case "Major Story Arcs":
                        descriptionToReturn.MajorStoryArcs = ProcessSection(link);
                        break;
                    case "Other Versions":
                    case "Alternate Realities":
                        descriptionToReturn.AlternateRealities = ProcessSection(link);
                        break;
                }
            }
            return descriptionToReturn;
        }

        private static Section ProcessSection(HtmlNode link)
        {
            Section section = new Section
            {
                Title = link.InnerText
            }; 
            var nextSibling = link.NextSibling;

            while (nextSibling.Name != link.Name)
            {
                switch (nextSibling.Name)
                {
                    case "p":
                        section.ContentQueue.Enqueue(ProcessParagraph(nextSibling));
                        break;
                    case "figure":
                        section.ContentQueue.Enqueue(ProcessFigure(nextSibling));
                        break;
                    case "h3":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));
                        break;
                }

                nextSibling = nextSibling.NextSibling;
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
                    paragraph.Links.Add(new Link
                    {
                        Href = htmlNode.GetAttributeValue("href", String.Empty),
                        Text = htmlNode.InnerText
                    });
                }
            }

            return paragraph;
        }

        private static Figure ProcessFigure(HtmlNode figureNode)
        {
            Figure figure = new Figure
            {
                ImageSource = new Uri(figureNode.GetAttributeValue("data-img-src", String.Empty))
            };
            return figure;
        }

        private static Section ProcessSubSection(HtmlNode subSectionNode)
        {
            Section section = new Section
            {
                Title = subSectionNode.InnerText
            };

            var nextSibling = subSectionNode.NextSibling;
            while (!nextSibling.Name.StartsWith("h"))
            {
                switch (nextSibling.Name)
                {
                    case "p":
                        section.ContentQueue.Enqueue(ProcessParagraph(nextSibling));
                        break;
                    case "figure":
                        section.ContentQueue.Enqueue(ProcessFigure(nextSibling));
                        break;
                    case "h3":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));
                        break;
                    case "h4":
                        section.ContentQueue.Enqueue(ProcessSubSection(nextSibling));
                        break;
                }
            }
            return section;
        }
    }
}
