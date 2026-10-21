namespace Gustavsberg.Domain;

public record Player(Guid Id, int Number, string FirstName, string LastName, string Position, int Age)
{
    public string Name => $"#{Number} {FirstName} {LastName}";
}
public record Category(Guid Id, string Reason, string Description, decimal Amount)
{
    public string Name => $"{Reason} · {Amount:0.00} kr";
}
// Snapshot values keep history stable when a category or player changes later.
public record Fine(long Number, Guid PlayerId, string PlayerName, string Reason,
    string Description, decimal Amount, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt = null, bool Deleted = false);
public record TeamEvent(Guid Id, DateOnly Date, TimeOnly? Time, string Title, string Description, string? Source = null);
public record TeamState
{
    public List<Player> Players { get; init; } = [];
    public List<Category> Categories { get; init; } = [];
    public List<Fine> Fines { get; init; } = [];
    public List<TeamEvent> Events { get; init; } = [];
    public long NextFineNumber { get; set; } = 1;
    public bool FixturesImported { get; set; }
}
