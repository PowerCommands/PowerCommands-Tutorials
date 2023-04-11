using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Show points statistic for a specific season or current season, default top count is 25",
                         options: "name|top|goals|assists|points-per-game|forward|defense|rookie",
                     suggestions: "SWE|FIN|CAN|USA|CZE|SVK|DEU|AUS|CHE|SVN|NOR|DNK|NLD|BLR|LVA|FRA|AUT|GBR|UKR|HRV|LTU|KAZ|POL|NGA|BHS|ITA|RUS",
                         example: "//Show points stats for current season top 25 (default)|season|//Show points stats for season 2010, show first top 100|season 2010 --top 100|//Show points stats for all swedish players for current season|season --nation swe|//Compare swedish and finnish players for the current season in the top 100|season SWE FIN --top 100|//Compare swedish and finnish players for season 2016/2017 in the top 100|season 2017 SWE FIN --top 100|//Show rookie points stats for current season top 25 (default)|season --rookie|//Show defense men points stats for current season top 25 (default)|season --defense|//Show stats for current season top 25 goal scorer (default)|season --goals|//Show stats for current season top 25 assists (default)|season --assists|//Show stats for current season top 25 points per game (default)|season --goals-per-game")]
public class SeasonCommand : NhlBaseCommand
{
    public SeasonCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override RunResult Run()
    {
        var seasonId = GetSeasonId();
        var nations = GetNations();
        var take = Input.OptionToInt("top", nations.Count > 0 || HasOption("rookie") ? 2000 : 25);
        var name = GetOptionValue("name").ToLower();
        var season = DatabaseManager.SeasonsDb.SeasonStats.First(s => s.SeasonId == seasonId);
        var stats = season.Data.Take(take).ToArray();

        //filter
        if(HasOption("forward")) stats = season.Data.Where(d => d.PositionCode != "D").Take(take).ToArray();
        if(HasOption("defense")) stats = season.Data.Where(d => d.PositionCode == "D").Take(take).ToArray();
        

        //Order by other then points
        if(HasOption("goals")) stats = season.Data.OrderByDescending(d => d.Goals).Take(take).ToArray();
        else if(HasOption("points-per-game")) stats = season.Data.OrderByDescending(d => d.PointsPerGame).Take(take).ToArray();
        
        var pointsTable = new List<PointTableItem>();
        for (int i= 0; i < stats.Length; i++)
        {
            var playerStat = stats[i];
            var player = DatabaseManager.PlayersDb.People.FirstOrDefault(p => p.Id == playerStat.PlayerId) ?? new Player{FullName = "MISSING"};
            var pointTableItem = new PointTableItem { Place = i + 1, Nationality = player.Nationality, GamesPlayed = playerStat.GamesPlayed, Assists = playerStat.Assists, FullName = playerStat.SkaterFullName, Points = playerStat.Points, Goals = playerStat.Goals, PointsPerGame = playerStat.PointsPerGame, TeamAbbrevs = playerStat.TeamAbbrevs, PositionCode = playerStat.PositionCode};
            if(HasOption("rookie") && !player.Rookie) continue;
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