namespace NhlCommands.DomainObjects;

public class Season
{
    public List<PlayerStat> Data { get; set; }
    public int Total { get; set; }
    public int SeasonId { get; set; }
    public DateTime Updated { get; set; }
}