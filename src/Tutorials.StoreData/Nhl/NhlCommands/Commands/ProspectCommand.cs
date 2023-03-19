using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;


[PowerCommandDesign( description: "Fetch prospect data from NHL api",
                         options: "update",
                        useAsync: true,
                         example: "prospect")]
public class ProspectCommand : NhlBaseCommand
{
    public ProspectCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var updatedProspects = await GetUpdateProspects();
        if (updatedProspects.Count > 0 && HasOption("update"))
        {
            ProspectsDb.Prospects.AddRange(updatedProspects);
            SaveProspectsDB();
            WriteSuccessLine("Updated prospects saved!");
        }
        else
        {
            Console.Clear();
            var prospectCount = 0;
            foreach (var prospect in ProspectsDb.Prospects)
            {
                prospectCount++;
                WriteLine($"{prospectCount.ToString(),4} {prospect.FullName} {prospect.BirthCountry} {prospect.AmateurTeam.Name}");
            }
        }
        return Ok();
    }

    private async Task<List<Prospect>> GetUpdateProspects()
    {
        var updatedProspects = new List<Prospect>();
        var counter = 0;
        foreach (var draftYears in DraftsDb.DraftYears)
        {
            foreach (var draft in draftYears.Drafts)
            {
                foreach (var round in draft.Rounds)
                {
                    foreach (var draftPick in round.Picks)
                    {
                        counter++;
                        if (ProspectsDb.Prospects.Any(p => p.Id == draftPick.Prospect.Id))
                        {
                            WriteLine("Already exist, skipping...");
                            continue;
                        }
                        var prospect = await GetProspect(draftPick.Prospect.Id);
                        if (prospect.Id > 0)
                        {
                            updatedProspects.Add(prospect);
                            WriteSuccessLine($"{counter} New prospect {prospect.FullName} added OK");
                        }
                    }
                }
            }
        }
        return updatedProspects;
    }
    private async Task<Prospect> GetProspect(int prospectId)
    {
        try
        {
            var url = $"https://statsapi.web.nhl.com/api/v1/draft/prospects/{prospectId}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var prospects = JsonSerializer.Deserialize<ProspectsDb>(responseString, options) ?? new ();
            var prospect = prospects.Prospects.First();
            var player = await GetNhlPlayer(prospect);
            prospect.Nationality = player.Nationality;
            return prospect;
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new Prospect();
        }
    }
}