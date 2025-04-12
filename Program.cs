using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

public class Program
{
    private static HttpClient _httpClient = new();
    private static HtmlDocument _htmlDocument = new();
    private static readonly string regex_str = @"

  (?:\""|')                               # Start newline delimiter

  (
    ((?:[a-zA-Z]{1,10}://|//)           # Match a scheme [a-Z]*1-10 or //
    [^""'/]{1,}\.                        # Match a domainname (any character + dot)
    [a-zA-Z]{2,}[^""']{0,})              # The domainextension and/or path

    |

    ((?:/|\.\./|\./)                    # Start with /,../,./
    [^""'><,;| *()(%%$^/\\\[\]]          # Next character can't be...
    [^""'><,;|()]{1,})                   # Rest of the characters can't be

    |

    ([a-zA-Z0-9_\-/]{1,}/               # Relative endpoint with /
    [a-zA-Z0-9_\-/.]{1,}                # Resource name
    \.(?:[a-zA-Z]{1,4}|action)          # Rest + extension (length 1-4 or action)
    (?:[\?|#][^""|']{0,}|))              # ? or # mark with parameters

    |

    ([a-zA-Z0-9_\-/]{1,}/               # REST API (no extension) with /
    [a-zA-Z0-9_\-/]{3,}                 # Proper REST endpoints usually have 3+ chars
    (?:[\?|#][^""|']{0,}|))              # ? or # mark with parameters

    |

    ([a-zA-Z0-9_\-]{1,}                 # filename
    \.(?:php|asp|aspx|jsp|json|
         action|html|js|txt|xml)        # . + extension
    (?:[\?|#][^""|']{0,}|))              # ? or # mark with parameters

  )

  (?:\""|')                               # End newline delimiter

";  // from linkFinder -> https://github.com/GerbenJavado/LinkFinder

    private static readonly Regex _urlRegex = new Regex(regex_str, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    public static async Task Main()
    {
        string url = String.Empty;
        string html = String.Empty;
        Console.WriteLine("Welcome to the TraceJS");
        while (true)
        {
            Console.WriteLine("Choose an action:");
            Console.WriteLine("1 - List inline JavaScript in the given page");
            Console.WriteLine("2 - List external JavaScript file URLs");
            Console.WriteLine("3 - Analyze paths inside external JavaScript");
            Console.WriteLine("4 - Exit");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("Enter URL: ");
                    url = Console.ReadLine();
                    html = await FetchDataAsynch(url);
                    await FetchInnerJS(html);
                    break;
                case "2":          // Bu kısım daha ayrıntılı hale getirilebilir.
                    Console.Write("Enter URL: ");
                    url = Console.ReadLine();
                    html = await FetchDataAsynch(url);
                    await FetchExternalJS(html, url);
                    break;
                case "3":
                    Console.Write("Enter URL: ");
                    url = Console.ReadLine();
                    html = await FetchDataAsynch(url);
                    await FindPath_in_ExternalJS(html, url);
                    break;
                case "4":
                    return;
            }
        }
    }

    public static async Task<string> FetchDataAsynch(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            string htmlcontent = await response.Content.ReadAsStringAsync();
            return htmlcontent;
        }
        catch (HttpRequestException e)
        {
            return string.Empty;
        }
    }

    public static async Task<string> FetchInnerJS(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            Console.WriteLine("No HTML content");
            return string.Empty;
        }
        else
        {
            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(htmlContent);
            var scriptNode = _htmlDocument.DocumentNode.SelectNodes("//script[not(@src)]");
            string allScripts = string.Empty;
            if (scriptNode != null)
            {
                foreach (var sc in scriptNode)
                {
                    if (!string.IsNullOrWhiteSpace(sc.InnerText))
                    {
                        Console.WriteLine("Script Found: ");
                        Console.WriteLine(sc.InnerText);
                        allScripts += sc.InnerText + "\n";
                    }
                }
                return allScripts;
            }
            else
            {
                Console.WriteLine("No inline script tags found.");
                return string.Empty;
            }

        }
    }

    public static async Task<List<string>> FetchExternalJS(string htmlContent, string url)
    {
        List<string> urls = new List<string>();
        if (string.IsNullOrEmpty(htmlContent))
        {
            Console.WriteLine("No HTML content");
            return null;
        }
        else
        {
            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(htmlContent);
            var scriptNode = _htmlDocument.DocumentNode.SelectNodes("//script[@src]");

            if (scriptNode != null)
            {
                string fullUrl = string.Empty;
                foreach (var node in scriptNode)
                {
                    var srcValue = node.GetAttributeValue("src", null); 
                    if (srcValue.StartsWith("https://")) // URI kullanılarak daha detaylı şekilde URL'ler düzenlenmeli. 
                    {
                        fullUrl = srcValue;
                    }
                    else
                    {
                        fullUrl = url + srcValue;
                    }
                    Console.WriteLine($"URL: {fullUrl}");
                    urls.Add(fullUrl);
                }
                return urls;
            }
            else
            {
                Console.WriteLine("No external script tags found.");
                return null;
            }
        }

    }

    public static async Task FindPath_in_ExternalJS(string htmlcontent, string url)
    {
        var jsUrls = await FetchExternalJS(htmlcontent, url);
        if (jsUrls.Count == 0)
        {
            Console.WriteLine("No external script tags found.");
            return;
        }
        foreach (var jsUrl in jsUrls)
        {
            var jsContent = await FetchDataAsynch(jsUrl);
            var baseurlhtml = await FetchDataAsynch(url);
            if (string.IsNullOrEmpty(jsContent))
            {
                Console.WriteLine($"Empty or failed to fetch: {jsUrl}");
                continue;
            }

            var matches = _urlRegex.Matches(jsContent);
            foreach (Match match in matches)
            {
                Console.WriteLine($"Found in {jsUrl}: {match.Value}");
            }

            var matches2 = _urlRegex.Matches(baseurlhtml);
            foreach (Match match in matches2)
            {
                Console.WriteLine($"Found in {url}: {match.Value}");
            }
        }

    }

}

