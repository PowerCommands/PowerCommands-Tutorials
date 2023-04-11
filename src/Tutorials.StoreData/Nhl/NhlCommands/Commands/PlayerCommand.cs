namespace NhlCommands.Commands;

[PowerCommandTest(         tests: " wayne gretzky")]
[PowerCommandDesign( description: "Search player with filters",
                         options: "nationality",
                         example: "search \"wayne gretzky\"")]
public class PlayerCommand : NhlBaseCommand
{
    public PlayerCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var nameSearch = string.IsNullOrEmpty(Input.SingleQuote) ? Input.SingleArgument : Input.SingleQuote.ToLower();
        var players = DatabaseManager.PlayersDb.People.Where(p => p.FullName.ToLower().Contains(nameSearch));
        foreach (var player in players) WriteLine($"{player.FullName} {player.Nationality} {player.BirthDate} {player.RosterStatus} {player.Rookie}");
        return Ok();
    }
}