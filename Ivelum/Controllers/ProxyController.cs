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

            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'content html_format')]"))
            {
                if (string.IsNullOrWhiteSpace(node.InnerText)) continue;
                node.ParentNode.ReplaceChild(HtmlTextNode.CreateNode(AddAdditionalEl(node.InnerText, additionalEl)), node);
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }

        private string AddAdditionalEl(string innerText, string additionalEl)
        {
            StringBuilder result = new StringBuilder();
            int count = 0;
            int maxCount = 6;
            foreach (char ch in innerText.ToCharArray())
            {
                if (Char.IsWhiteSpace(ch) && count == maxCount)
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
                result.Append(ch);
            }
            return result.ToString();
        }

        private string ConvertUrls(string input, string from, string to)
        {
            Regex r = new Regex(from, RegexOptions.IgnoreCase);
            
            return r.Replace(input, to);
        }

    }
}