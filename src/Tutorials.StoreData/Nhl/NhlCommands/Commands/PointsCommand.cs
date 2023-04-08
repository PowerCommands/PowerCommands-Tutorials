using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Show points statistic for a specific season or current season, default take count is 25, if filter (name or nation) is used the result includes all players matching the filter",
                         options: "nation|name|take",
                     suggestions: "SWE|FIN|CAN|USA|CZE|SVK|DEU|NOR|DNK|NLD|BLR|CHE|LVA|RUS",
                         example: "Show points stats for current season|points|//Show points stats for season 2023, take first 100|points 2023 --take 100|//Show points stats for all swedish players for current season|points --nation swe")]
public class PointsCommand : NhlBaseCommand
{
    public PointsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override RunResult Run()
    {
        var seasonId = Input.FirstArgumentToInt();
        if(seasonId == 0) seasonId = DateTime.Now.Month < 9 ? seasonId = DateTime.Now.Year : DateTime.Now.Year+1;   //The current season is the year that the season ends
        var take = Input.OptionToInt("take", 25);
        var nation = GetOptionValue("nation").ToLower();
        var name = GetOptionValue("name").ToLower();
        if (!string.IsNullOrEmpty(nation) || !string.IsNullOrEmpty(name)) take = 2000;
        var stats = SeasonsDb.SeasonStats.First(s => s.SeasonId == seasonId).Data.Take(take).ToArray();
        var pointsTable = new List<PointTableItem>();
        for (int i= 0; i < stats.Length; i++)
        {
            var playerStat = stats[i];
            var player = PlayersDb.People.FirstOrDefault(p => p.Id == playerStat.PlayerId) ?? new Player{FullName = "MISSING"};
            var pointTableItem = new PointTableItem { Place = i + 1, Nationality = player.Nationality, GamesPlayed = playerStat.GamesPlayed, Assists = playerStat.Assists, FullName = playerStat.SkaterFullName, Points = playerStat.Points, Goals = playerStat.Goals, PointsPerGame = playerStat.PointsPerGame, TeamAbbrevs = playerStat.TeamAbbrevs };
            if(!string.IsNullOrEmpty(name) && !player.FullName.ToLower().Contains(name)) continue;
            if(!string.IsNullOrEmpty(nation) && !player.Nationality.ToLower().Contains(nation)) continue;
            pointsTable.Add(pointTableItem);
        }
        ConsoleTableService.RenderTable(pointsTable, this);
        WriteSuccessLine($"Count: {pointsTable.Count}");
        return Ok();
    }
}