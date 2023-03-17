using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;


[PowerCommandDesign( description: "Fetch prospect data from NHL api",
                        useAsync: true,
                         example: "prospect")]
public class ProspectCommand : NhlBaseCommand
{
    public ProspectCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }
    public override async Task<RunResult> RunAsync()
    {
        var updatedProspects = await GetUpdateProspects();
        if (updatedProspects.Count > 0)
        {
            ProspectsDb.Prospects.AddRange(updatedProspects);
            SaveProspectsDB();
            WriteSuccessLine("Updated prospects saved!");
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
                        if (ProspectsDb.Prospects.Any(p => p.Id == draftPick.Prospect.Id)) continue;
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
            var prospect = JsonSerializer.Deserialize<ProspectsDb>(responseString, options) ?? new ();
            return prospect.Prospects.First();
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new Prospect();
        }
    }
}