using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using MyWorldIsComics.DataModel;

namespace MyWorldIsComics.Mappers
{
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

                HtmlNode sibling = link.NextSibling;
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
                        descriptionToReturn.CurrentEvents = sibling.InnerText;
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
                }
            }
            return descriptionToReturn;
        }

        private static void ParseCurrentEvents(XmlReader reader)
        {

        }

        private static void ParseOrigin(XmlReader reader)
        {

        }

        private static void ParseCreation(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        private static void ParseCharacterEvolution(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        private static void ParseMajorStoryArcs(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        private static void ParseAlternateRealities(XmlReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
