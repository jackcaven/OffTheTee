namespace GolfTournament.Domain.Enums;

public enum TournamentFormat
{
    Strokeplay,
    Stableford
}

public enum TournamentStatus
{
    Draft,
    Registration,
    Active,
    Completed
}

public enum HandicapSource
{
    Manual,
    WHS,
    CONGU
}

public enum ScoreStatus
{
    Draft,
    Submitted,
    Verified
}

public enum CourseDataSource
{
    Manual,
    API
}

public enum HandicapCalculationMode
{
    Auto,
    Manual
}

public enum RoundStatus
{
    Pending,
    Active,
    Completed
}
