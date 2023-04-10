namespace NhlCommands.Commands;

[PowerCommandTest(         tests: " wayne gretzky")]
[PowerCommandDesign( description: "Search player with filters",
                         options: "undrafted|nationality",
                         example: "search \"wayne gretzky\"")]
public class PlayerCommand : NhlBaseCommand
{
    public PlayerCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var undrafted = HasOption("undrafted");
        var nationality = GetOptionValue("nationality");
        var nameSearch = string.IsNullOrEmpty(Input.SingleQuote) ? Input.SingleArgument : Input.SingleQuote.ToLower();

        var players = DatabaseManager.PlayersDb.People.Where(p => p.Drafted != undrafted && (p.Nationality == nationality || string.IsNullOrEmpty(nationality)) && p.FullName.ToLower().Contains(nameSearch));

        foreach (var player in players) WriteLine($"{player.FullName} {player.Nationality} {player.BirthDate}");
        return Ok();
    }
}