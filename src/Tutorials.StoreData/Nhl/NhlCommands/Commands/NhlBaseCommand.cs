using NhlCommands.DomainObjects;

namespace NhlCommands.Commands;

public abstract class NhlBaseCommand : CommandBase<PowerCommandsConfiguration>
{
    private readonly string _playersFileName = "players.json";
    private readonly string _seasonsFileName = "seasons.json";

    protected readonly string DbPath = Path.Combine(ConfigurationGlobals.ApplicationDataFolder, "nhl");
    protected readonly PlayersDb PlayersDb;
    protected readonly SeasonsDb SeasonsDb;
    protected NhlBaseCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration)
    {
        if(!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
        PlayersDb = StorageService<PlayersDb>.Service.GetObject(Path.Combine(DbPath, _playersFileName));
        SeasonsDb = StorageService<SeasonsDb>.Service.GetObject(Path.Combine(DbPath, _seasonsFileName));
    }
    protected void SavePlayersDB() => StorageService<PlayersDb>.Service.StoreObject(PlayersDb, Path.Combine(DbPath, _playersFileName));
    protected void SaveSeasonsDB() => StorageService<SeasonsDb>.Service.StoreObject(SeasonsDb, Path.Combine(DbPath, _seasonsFileName));

}