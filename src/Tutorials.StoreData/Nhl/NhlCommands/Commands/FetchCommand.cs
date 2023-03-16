using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Description of your command...",
                        useAsync: true,
                         options: "save",
                         example: "//Fetch the season 2022/2023 stats|fetch 2023")]
public class FetchCommand : CommandBase<PowerCommandsConfiguration>
{
    public FetchCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var seasonId = Input.FirstArgumentToInt();

        var seasonStats = await GetSeasonData(seasonId, page: 1);   //First top 100
        var seasonStats2 = await GetSeasonData(seasonId, page: 2);  //101-200
        seasonStats.Data.AddRange(seasonStats2.Data);
        
        if(seasonStats.Total > 0 && Input.HasOption("save")) SaveSeason(seasonStats, seasonId);

        var rank = 1;
        foreach (var player in seasonStats.Data)
        {
            WriteLine($"{rank}. {player.SkaterFullName} {player.Points} ({player.Goals}+{player.Assists}) {player.TeamAbbrevs}");
            rank++;
        }
        Console.Write(ConfigurationGlobals.Prompt);
        return Ok();
    }

    public async Task<Season> GetSeasonData(int seasonId, int page)
    {
        var season = $"{seasonId - 1}{seasonId}";
        var url = $"https://api.nhle.com/stats/rest/en/skater/summary?isAggregate=false&isGame=false&sort=[{{%22property%22:%22points%22,%22direction%22:%22DESC%22}},{{%22property%22:%22goals%22,%22direction%22:%22DESC%22}},{{%22property%22:%22assists%22,%22direction%22:%22DESC%22}},{{%22property%22:%22playerId%22,%22direction%22:%22ASC%22}}]&start={page-1}&limit=100&factCayenneExp=gamesPlayed%3E=1&cayenneExp=gameTypeId=2%20and%20seasonId%3C={season}%20and%20seasonId%3E={season}";

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true,  };
        var stats = JsonSerializer.Deserialize<Season>(responseString, options) ?? new Season();
        return stats;
    }
    public void SaveSeason(Season season, int seasonId)
    {
        season.SeasonId = seasonId;
        var seasons = StorageService<Seasons>.Service.GetObject();
        var existing = seasons.SeasonStats.FirstOrDefault(s => s.SeasonId == season.SeasonId);
        if (existing != null) seasons.SeasonStats.Remove(existing);
        seasons.SeasonStats.Add(season);
        StorageService<Seasons>.Service.StoreObject(seasons);
        WriteSuccessLine($"Season {seasonId - 1}{seasonId} saved!");
    }
}