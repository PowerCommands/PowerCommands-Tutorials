using NhlCommands.Managers;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Download data from nhl.com",
                         options: "stats|standings|drafts",
                         useAsync: true,
                         example: "//Download new player statistic for current season|download --stats|//Download new player statistic for season 2000|download 2000 --stats")]
public class DownloadCommand : NhlBaseCommand
{
    public DownloadCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        if (HasOption("drafts")) await DownloadDrafts();
        if (HasOption("stats")) await DownloadStats();
        if (HasOption("standings")) await DownloadStandings();
        Write(ConfigurationGlobals.Prompt);
        return Ok();
    }
    public async Task DownloadDrafts()
    {
        var startYear = GetSeasonId();
        var downloadManager = new DownloadDraftsManager(DatabaseManager, this);
        await downloadManager.DownloadAsync(startYear);
    }
    public async Task DownloadStats()
    {
        var seasonId = GetSeasonId();
        var downloadManager = new DownloadStatsManager(DatabaseManager, this);
        await downloadManager.DownloadAsync(seasonId);
    }

    public async Task DownloadStandings()
    {
        var minSavedSeason = DatabaseManager.DraftsDb.DraftYears.Min(d => d.Year);
        
        var startSeason = minSavedSeason == 0 ? DatabaseManager.SeasonsDb.SeasonStats.Min(s => s.SeasonId) : minSavedSeason;
        var stopSeason = GetSeasonId();
        for (int seasonId = startSeason; seasonId < stopSeason+1; seasonId++)
        {
            var downloadManager = new DownloadStandingsManager(DatabaseManager, this);
            await downloadManager.DownloadAsync(seasonId);
        }
    }
}