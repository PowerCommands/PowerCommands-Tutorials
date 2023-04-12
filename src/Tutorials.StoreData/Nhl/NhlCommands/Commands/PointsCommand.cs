using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Show points statistics",
                         options: "stop",
                         example: "point 2010")]
public class PointsCommand : NhlBaseCommand
{
    public PointsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var start = Input.FirstArgumentToInt() == 0 ? GetCurrentSeason() : Input.FirstArgumentToInt();
        var stop = Input.OptionToInt("stop", start)+1;

        var stats = new List<SeasonPointStats>();
        for (int seasonId = start; seasonId < stop; seasonId++) stats.Add(GetPointStats(seasonId));
        
        ConsoleTableService.RenderTable(stats, this);

        return Ok();
    }
    private SeasonPointStats GetPointStats(int seasonId)
    {
        var stats = DatabaseManager.SeasonsDb.SkaterStats.FirstOrDefault(s => s.SeasonId == seasonId);
        if (stats == null) return new SeasonPointStats { Season = $"{GetSeasonForDisplay(seasonId)}", Status = "LOCKOUT", Winner = "-",Nation = "-"};
        var standings = DatabaseManager.StandingsDb.Standings.Where(s => s.SeasonId == seasonId).ToList();
        
        
        var rounds = standings.First().Records.First().TeamRecords.First().GamesPlayed;
        var status = seasonId > 1993 && rounds < 82 ? "Interrupted" : "Completed";
        if (seasonId == GetCurrentSeason() && DateTime.Now.Month < 5) status = "Current";
        var winner = stats.Data.OrderByDescending(d => d.Points).First();
        var player = DatabaseManager.PlayersDb.People.First(p => p.Id == winner.PlayerId);
        var over99 = stats.Data.Count(p => p.Points > 99);

        return new SeasonPointStats { Season = GetSeasonForDisplay(seasonId), Status = status, Winner = player.FullName, Nation = player.Nationality, WinnerPoint = winner.Points, Over99 = over99, Games = rounds, PointsPerGame = winner.PointsPerGame };
    }
}