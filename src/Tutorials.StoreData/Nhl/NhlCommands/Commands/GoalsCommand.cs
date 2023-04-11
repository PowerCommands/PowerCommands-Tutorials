using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Show statistics regarding goals",
                         options: "stop",
                         example: "//Show basic statistic for current season|goals")]
public class GoalsCommand : NhlBaseCommand
{
    public GoalsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var start = Input.FirstArgumentToInt();
        var stop = Input.OptionToInt("stop", start)+1;

        var goalStats = new List<SeasonGoalStats>();
        for (int seasonId = start; seasonId < stop; seasonId++) goalStats.Add(GetGoalStats(seasonId));
        
        ConsoleTableService.RenderTable(goalStats, this);

        return Ok();
    }

    private SeasonGoalStats GetGoalStats(int seasonId)
    {
        var stats = DatabaseManager.SeasonsDb.SeasonStats.FirstOrDefault(s => s.SeasonId == seasonId);
        if (stats == null) return new SeasonGoalStats { Season = $"{GetSeasonForDisplay(seasonId)} LOCKOUT" };
        var standings = DatabaseManager.StandingsDb.Standings.Where(s => s.SeasonId == seasonId);
        var totalGoals = stats.Data.Sum(d => d.Goals);
        var totalTeams = standings.SelectMany(standing => standing.Records).Sum(record => record.TeamRecords.Length);
        var totalGames = (from standing in standings from record in standing.Records from teamRecord in record.TeamRecords select teamRecord.GamesPlayed).Sum()/2;
        
        var goalsPerGame = (decimal)totalGoals / totalGames;

        return new SeasonGoalStats{Goals = totalGoals, GoalsPerGame = Math.Round(goalsPerGame,2), Matches = totalGames,Season = GetSeasonForDisplay(seasonId),Teams = totalTeams};
    }
}