using NhlCommands.DomainObjects;
using System.Text.Json;
using NhlCommands.Contracts;
using NhlCommands.DomainObjects.Database;
using NhlCommands.Managers;

namespace NhlCommands.Commands;

//https://github.com/erunion/sport-api-specifications/tree/master/nhl
public abstract class NhlBaseCommand : CommandBase<PowerCommandsConfiguration>
{
    public static readonly DbManager DatabaseManager = new DbManager(Path.Combine(ConfigurationGlobals.ApplicationDataFolder, "nhl"));

    //private readonly string _playersFileName = "players.json";
    //private readonly string _seasonsFileName = "seasons.json";
    //private readonly string _draftsFileName = "drafts.json";
    //private readonly string _prospectsFileName = "prospects.json";

    //protected readonly string DbPath = Path.Combine(ConfigurationGlobals.ApplicationDataFolder, "nhl");
    //protected readonly PlayersDb PlayersDb;
    //protected readonly SeasonsDb SeasonsDb;
    //protected readonly DraftsDb DraftsDb;
    //protected readonly ProspectsDb ProspectsDb;
    protected NhlBaseCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration)
    {
        //if(!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
        //PlayersDb = StorageService<PlayersDb>.Service.GetObject(Path.Combine(DbPath, _playersFileName));
        //SeasonsDb = StorageService<SeasonsDb>.Service.GetObject(Path.Combine(DbPath, _seasonsFileName));
        //DraftsDb = StorageService<DraftsDb>.Service.GetObject(Path.Combine(DbPath, _draftsFileName));
        //ProspectsDb = StorageService<ProspectsDb>.Service.GetObject(Path.Combine(DbPath, _prospectsFileName));
    }
    //protected void SavePlayersDB() => StorageService<PlayersDb>.Service.StoreObject(PlayersDb, Path.Combine(DbPath, _playersFileName));
    //protected void SaveSeasonsDB()
    //{
    //    //StorageService<SeasonsDb>.Service.StoreObject(SeasonsDb, Path.Combine(DbPath, _seasonsFileName));
    //    var dbManager = new DbManager(DbPath);
    //    dbManager.Save(SeasonsDb);
    //}

    //protected void SaveDraftsDB() => StorageService<DraftsDb>.Service.StoreObject(DraftsDb, Path.Combine(DbPath, _draftsFileName));
    //protected void SaveProspectsDB() => StorageService<ProspectsDb>.Service.StoreObject(ProspectsDb, Path.Combine(DbPath, _prospectsFileName));

    //protected async Task<Player> GetNhlPlayer(Prospect prospect)
    //{
    //    try
    //    {
    //        var player = await GetNhlPlayer(prospect.NhlPlayerId);
    //        player.AmateurTeam = prospect.AmateurTeam;
    //        player.AmateurLeague = prospect.AmateurLeague;
    //        return player;
    //    }
    //    catch (Exception ex)
    //    {
    //        WriteFailureLine(ex.Message);
    //        return new Player();
    //    }
    //}
    //protected async Task<Player> GetNhlPlayer(int playerId)
    //{
    //    try
    //    {
    //        var url = $"https://statsapi.web.nhl.com/api/v1/people/{playerId}";

    //        using var httpClient = new HttpClient();
    //        var response = await httpClient.GetAsync(url);
    //        var responseString = await response.Content.ReadAsStringAsync();
    //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
    //        var players = JsonSerializer.Deserialize<PlayersDb>(responseString, options) ?? new();
    //        var player = players.People.First();
    //        var prospect = ProspectsDb.Prospects.FirstOrDefault(p => p.NhlPlayerId == playerId);
            
    //        if (prospect == null) return player;
            
    //        player.AmateurTeam = prospect.AmateurTeam;
    //        player.AmateurLeague = prospect.AmateurLeague;
    //        player.Drafted = true;
    //        return player;
    //    }
    //    catch (Exception ex)
    //    {
    //        WriteFailureLine(ex.Message);
    //        return new Player();
    //    }
    //}
    //protected DraftPick GetDraftPick(int prospectId)
    //{
    //    foreach (var draftYear in DraftsDb.DraftYears)
    //    {
    //        foreach (var draft in draftYear.Drafts)
    //        {
    //            foreach (var round in draft.Rounds)
    //            {
    //                foreach (var draftPick in round.Picks)
    //                {
    //                    if (draftPick.Prospect.Id == prospectId) return draftPick;
    //                }
    //            }
    //        }
    //    }
    //    return new DraftPick();
    //}
    protected int GetSeasonId()
    {
        var seasonId = Input.FirstArgumentToInt();
        if(seasonId == 0) seasonId = DateTime.Now.Month < 9 ? seasonId = DateTime.Now.Year : DateTime.Now.Year+1;   //The current season is the year that the season ends
        return seasonId;
    }
    protected List<string> GetNations()
    {
        var knownNations = "SWE|FIN|CAN|USA|CZE|SVK|DEU|NOR|DNK|NLD|BLR|CHE|LVA|RUS".Split('|');
        return Input.Arguments.Select(argument => knownNations.FirstOrDefault(n => n == argument)).Where(nation => !string.IsNullOrEmpty(nation)).ToList()!;
    }
    protected void WriteNationsSummary<T>(List<T> nationalities) where T : INationality
    {
        var nations = GetNations();
        if (nations.Count <= 1) return;
        var nationsCount = (from nation in nations let count = nationalities.Count(p => !string.IsNullOrEmpty(p.Nationality) &&  p.Nationality.Equals(nation, StringComparison.CurrentCultureIgnoreCase)) select new NationCount { Count = count, Nation = nation }).ToList();
        ConsoleTableService.RenderTable(nationsCount, this);
    }
    private class NationCount { public string? Nation { get; init; } public int Count { get; init; }}
}