using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

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
}