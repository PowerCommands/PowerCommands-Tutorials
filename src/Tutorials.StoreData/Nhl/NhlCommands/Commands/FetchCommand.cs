using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Description of your command...",
                        useAsync: true,
                         options: "season",
                         example: "//Fetch the season 2022/2023 stats|fetch 2023")]
public class FetchCommand : CommandBase<PowerCommandsConfiguration>
{
    public FetchCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var season = Input.FirstArgumentToInt();

        var seasonStats = await GetSeasonData(season);
        var rank = 1;
        foreach (var player in seasonStats.Data)
        {
            WriteLine($"{rank}. {player.SkaterFullName} {player.Points} ({player.Goals}+{player.Assists}) {player.TeamAbbrevs}");
            rank++;
        }
        return Ok();
    }

    public static async Task<Season> GetSeasonData(int seasonId)
    {
        var season = $"{seasonId - 1}{seasonId}";
        var url = $"https://api.nhle.com/stats/rest/en/skater/summary?isAggregate=false&isGame=false&sort=[{{%22property%22:%22points%22,%22direction%22:%22DESC%22}},{{%22property%22:%22goals%22,%22direction%22:%22DESC%22}},{{%22property%22:%22assists%22,%22direction%22:%22DESC%22}},{{%22property%22:%22playerId%22,%22direction%22:%22ASC%22}}]&start=0&limit=200&factCayenneExp=gamesPlayed%3E=1&cayenneExp=gameTypeId=2%20and%20seasonId%3C={season}%20and%20seasonId%3E={season}";

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true,  };
        var stats = JsonSerializer.Deserialize<Season>(responseString, options);
        return stats ?? new Season();
    }
}