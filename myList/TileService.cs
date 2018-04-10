using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Xml.Linq;


namespace myList
{
    class TileService
    {
        static public void SetBadgeCountOnTile(int count)
        {
            // Update the badge on the real tile
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }
        public static Windows.Data.Xml.Dom.XmlDocument CreateTiles(Todo newtodo, string pic_path)
        {
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                        new XElement("visual",
                            // Small Tile
                            new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "myList"), new XAttribute("template", "TileSmall"),
                                new XElement("group",
                                    new XElement("subgroup",
                                        new XElement("text", newtodo.Title, new XAttribute("hint-style", "caption")),
                                        new XElement("text", newtodo.Detail, new XAttribute("hint-style", "captionsubtle")),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3)
                                    )
                                )
                            ),
                                 // Medium Tile
                            new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "myList"), new XAttribute("template", "TileMedium"),
                                new XElement("group",
                                    new XElement("subgroup",
                                        new XElement("text", newtodo.Title, new XAttribute("hint-style", "caption")),
                                        new XElement("text", newtodo.Detail, new XAttribute("hint-style", "caption"),
                                        new XAttribute("hint-wrap", true),
                                        new XAttribute("hint-maxLines", 3))
                                    )
                                )
                            ),
                            // Wide Tile
                            new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "myList"), new XAttribute("template", "TileWide"),
                                new XElement("group",
                                    new XElement("subgroup",
                                        new XElement("text", newtodo.Title, new XAttribute("hint-style", "caption")),
                                        new XElement("text", newtodo.Detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3)),
                                        new XElement("text", newtodo.Date, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                        ),
                                        new XElement("subgroup", new XAttribute("hint-weight", 15),
                                            new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", pic_path))
                                        )
                                )
                            ),
                            //Large Tile
                            new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "myList"), new XAttribute("template", "TileLarge"),
                                new XElement("group",
                                    new XElement("subgroup",
                                        new XElement("text", newtodo.Title, new XAttribute("hint-style", "caption")),
                                        new XElement("text", newtodo.Detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3)),
                                        new XElement("text", newtodo.Date, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                        ),
                                        new XElement("subgroup", new XAttribute("hint-weight", 15),
                                            new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", pic_path))
                                        )
                                )
                            )
                        )
                    )
            );
            Windows.Data.Xml.Dom.XmlDocument xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }
    }
}
