using NhlCommands.Managers;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Download data from nhl.com",
                         options: "skaters|goalies|standings|drafts|integrity",
                         useAsync: true,
                         example: "//Download new player statistic for current season|download --stats|//Download new player statistic for season 2000|download 2000 --stats")]
public class DownloadCommand : NhlBaseCommand
{
    public DownloadCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        if (HasOption("drafts")) await DownloadDrafts();
        if (HasOption("skaters")) await DownloadSkaterStats();
        if (HasOption("goalies")) await DownloadGoaliesStats();
        if (HasOption("standings")) await DownloadStandings();
        if (HasOption("integrity")) await DownloadMissingPlayers();
        Write(ConfigurationGlobals.Prompt);
        return Ok();
    }
    public async Task DownloadDrafts()
    {
        var startYear = GetSeasonId();
        var downloadManager = new DownloadDraftsManager(DatabaseManager, this);
        await downloadManager.DownloadAsync(startYear);
    }
    public async Task DownloadSkaterStats()
    {
        var seasonId = GetSeasonId();
        var downloadManager = new DownloadSkaterStatsManager(DatabaseManager, this);
        await downloadManager.DownloadAsync(seasonId);
    }

    public async Task DownloadGoaliesStats()
    {
        var downloadManager = new DownloadGoalieStatsManager(DatabaseManager, this);
        await downloadManager.DownloadAsync();
    }

    public async Task DownloadMissingPlayers()
    {
        var downloadManager = new IntegrityCheckManager(DatabaseManager, this);
        await downloadManager.DownloadAsync(0);
    }

    public async Task DownloadStandings()
    {
        var minSavedSeason = DatabaseManager.DraftsDb.DraftYears.Min(d => d.Year);
        
        var startSeason = minSavedSeason == 0 ? DatabaseManager.SeasonsDb.SkaterStats.Min(s => s.SeasonId) : minSavedSeason;
        var stopSeason = GetSeasonId();
        for (int seasonId = startSeason; seasonId < stopSeason+1; seasonId++)
        {
            var downloadManager = new DownloadStandingsManager(DatabaseManager, this);
            await downloadManager.DownloadAsync(seasonId);
        }
    }
}