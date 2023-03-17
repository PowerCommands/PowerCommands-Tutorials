namespace NhlCommands.DomainObjects;

public class Prospect
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Link { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string BirthDate { get; set; }
    public string BirthCity { get; set; }
    public string BirthCountry { get; set; }
    public string Height { get; set; }
    public int Weight { get; set; }
    public string ShootsCatches { get; set; }
    public ProspectPrimaryPosition PrimaryPosition { get; set; }
    public int NhlPlayerId { get; set; }
    public string DraftStatus { get; set; }
    public ProspectCategory ProspectCategory { get; set; }
    public ProspectAmateurTeam AmateurTeam { get; set; }
    public ProspectAmateurLeague AmateurLeague { get; set; }
}