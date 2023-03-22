using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

//https://github.com/erunion/sport-api-specifications/tree/master/nhl
public abstract class NhlBaseCommand : CommandBase<PowerCommandsConfiguration>
{
    private readonly string _playersFileName = "players.json";
    private readonly string _seasonsFileName = "seasons.json";
    private readonly string _draftsFileName = "drafts.json";
    private readonly string _prospectsFileName = "prospects.json";

    protected readonly string DbPath = Path.Combine(ConfigurationGlobals.ApplicationDataFolder, "nhl");
    protected readonly PlayersDb PlayersDb;
    protected readonly SeasonsDb SeasonsDb;
    protected readonly DraftsDb DraftsDb;
    protected readonly ProspectsDb ProspectsDb;
    protected NhlBaseCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration)
    {
        if(!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
        PlayersDb = StorageService<PlayersDb>.Service.GetObject(Path.Combine(DbPath, _playersFileName));
        SeasonsDb = StorageService<SeasonsDb>.Service.GetObject(Path.Combine(DbPath, _seasonsFileName));
        DraftsDb = StorageService<DraftsDb>.Service.GetObject(Path.Combine(DbPath, _draftsFileName));
        ProspectsDb = StorageService<ProspectsDb>.Service.GetObject(Path.Combine(DbPath, _prospectsFileName));
    }
    protected void SavePlayersDB() => StorageService<PlayersDb>.Service.StoreObject(PlayersDb, Path.Combine(DbPath, _playersFileName));
    protected void SaveSeasonsDB() => StorageService<SeasonsDb>.Service.StoreObject(SeasonsDb, Path.Combine(DbPath, _seasonsFileName));
    protected void SaveDraftsDB() => StorageService<DraftsDb>.Service.StoreObject(DraftsDb, Path.Combine(DbPath, _draftsFileName));
    protected void SaveProspectsDB() => StorageService<ProspectsDb>.Service.StoreObject(ProspectsDb, Path.Combine(DbPath, _prospectsFileName));

    protected async Task<Player> GetNhlPlayer(Prospect prospect)
    {
        try
        {
            var player = await GetNhlPlayer(prospect.NhlPlayerId);
            player.AmateurTeam = prospect.AmateurTeam;
            player.AmateurLeague = prospect.AmateurLeague;
            return player;
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new Player();
        }
    }
    protected async Task<Player> GetNhlPlayer(int playerId)
    {
        try
        {
            var url = $"https://statsapi.web.nhl.com/api/v1/people/{playerId}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var players = JsonSerializer.Deserialize<PlayersDb>(responseString, options) ?? new();
            var player = players.People.First();
            var prospect = ProspectsDb.Prospects.FirstOrDefault(p => p.NhlPlayerId == playerId);
            
            if (prospect == null) return player;
            
            player.AmateurTeam = prospect.AmateurTeam;
            player.AmateurLeague = prospect.AmateurLeague;
            player.Drafted = true;
            return player;
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new Player();
        }
    }
    protected DraftPick GetDraftPick(int prospectId)
    {
        foreach (var draftYear in DraftsDb.DraftYears)
        {
            foreach (var draft in draftYear.Drafts)
            {
                foreach (var round in draft.Rounds)
                {
                    foreach (var draftPick in round.Picks)
                    {
                        if (draftPick.Prospect.Id == prospectId) return draftPick;
                    }
                }
            }
        }
        return new DraftPick();
    }
}