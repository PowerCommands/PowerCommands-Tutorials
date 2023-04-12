using NhlCommands.Contracts;

namespace NhlCommands.Commands;

[PowerCommandTest(         tests: " ")]
[PowerCommandDesign( description: "Shows metadata about the database",
                         options: "details",
                     suggestions: "season|drafts",
                         example: "db")]
public class DbCommand : NhlBaseCommand
{
    public DbCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        WriteMetaData(DatabaseManager.PlayersDb, "Players");
        WriteLine("--------------------------------------------------------------------------------------------------------------------------");
        WriteLine("");
        WriteMetaData(DatabaseManager.SeasonsDb, "Season individual players statistic");
        if(HasOption("details") && GetOptionValue("details") == "season") WriteSeasonDetails();
        WriteLine("--------------------------------------------------------------------------------------------------------------------------");
        WriteLine("");
        WriteMetaData(DatabaseManager.StandingsDb, "Season team standings statistic");
        WriteLine("--------------------------------------------------------------------------------------------------------------------------");
        WriteLine("");
        WriteMetaData(DatabaseManager.DraftsDb, "Drafts");
        if(HasOption("details") && GetOptionValue("details") == "drafts") WriteDraftDetails();
        WriteLine("--------------------------------------------------------------------------------------------------------------------------");
        WriteLine("");
        WriteMetaData(DatabaseManager.ProspectsDb, "Prospects");
        return Ok();
    }

    private void WriteMetaData<T>(T database, string databaseName) where T : IDatabase
    {
        WriteHeadLine(databaseName);
        foreach (var description in database.GetDescriptions()) WriteLine(description);
        WriteHeadLine($"Last updated:{database.Updated}");
        WriteCodeExample("File size:", DatabaseManager.GetFileSize(database.GetType()));
    }

    private void WriteDraftDetails()
    {
        var draftCounts = new List<YearCount>();
        foreach (var draftYear in DatabaseManager.DraftsDb.DraftYears.OrderByDescending(y => y.Year))
        {
            var yearCount = 0;
            foreach (var draft in draftYear.Drafts)
            {
                foreach (var round in draft.Rounds)
                {
                    yearCount += round.Picks.Length;
                }
            }
            draftCounts.Add(new YearCount{Year = draftYear.Year, Count = yearCount});
        }
        ConsoleTableService.RenderTable(draftCounts, this);
    }

    private void WriteSeasonDetails()
    {
        WriteHeadLine("Skaters");
        var seasonCounts = DatabaseManager.SeasonsDb.SkaterStats.OrderByDescending(s => s.SeasonId).Select(season => new YearCount { Year = season.SeasonId, Count = season.Data.Count }).ToList();
        ConsoleTableService.RenderTable(seasonCounts, this);

        WriteHeadLine("Goalies");
        seasonCounts = DatabaseManager.SeasonsDb.GoalieStats.OrderByDescending(s => s.SeasonId).Select(season => new YearCount { Year = season.SeasonId, Count = season.Data.Count }).ToList();
        ConsoleTableService.RenderTable(seasonCounts, this);
    }

    class YearCount{ public int Year { get; set; } public int Count { get; set; }}
}