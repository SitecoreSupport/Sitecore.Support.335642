using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Web;
using System.Collections.Generic;
using Sitecore.Data.Fields;

namespace Sitecore.Support.Data.Fields
{
    public class HtmlField : Sitecore.Data.Fields.HtmlField
    {
        public HtmlField(Field innerField)
          : base(innerField)
        {
        }

        public override void ValidateLinks(LinksValidationResult result)
        {
            Assert.ArgumentNotNull((object)result, "result");
            string html = this.Value;
            if (string.IsNullOrEmpty(html))
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            this.AddTextLinks(result, document);
            this.AddMediaLinks(result, document);
        }

        private void AddTextLinks(LinksValidationResult result, HtmlDocument document)
        {
            Assert.ArgumentNotNull((object)result, "result");
            Assert.ArgumentNotNull((object)document, "document");
            HtmlNodeCollection htmlNodeCollection = document.DocumentNode.SelectNodes("//a[@href]");
            if (htmlNodeCollection == null)
                return;

            foreach (var node in htmlNodeCollection)
            {
                HtmlNode current = node;
                this.AddTextLink(result, current);
            }

        }

        private void AddTextLink(LinksValidationResult result, HtmlNode node)
        {
            Assert.ArgumentNotNull((object)result, "result");
            Assert.ArgumentNotNull((object)node, "node");
            string attributeValue = node.GetAttributeValue("href", string.Empty);
            List<string> stringList = new List<string>()
      {
        "~/link.aspx?"
      };
            stringList.AddRange((IEnumerable<string>)MediaManager.Provider.Config.MediaPrefixes);
            bool flag = false;
            foreach (string str in stringList)
            {
                if (attributeValue.IndexOf(str) >= 0 && !attributeValue.Contains("://"))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return;
            try
            {
                Item linkedToItem = this.GetLinkedToItem(attributeValue);
                HtmlField.AddLink(result, linkedToItem, attributeValue);
            }
            catch
            {
                HtmlField.AddLink(result, (Item)null, attributeValue);
            }
        }

        private void AddMediaLinks(LinksValidationResult result, HtmlDocument document)
        {
            Assert.ArgumentNotNull((object)result, "result");
            Assert.ArgumentNotNull((object)document, "document");
            HtmlNodeCollection htmlNodeCollection = document.DocumentNode.SelectNodes("//img");
            if (htmlNodeCollection == null)
                return;

            foreach (var node in htmlNodeCollection)
            {
                HtmlNode current = node;
                this.AddMediaLink(result, current);
            }
        }

        private void AddMediaLink(LinksValidationResult result, HtmlNode node)
        {
            Assert.ArgumentNotNull((object)result, "result");
            Assert.ArgumentNotNull((object)node, "node");
            string attributeValue = node.GetAttributeValue("src", string.Empty);
            if (string.IsNullOrEmpty(attributeValue))
                return;
            bool flag = false;
            foreach (string mediaPrefix in MediaManager.Provider.Config.MediaPrefixes)
            {
                if (attributeValue.IndexOf(mediaPrefix) >= 0 && !attributeValue.Contains("://"))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return;
            try
            {
                Item targetItem = this.InnerField.Database.GetItem(DynamicLink.Parse(attributeValue).ItemId);
                HtmlField.AddLink(result, targetItem, attributeValue);
            }
            catch
            {
                HtmlField.AddLink(result, (Item)null, attributeValue);
            }
        }

        private Item GetLinkedToItem(string href)
        {
            Assert.ArgumentNotNull((object)href, "href");
            DynamicLink dynamicLink;
            try
            {
                dynamicLink = DynamicLink.Parse(href);
            }
            catch (InvalidLinkFormatException ex)
            {
                return (Item)null;
            }
            return this.InnerField.Database.GetItem(dynamicLink.ItemId);
        }

        private static void AddLink(LinksValidationResult result, Item targetItem, string targetPath)
        {
            Assert.ArgumentNotNull((object)result, "result");
            Assert.ArgumentNotNull((object)targetPath, "targetPath");
            if (targetItem != null)
                result.AddValidLink(targetItem, targetPath);
            else
                result.AddBrokenLink(targetPath);
        }
    }
}