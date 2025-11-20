using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY")
                        ?? "AIzaSyDyxMu_vWlNhFo4zCitWrr36DZwp9rzTRg";
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
    }
}
