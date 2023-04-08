using NhlCommands.DomainObjects;
using System.Text.Json;

namespace NhlCommands.Commands;

[PowerCommandDesign( description: "Fetch draft data from NHL api to build up your base data or just display drafts from the local database file.",
                         options: "year|take|countries|!start-year|update",
                        useAsync: true,
                     suggestions: "SWE|FIN|CAN|USA|CZE|SVK|DEU|NOR|DNK|NLD|BLR|CHE|LVA|RUS",
                         example: "draft")]
public class DraftCommand : NhlBaseCommand
{
    public DraftCommand(string identifier, PowerCommandsConfiguration configuration) : base(identifier, configuration) { }

    public override async Task<RunResult> RunAsync()
    {
        var startYear = Input.OptionToInt("start-year", 2000);
        if (HasOption("update"))
        {
            var drafts = await GetDrafts(startYear);
            UpdateDraftsDB(drafts);
        }
        else
        {
            var draftsCount = 0;
            var year = Input.OptionToInt("year");
            var take = Input.OptionToInt("take", 100000);
            var picks = new List<DraftPick>();
            foreach (var draftYear in DraftsDb.DraftYears.Where(d => d.Year == year || year == 0))
            {
                foreach (var draft in draftYear.Drafts)
                {
                    foreach (var round in draft.Rounds)
                    {
                        foreach (var draftPick in round.Picks)
                        {
                            picks.Add(draftPick);
                        }
                    }
                }
            }
            var countries = GetOptionValue("countries").Split(',');
            var prospects = new List<Prospect>();
            foreach (var draftPick in picks.Take(take))
            {
                var prospect = ProspectsDb.Prospects.FirstOrDefault(p => p.Id == draftPick.Prospect.Id) ?? new Prospect { BirthCity = "?", BirthCountry = "?", AmateurLeague = new ProspectAmateurLeague { Name = "?" }, AmateurTeam = new ProspectAmateurTeam { Name = "?" } };
                prospects.Add(prospect);
                if(countries.Length > 0 && countries.All(c => c != $"{prospect.Nationality}")) continue;
                draftsCount++;
                WriteLine($"{draftPick.Year} {draftPick.Prospect.FullName} {prospect.BirthCity} {prospect.BirthCountry} {prospect.AmateurTeam.Name}  Round:{draftPick.Round} PickOverall: {draftPick.PickOverall}");
            }
            WriteLine($"Total drafts count:{draftsCount}");
            if(countries.Length > 0 )
            {
                foreach (var country in countries)
                {
                    var countryCount = prospects.Count(p => p.BirthCountry == country);
                    WriteLine($"{country}: {countryCount}");
                }
            }
            Console.Write(ConfigurationGlobals.Prompt);
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