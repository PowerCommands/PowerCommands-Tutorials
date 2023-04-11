﻿namespace NhlCommands.DomainObjects;

public class SeasonGoalStats
{
    public string Season { get; set; }
    public int Teams { get; set; }
    public int Goals { get; set; }
    public int Matches { get; set; }
    public decimal GoalsPerGame { get; set; }
}