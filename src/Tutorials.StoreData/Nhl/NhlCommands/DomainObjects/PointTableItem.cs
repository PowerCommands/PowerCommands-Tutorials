﻿using NhlCommands.Contracts;

namespace NhlCommands.DomainObjects;

public class PointTableItem : INationality
{
    public int Place { get; set; }
    public string FullName { get; set; }
    public string TeamAbbrevs { get; set; } = string.Empty;
    public string Nationality { get; set; }
    public int GamesPlayed { get; set; }
    public int Points { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public float? PointsPerGame { get; set; }
}