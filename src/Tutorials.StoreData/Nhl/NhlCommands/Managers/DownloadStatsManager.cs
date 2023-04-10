﻿using System.Text.Json;
using NhlCommands.DomainObjects;

namespace NhlCommands.Managers;

public class DownloadStatsManager : DownloadBaseManager
{
    public DownloadStatsManager(DbManager dbManager, IConsoleWriter writer) : base(dbManager, writer) { }

    public async Task DownloadAsyncAsync(int seasonId)
    {
        var seasonStats = await GetSeason(seasonId);
        if(seasonStats.Total > 0) UpdateSeason(seasonStats, seasonId);
        var rank = 1;
        var newNhlPlayerFound = false;
        foreach (var player in seasonStats.Data)
        {
            var nationality = "";
            var existing = DBManager.PlayersDb.People.FirstOrDefault(p => p.Id == player.PlayerId);
            if (existing == null)
            {
                var nhlPlayer = await GetNhlPlayer(player.PlayerId);
                if (nhlPlayer.Id > 0)
                {
                    nationality = $"{nhlPlayer.Nationality}";
                    DBManager.PlayersDb.People.Add(nhlPlayer);
                    newNhlPlayerFound = true;
                }
            }
            else nationality = existing.Nationality;
            
            Writer.WriteLine($"{rank}. {player.SkaterFullName} {nationality} {player.Points} ({player.Goals}+{player.Assists}) {player.TeamAbbrevs}");
            rank++;
        }
        Writer.WriteSuccess($"{seasonStats.Data.Count} players fetched from nhl.com\n");
        if (newNhlPlayerFound)
        {
            DBManager.PlayersDb.Updated = DateTime.Now;
            DBManager.Save(DBManager.PlayersDb);
            Writer.WriteSuccess("New players saved to file.\n");
        }
    }

    private async Task<Season> GetSeason(int seasonId)
    {
        var retVal = new Season { SeasonId = seasonId, Data = new()};
        var start = 1;
        var maxIterations = 15;
        
        for(int  i = 0; i < maxIterations; i++)
        {
            var moreSeasonPlayers = await GetSeasonData(seasonId, start);
            Writer.WriteLine($"Fetching players from rank {start} to {retVal.Data.Count + moreSeasonPlayers.Data.Count}...\n");
            retVal.Data.AddRange(moreSeasonPlayers.Data);
            retVal.Total = moreSeasonPlayers.Total;
            var fetchMorePlayers = moreSeasonPlayers.Data.Count == 100;
            if(!fetchMorePlayers)  break;
            start += 100;
            Thread.Sleep(1000);
        }
        return retVal;
    }

    private async Task<Season> GetSeasonData(int seasonId, int start)
    {
        try
        {
            var season = $"{seasonId - 1}{seasonId}";
            var url = $"https://api.nhle.com/stats/rest/en/skater/summary?isAggregate=false&isGame=false&sort=[{{%22property%22:%22points%22,%22direction%22:%22DESC%22}},{{%22property%22:%22goals%22,%22direction%22:%22DESC%22}},{{%22property%22:%22assists%22,%22direction%22:%22DESC%22}},{{%22property%22:%22playerId%22,%22direction%22:%22ASC%22}}]&start={start - 1}&limit=100&factCayenneExp=gamesPlayed%3E=1&cayenneExp=gameTypeId=2%20and%20seasonId%3C={season}%20and%20seasonId%3E={season}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var stats = JsonSerializer.Deserialize<Season>(responseString, options) ?? new Season();
            return stats;
        }
        catch (Exception ex)
        {
            Writer.WriteFailure($"{ex.Message}\n");
            return new Season();
        }
    }
    private void UpdateSeason(Season season, int seasonId)
    {
        season.SeasonId = seasonId;
        var existing = DBManager.SeasonsDb.SeasonStats.FirstOrDefault(s => s.SeasonId == season.SeasonId);
        if (existing != null) DBManager.SeasonsDb.SeasonStats.Remove(existing);
        season.Updated = DateTime.Now;
        DBManager.SeasonsDb.SeasonStats.Add(season);
        DBManager.Save(DBManager.SeasonsDb);
        Writer.WriteSuccess($"Season {seasonId - 1}{seasonId} saved!\n");
    }
}