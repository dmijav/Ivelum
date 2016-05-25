using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Ivelum.Controllers
{
    public class ProxyController : Controller
    {
        private const string HostInner = @"http://localhost:59104/";
        private const string HostOuter = @"https://habrahabr.ru/";
        private const string AdditionalEl = "™";

        public async Task<ActionResult> Index()
        {
            var baseUrl = Request.Url;

            string uri = ConvertUrls(baseUrl.AbsoluteUri, HostInner, HostOuter);
            var request = WebRequest.Create(uri);
            var result = request.GetResponse();
          
            var stream = result.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
   
            string reqResp = sr.ReadToEnd().Trim();

            reqResp = ConvertUrls(reqResp, HostOuter, HostInner);
            var updatedResp = UpdateHtmlData(reqResp, AdditionalEl);
            return Content(updatedResp);

        }

        private string UpdateHtmlData(string inputReq, string additionalEl)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(new StringReader(inputReq));
            try
            {
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//a[contains(@class,'post__title_link') or contains(@class,'hubs')]"))
                {
                    if (string.IsNullOrWhiteSpace(node.InnerText)) continue;
                    node.InnerHtml = AddAdditionalEl(node, additionalEl);
                }
            }
            catch (Exception e)
            {
            }
            try
            {
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//h2[contains(@class,'post__title')]"))
                {
                    if (string.IsNullOrWhiteSpace(node.InnerText)) continue;
                    node.InnerHtml = AddAdditionalEl(node, additionalEl);
                }
            }
            catch (Exception e)
            {
            }
            try
            {
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'content html_format') or contains(@class,'author-info__username')]"))
                {
                    if (string.IsNullOrWhiteSpace(node.InnerText)) continue;
                    node.InnerHtml = AddAdditionalEl(node, additionalEl);
                }
            }
            catch (Exception e)
            {
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }

        private string AddAdditionalEl(HtmlNode node, string additionalEl)
        {
            var innerHtml = node.InnerHtml;
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(new StringReader(innerHtml));
            foreach (HtmlTextNode innerNode in htmlDoc.DocumentNode.SelectNodes("//text()"))
            {
                innerNode.Text = AddAdditionalSimbol(innerNode.Text, additionalEl);
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }

        private string AddAdditionalSimbol(string innerHtml, string additionalEl)
        {
            StringBuilder result = new StringBuilder();
            int count = 0;
            int maxCount = 6;
            foreach (char ch in innerHtml.ToCharArray())
            {
                if ((Char.IsWhiteSpace(ch) || ch == ',' || ch == '.' || ch == ':' || ch == ';' || ch == '!' || ch == '?') && count == maxCount)
                {
                    result.Append(additionalEl);
                    count = 0;
                }
                if (Char.IsLetter(ch))
                {
                    count++;
                }
                else
                {
                    count = 0;
                }
                if (ch == '\n') result.Append(Environment.NewLine);
                result.Append(ch);
            }
            if (count == maxCount) result.Append(additionalEl);
            return result.ToString();
        }

        private string ConvertUrls(string input, string from, string to)
        {
            Regex r = new Regex(from, RegexOptions.IgnoreCase);
            
            return r.Replace(input, to);
        }

    }
}