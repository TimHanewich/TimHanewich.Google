using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Generic;

namespace TimHanewich.Google
{
    public class GoogleSearch
    {
        public SearchResult[] Results {get; set;}

        public static async Task<GoogleSearch> SearchAsync(string query)
        {
            //Convert the query to html friendly.
            string QueryHtml = HttpUtility.HtmlEncode(query);
            QueryHtml = QueryHtml.Replace(" ", "+"); //Replace spaces with +
            

            //Make the HTTP call
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri("https://www.google.com/search?q=" + QueryHtml);
            HttpResponseMessage response = await hc.SendAsync(req);
            string web = await response.Content.ReadAsStringAsync();
    
            GoogleSearch ToReturn = ParseWebContent(web);
            return ToReturn;
        }
    
        public static GoogleSearch ParseWebContent(string web)
        {
            //Tools
            int loc1 = 0;
            int loc2 = 0;

            //Split into results
            List<SearchResult> Results = new List<SearchResult>();
            string[] parts = web.Split(new string[]{"ZINbbc xpd O9g5cc uUPGi"}, StringSplitOptions.None);
            for (int t = 2; t < parts.Length; t++)
            {

                //Is this a valid search result?
                bool IsValidSearchResult = true;
                if (parts[t].Contains("\\x22\\x3e\\x3"))
                {
                    IsValidSearchResult = false;
                }
                
                //Process it if it is valid
                if (IsValidSearchResult)
                {
                    //Get the URL
                    loc1 = parts[t].IndexOf("href");
                    loc1 = parts[t].IndexOf("\"", loc1 + 1);
                    loc2 = parts[t].IndexOf("\"", loc1 + 1);
                    string url = "https://google.com" + parts[t].Substring(loc1 + 1, loc2 - loc1 - 1);

                    //Get the title
                    loc1 = parts[t].IndexOf("zBAuLc");
                    loc1 = parts[t].IndexOf("<div class", loc1 + 1);
                    loc1 = parts[t].IndexOf(">", loc1 + 1);
                    loc2 = parts[t].IndexOf("<", loc1 + 1);
                    string title = parts[t].Substring(loc1 + 1, loc2 - loc1 - 1);
                    title = HttpUtility.HtmlDecode(title);

                    //Get the description
                    loc1 = parts[t].IndexOf("BNeawe s3v9rd AP7Wnd");
                    loc1 = parts[t].IndexOf("BNeawe s3v9rd AP7Wnd", loc1 + 1);
                    loc1 = parts[t].IndexOf(">", loc1 + 1);
                    loc2 = parts[t].IndexOf("<", loc1 + 1);
                    string description = parts[t].Substring(loc1 + 1, loc2 - loc1 - 1);
                    description = HttpUtility.HtmlDecode(description);


                    //Add if a title is present
                    if (title != "")
                    {
                        SearchResult sr = new SearchResult();
                        sr.Url = url;
                        sr.Title = title;
                        sr.Description = description;
                        Results.Add(sr);
                    }
                }
            }

            //Construt and return
            GoogleSearch ToReturn = new GoogleSearch();
            ToReturn.Results = Results.ToArray();
            return ToReturn;
        }
    }
}