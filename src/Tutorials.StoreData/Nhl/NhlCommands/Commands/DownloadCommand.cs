using NhlCommands.Managers;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Download data from nhl.com",
                         options: "stats|drafts",
                         useAsync: true,
                         example: "//Download new player statistic for current season|download --stats|//Download new player statistic for season 2000|download 2000 --stats")]
public class DownloadCommand : NhlBaseCommand
{
    public DownloadCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        if (HasOption("drafts")) await DownloadDrafts();
        if (HasOption("stats")) await DownloadStats();
        Write(ConfigurationGlobals.Prompt);
        return Ok();
    }
    public async Task DownloadDrafts()
    {
        var startYear = GetSeasonId();
        var downloadManager = new DownloadDraftsManager(DatabaseManager, this);
        await downloadManager.UpdateDraftsDB(startYear);
    }
    public async Task DownloadStats()
    {
        var seasonId = GetSeasonId();
        var downloadManager = new DownloadStatsManager(DatabaseManager, this);
        await downloadManager.DownloadAsyncAsync(seasonId);
    }
}