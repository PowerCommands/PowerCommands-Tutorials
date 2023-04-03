using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Show points statistic for a specific season, default take count is 25",
                         options: "take",
                     suggestions: "2023|2022|2021|2020|2019|2018",
                         example: "points 2023|//Points for season 2023, take first 100|points 2023 --take 100")]
public class PointsCommand : NhlBaseCommand
{
    public PointsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override RunResult Run()
    {
        var seasonId = Input.FirstArgumentToInt();
        var take = Input.OptionToInt("take", 25);
        var stats = SeasonsDb.SeasonStats.First(s => s.SeasonId == seasonId).Data.Take(take).ToArray();
        var pointsTable = new List<PointTableItem>();
        for (int i= 0; i < stats.Length; i++)
        {
            var playerStat = stats[i];
            var player = PlayersDb.People.FirstOrDefault(p => p.Id == playerStat.PlayerId) ?? new Player{FullName = "MISSING"};
            var pointTableItem = new PointTableItem { Place = i + 1, Nationality = player.Nationality, GamesPlayed = playerStat.GamesPlayed, Assists = playerStat.Assists, FullName = playerStat.SkaterFullName, Points = playerStat.Points, Goals = playerStat.Goals, PointsPerGame = playerStat.PointsPerGame, TeamAbbrevs = playerStat.TeamAbbrevs };
            pointsTable.Add(pointTableItem);
        }
        ConsoleTableService.RenderTable(pointsTable, this);
        return Ok();
    }
}