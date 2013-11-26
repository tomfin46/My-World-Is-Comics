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

                //if (link.Name == "h2")
                //{
                //    ProcessSection(link);
                //}


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
                        descriptionToReturn.CurrentEvents = link.NextSibling.InnerHtml;
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

        private static void ProcessSection(HtmlNode link)
        {
            var headerName = link.InnerText;
            var nextSibling = link.NextSibling;

            while (nextSibling.Name != "h2")
            {
                if (nextSibling.Name == "p")
                {
                    ProcessParagraph(nextSibling, headerName);
                }
                else if (nextSibling.Name == "figure")
                {
                    ProcessFigure(nextSibling, headerName);
                }
                else if (nextSibling.Name == "h3")
                {
                    ProcessSubSection(nextSibling, headerName);
                }
                else if (nextSibling.Name == "h4")
                {
                    ProcessSubSubSection(nextSibling, headerName);
                }

                nextSibling = nextSibling.NextSibling;
            }


        }

        private static void ProcessParagraph(HtmlNode nextSibling, string headerName)
        {
            throw new NotImplementedException();
        }

        private static void ProcessFigure(HtmlNode nextSibling, string headerName)
        {
            throw new NotImplementedException();
        }

        private static void ProcessSubSection(HtmlNode nextSibling, string headerName)
        {
            throw new NotImplementedException();
        }

        private static void ProcessSubSubSection(HtmlNode nextSibling, string headerName)
        {
            throw new NotImplementedException();
        }
    }
}
