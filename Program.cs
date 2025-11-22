using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Proszę ustawić zmienną środowiskową YOUTUBE_API_KEY z kluczem API YouTube.");
            return;
        }
        Console.WriteLine("Podaj kod kraju (np. PL, DE, US):");
        string region = args.Length > 0 ? args[0] : Console.ReadLine(); 

        string url = $"https://www.googleapis.com/youtube/v3/videos" +
                     $"?part=snippet,statistics&chart=mostPopular" +
                     $"&regionCode={region}&maxResults=10&key={apiKey}";

        using var http = new HttpClient();
        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Błąd: {response.StatusCode}");
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");

        Console.WriteLine($"Top 10 najpopularniejszych filmów na YouTube w kraju {region}:");
        int i = 1;
        foreach (var item in items.EnumerateArray())
        {
            var snippet = item.GetProperty("snippet");
            var stats = item.GetProperty("statistics");

            string title = snippet.GetProperty("title").GetString() ?? "";
            string channel = snippet.GetProperty("channelTitle").GetString() ?? "";
            string views = stats.TryGetProperty("viewCount", out var v) ? v.GetString() : "brak";

            Console.WriteLine($"{i,2}. {title} — {channel} ({views} wyświetleń)");
            i++;
        }


        url = $"https://www.googleapis.com/youtube/v3/videos" +
                     $"?part=snippet,statistics&chart=mostPopular" +
                     $"&regionCode={region}&maxResults=10&key={apiKey}";
        response = await http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Błąd: {response.StatusCode}");
            return;
        }
        json = await response.Content.ReadAsStringAsync();
        using var docLikes = JsonDocument.Parse(json);
        var itemsLikes = docLikes.RootElement.GetProperty("items");
        Console.WriteLine($"Top 10 najbardziej lubianych filmów na YouTube w kraju {region}:");
        i = 1;


var videos = items.EnumerateArray()
    .Select(item => new
    {
        Item = item,
        LikeCount = item.GetProperty("statistics")
                        .TryGetProperty("likeCount", out var likeProp) 
                        ? int.Parse(likeProp.GetString() ?? "0") 
                        : 0
    })
    .OrderByDescending(i => i.LikeCount)
    .Take(10)
    .Select(i => i.Item);

int index = 1;
foreach (var item in videos)
{
    var snippet = item.GetProperty("snippet");
    var stats = item.GetProperty("statistics");

    string title = snippet.GetProperty("title").GetString() ?? "";
    string channel = snippet.GetProperty("channelTitle").GetString() ?? "";
    string likes = stats.TryGetProperty("likeCount", out var likeProp) ? likeProp.GetString() : "brak";

    Console.WriteLine($"{index,2}. {title} | {channel} | 👍 {likes} polubień");
    index++;
}
    }

        }
    
 


