using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Fetch season player stats from nhl.com",
                        useAsync: true,
                         options: "no-save",
                         example: "//Fetch stats for current season|stats|//Fetch the season 2021/2022 stats|stats 2022|//Fetch stats for a past season and do not save it|stats 2022 --no-save")]
public class StatsCommand : NhlBaseCommand
{
    public StatsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var seasonId = GetSeasonId();

        var seasonStats = await GetSeason(seasonId);
        if(seasonStats.Total > 0 && !HasOption("no-save")) UpdateSeason(seasonStats, seasonId);
        var rank = 1;
        var newNhlPlayerFound = false;
        foreach (var player in seasonStats.Data)
        {
            var nationality = "";
            var existing = PlayersDb.People.FirstOrDefault(p => p.Id == player.PlayerId);
            if (existing == null)
            {
                var nhlPlayer = await GetNhlPlayer(player.PlayerId);
                if (nhlPlayer.Id > 0)
                {
                    nationality = $"{nhlPlayer.Nationality}";
                    PlayersDb.People.Add(nhlPlayer);
                    newNhlPlayerFound = true;
                }
            }
            else nationality = existing.Nationality;
            
            WriteLine($"{rank}. {player.SkaterFullName} {nationality} {player.Points} ({player.Goals}+{player.Assists}) {player.TeamAbbrevs}");
            rank++;
        }
        WriteSuccessLine($"{seasonStats.Data.Count} players fetched from nhl.com");
        if (newNhlPlayerFound)
        {
            SavePlayersDB();
            WriteSuccessLine("New players saved to file.");
        }
        Console.Write(ConfigurationGlobals.Prompt);
        return Ok();
    }

    public async Task<Season> GetSeason(int seasonId)
    {
        var retVal = new Season { SeasonId = seasonId, Data = new()};
        var start = 1;
        var maxIterations = 15;
        
        for(int  i = 0; i < maxIterations; i++)
        {
            var moreSeasonPlayers = await GetSeasonData(seasonId, start);
            WriteLine($"Fetching players from rank {start} to {retVal.Data.Count + moreSeasonPlayers.Data.Count}...");
            retVal.Data.AddRange(moreSeasonPlayers.Data);
            retVal.Total = moreSeasonPlayers.Total;
            var fetchMorePlayers = moreSeasonPlayers.Data.Count == 100;
            if(!fetchMorePlayers)  break;
            start += 100;
            Thread.Sleep(1000);
        }
        return retVal;
    }

    public async Task<Season> GetSeasonData(int seasonId, int start)
    {
        try
        {
            var season = $"{seasonId - 1}{seasonId}";
            var url = $"https://api.nhle.com/stats/rest/en/skater/summary?isAggregate=false&isGame=false&sort=[{{%22property%22:%22points%22,%22direction%22:%22DESC%22}},{{%22property%22:%22goals%22,%22direction%22:%22DESC%22}},{{%22property%22:%22assists%22,%22direction%22:%22DESC%22}},{{%22property%22:%22playerId%22,%22direction%22:%22ASC%22}}]&start={start - 1}&limit=100&factCayenneExp=gamesPlayed%3E=1&cayenneExp=gameTypeId=2%20and%20seasonId%3C={season}%20and%20seasonId%3E={season}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var stats = JsonSerializer.Deserialize<Season>(responseString, options) ?? new Season();
            return stats;
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new Season();
        }
    }
    public void UpdateSeason(Season season, int seasonId)
    {
        season.SeasonId = seasonId;
        var existing = SeasonsDb.SeasonStats.FirstOrDefault(s => s.SeasonId == season.SeasonId);
        if (existing != null) SeasonsDb.SeasonStats.Remove(existing);
        season.Updated = DateTime.Now;
        SeasonsDb.SeasonStats.Add(season);
        SaveSeasonsDB();
        WriteSuccessLine($"Season {seasonId - 1}{seasonId} saved!");
    }
}