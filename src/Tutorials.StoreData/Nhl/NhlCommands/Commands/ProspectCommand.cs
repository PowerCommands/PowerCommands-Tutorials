namespace NhlCommands.Commands;


[PowerCommandDesign(description: "Fetch prospect data from NHL api",
                        example: "prospect")]
public class ProspectCommand : NhlBaseCommand
{
    public ProspectCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override RunResult Run()
    {
        var year = Input.FirstArgumentToInt();
        foreach (var prospect in DatabaseManager.ProspectsDb.Prospects.Where(p => p.Id == 0))
        {
            Console.WriteLine($"{prospect.FullName} {prospect.BirthCountry}");
        }
        return Ok();
    }
}