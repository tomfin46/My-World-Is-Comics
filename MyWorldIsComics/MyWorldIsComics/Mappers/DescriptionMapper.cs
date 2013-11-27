using System.Collections.Generic;
using System.Linq;
using MyWorldIsComics.DataModel.DescriptionElements;

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
            foreach (HtmlNode link in collection)
            {
                if (link.Name != "h2") continue;

                if (link.Name == "h2")
                {
                    switch (link.InnerText)
                    {
                        case "Current Events":
                            descriptionToReturn.CurrentEvents = ProcessSection(link);
                            break;
                        //case "Origin":
                        //    descriptionToReturn.Origin = ProcessSection(link);
                        //    break;
                        //case "Creation":
                        //    descriptionToReturn.Creation = ProcessSection(link);
                        //    break;
                        //case "Character Evolution":
                        //    descriptionToReturn.CharacterEvolution = ProcessSection(link);
                        //    break;
                        //case "Major Story Arcs":
                        //    descriptionToReturn.MajorStoryArcs = ProcessSection(link);
                        //    break;
                        //case "Other Versions":
                        //case "Alternate Realities":
                        //    descriptionToReturn.AlternateRealities = ProcessSection(link);
                        //    break;
                    }

                }


                /*HtmlNode sibling = link.NextSibling;
                HtmlNode nextSibling;
                while (sibling.Name != "p")
                {
                    if (sibling.NextSibling != null)
                    {
                        sibling = sibling.NextSibling;
                    }
                }

                switch (link.InnerText)
                {
                    case "Current Events":
                        descriptionToReturn.CurrentEvents = link.NextSibling.InnerText;
                        break;
                    case "Origin":
                        descriptionToReturn.Origin = sibling.InnerText;
                        nextSibling = sibling.NextSibling;
                        if (nextSibling != null)
                        {
                            while (nextSibling.Name == "p")
                            {
                                descriptionToReturn.Origin += "\n" + nextSibling.InnerText;
                                nextSibling = nextSibling.NextSibling;
                            }
                        }

                        break;
                    case "Creation":
                        descriptionToReturn.Creation = sibling.InnerText;
                        nextSibling = sibling.NextSibling;
                        if (nextSibling != null)
                        {
                            while (nextSibling.Name == "p")
                            {
                                descriptionToReturn.Creation += "\n" + nextSibling.InnerText;
                                nextSibling = nextSibling.NextSibling;
                            }
                        }
                        break;
                    case "Character Evolution":
                        descriptionToReturn.CharacterEvolution = sibling.InnerText;
                        nextSibling = sibling.NextSibling;
                        if (nextSibling != null)
                        {
                            while (nextSibling.Name == "p")
                            {
                                descriptionToReturn.CharacterEvolution += "\n" + nextSibling.InnerText;
                                nextSibling = nextSibling.NextSibling;
                            }
                        }
                        break;
                    case "Major Story Arcs":
                        descriptionToReturn.MajorStoryArcs = sibling.InnerText;
                        nextSibling = sibling.NextSibling;
                        if (nextSibling != null)
                        {
                            while (nextSibling.Name == "p")
                            {
                                descriptionToReturn.MajorStoryArcs += "\n" + nextSibling.InnerText;
                                nextSibling = nextSibling.NextSibling;
                            }
                        }
                        break;
                    case "Alternate Realities":
                        descriptionToReturn.AlternateRealities = sibling.InnerText;
                        nextSibling = sibling.NextSibling;
                        if (nextSibling != null)
                        {
                            while (nextSibling.Name == "p")
                            {
                                descriptionToReturn.AlternateRealities += "\n" + nextSibling.InnerText;
                                nextSibling = nextSibling.NextSibling;
                            }
                        }
                        break;
                }*/
            }
            return descriptionToReturn;
        }

        private static Queue<Paragraph> ProcessSection(HtmlNode link)
        {
            var headerSectionQueue = new Queue<Paragraph>();
            var nextSibling = link.NextSibling;

            while (nextSibling.Name != link.Name)
            {
                switch (nextSibling.Name)
                {
                    case "p":
                        headerSectionQueue.Enqueue(ProcessParagraph(nextSibling));
                        break;
                    //case "figure":
                    //    ProcessFigure(nextSibling);
                    //    break;
                    //case "h3":
                    //    ProcessSubSection(nextSibling);
                    //    break;
                    //case "h4":
                    //    ProcessSubSubSection(nextSibling);
                    //    break;
                }

                nextSibling = nextSibling.NextSibling;
            }

            return headerSectionQueue;
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

        private static void ProcessFigure(HtmlNode figureNode, Queue<string> headerSection)
        {
            var image = figureNode.GetAttributeValue("data-img-src", String.Empty);
        }

        private static void ProcessSubSection(HtmlNode subSectionNode, Queue<string> headerSection)
        {
            headerSection.Enqueue(subSectionNode.Name);
        }

        private static void ProcessSubSubSection(HtmlNode subSubSectionNode, Queue<string> headerSection)
        {
            headerSection.Enqueue(subSubSectionNode.Name);
        }
    }
}
