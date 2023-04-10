using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Show points statistic for a specific season or current season, default top count is 25",
                         options: "name|top",
                     suggestions: "SWE|FIN|CAN|USA|CZE|SVK|DEU|AUS|CHE|SVN|NOR|DNK|NLD|BLR|LVA|FRA|AUT|GBR|UKR|HRV|LTU|KAZ|POL|NGA|BHS|ITA|RUS",
                         example: "Show points stats for current season top 25 (default)|points|//Show points stats for season 2023, show first top 100|points 2023 --top 100|//Show points stats for all swedish players for current season|points --nation swe|//Compare swedish and finnish players for the current season in the top 100|points SWE FIN --top 100|//Compare swedish and finnish players for season 2016/2017 in the top 100|points 2017 SWE FIN --top 100")]
public class PointsCommand : NhlBaseCommand
{
    public PointsCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override RunResult Run()
    {
        var seasonId = GetSeasonId();
        var nations = GetNations();
        var take = Input.OptionToInt("top", nations.Count > 0 ? 2000 : 25);
        var name = GetOptionValue("name").ToLower();
        var season = DatabaseManager.SeasonsDb.SeasonStats.First(s => s.SeasonId == seasonId);
        var stats = season.Data.Take(take).ToArray();

        var pointsTable = new List<PointTableItem>();
        for (int i= 0; i < stats.Length; i++)
        {
            var playerStat = stats[i];
            var player = DatabaseManager.PlayersDb.People.FirstOrDefault(p => p.Id == playerStat.PlayerId) ?? new Player{FullName = "MISSING"};
            var pointTableItem = new PointTableItem { Place = i + 1, Nationality = player.Nationality, GamesPlayed = playerStat.GamesPlayed, Assists = playerStat.Assists, FullName = playerStat.SkaterFullName, Points = playerStat.Points, Goals = playerStat.Goals, PointsPerGame = playerStat.PointsPerGame, TeamAbbrevs = playerStat.TeamAbbrevs };
            if(!string.IsNullOrEmpty(name) && !player.FullName.ToLower().Contains(name)) continue;
            
            if(nations.Count == 0) pointsTable.Add(pointTableItem);
            else if(nations.Count > 0 && nations.Any(n => string.Equals(player.Nationality, n, StringComparison.CurrentCultureIgnoreCase))) pointsTable.Add(pointTableItem);
        }
        ConsoleTableService.RenderTable(pointsTable, this);

        WriteNationsSummary(pointsTable);
        
        WriteSuccessLine($"Total count: {pointsTable.Count}");
        WriteSuccessLine($"Last updated: {season.Updated}");
        return Ok();
    }
}