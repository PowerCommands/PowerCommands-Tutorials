using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Fetch draft data from NHL api",
                         options: "!start-year|update",
                        useAsync: true,
                         example: "draft")]
public class DraftCommand : NhlBaseCommand
{
    public DraftCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override async Task<RunResult> RunAsync()
    {
        var startYear = Input.OptionToInt("start-year", 2000);
        if (Input.HasOption("update"))
        {
            var drafts = await GetDrafts(startYear);
            UpdateDraftsDB(drafts);
        }
        else
        {
            foreach (var draftYear in DraftsDb.DraftYears)
            {
                WriteLine($"Draft year count: {draftYear.Drafts.Count}");
                foreach (var draft in draftYear.Drafts)
                {
                    WriteLine($"Rounds count: {draft.Rounds.Length}");
                    foreach (var round in draft.Rounds)
                    {
                        WriteLine($"{round.Round} Picks count: {round.Picks}");
                        foreach (var draftPick in round.Picks)
                        {
                            WriteLine($"{draftPick.Prospect.FullName}");
                        }
                    }
                }
            }
        }
        return Ok();
    }
    private void UpdateDraftsDB(List<DraftYear> draftYears)
    {
        var hasChanges = false;
        foreach (var draftYear in draftYears)
        {
            var existing = DraftsDb.DraftYears.FirstOrDefault(d => d.Year == draftYear.Year);
            if (existing == null)
            {
                hasChanges = true;
                DraftsDb.DraftYears.Remove(existing);
                DraftsDb.DraftYears.Add(draftYear);
            }
        }
        if (hasChanges) SaveDraftsDB();
        WriteSuccessLine("DraftsDB updated");
        Console.Write(ConfigurationGlobals.Prompt);
    }
    private async Task<List<DraftYear>> GetDrafts(int startYear)
    {
        var retVal = new List<DraftYear>();
        for (int year = startYear; year < DateTime.Now.Year; year++)
        {
            var draftYear = await GetDraft(year);
            draftYear.Year = year;
            retVal.Add(draftYear);
            WriteSuccessLine($"Downloaded {draftYear.Drafts.Count} for draft year {year} OK");
        }
        return retVal;
    }
    private async Task<DraftYear> GetDraft(int year)
    {
        try
        {
            var url = $"https://statsapi.web.nhl.com/api/v1/draft/{year}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var draftYear = JsonSerializer.Deserialize<DraftYear>(responseString, options) ?? new DraftYear();
            return draftYear;
        }
        catch (Exception ex)
        {
            WriteFailureLine(ex.Message);
            return new DraftYear();
        }
    }
}