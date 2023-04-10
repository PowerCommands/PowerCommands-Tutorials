using NhlCommands.Contracts;

namespace NhlCommands.DomainObjects.Database;

public class SeasonsDb : IDatabase
{
    public List<Season> SeasonStats { get; set; } = new();
    public DateTime Updated { get; set; }
    public List<string> GetDescriptions() => new() { $"Seasons: {string.Join(',', SeasonStats.OrderBy(s => s.SeasonId).Select(s => s.SeasonId))}" };
}