using System.Globalization;
using Gustavsberg.Domain;
namespace Gustavsberg.Application;

public interface ITeamRepository { TeamState Load(); void Save(TeamState state); }

public sealed class TeamService(ITeamRepository repository)
{
    public TeamState State { get; private set; } = repository.Load();
    public IEnumerable<Fine> ActiveFines => State.Fines.Where(f => !f.Deleted).OrderBy(f => f.Number);
    public decimal Total => ActiveFines.Sum(f => f.Amount);
    public decimal Paid => ActiveFines.Where(f => f.PaidAt != null).Sum(f => f.Amount);
    public decimal Outstanding => Total - Paid;
    private void Commit(Action change)
    {
        try { change(); repository.Save(State); }
        catch { State = repository.Load(); throw; }
    }
    private static string Required(string text, string label)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException($"Fyll i {label}.");
        if (text.Trim().Length > 500) throw new ArgumentException($"{label} får vara högst 500 tecken.");
        return text.Trim();
    }
    public void AddPlayer(int number, string first, string last, string position, int age)
    {
        if (number is < 1 or > 99) throw new ArgumentException("Tröjnummer ska vara 1–99.");
        if (age is < 15 or > 100) throw new ArgumentException("Ålder ska vara 15–100.");
        if (State.Players.Any(p => p.Number == number)) throw new ArgumentException("Tröjnumret används redan.");
        var player = new Player(Guid.NewGuid(), number, Required(first,"förnamn"), Required(last,"efternamn"), Required(position,"position"), age);
        Commit(() => State.Players.Add(player));
    }
    public void AddCategory(string reason, string description, decimal amount)
    {
        if (amount <= 0 || amount > 100000 || decimal.Round(amount,2) != amount)
            throw new ArgumentException("Belopp ska vara 0,01–100 000 kr med högst två decimaler.");
        var category = new Category(Guid.NewGuid(), Required(reason,"orsak"), Required(description,"beskrivning"), amount);
        Commit(() => State.Categories.Add(category));
    }
    public void IssueFine(Guid playerId, Guid categoryId)
    {
        var player = State.Players.Single(p => p.Id == playerId);
        var category = State.Categories.Single(c => c.Id == categoryId);
        Commit(() => State.Fines.Add(new Fine(State.NextFineNumber++, player.Id, player.Name,
            category.Reason, category.Description, category.Amount, DateTimeOffset.Now)));
    }
    public void SetPaid(long number, bool paid) => Commit(() =>
    {
        var index = State.Fines.FindIndex(f => f.Number == number && !f.Deleted);
        if (index < 0) throw new ArgumentException("Boten finns inte.");
        State.Fines[index] = State.Fines[index] with { PaidAt = paid ? DateTimeOffset.Now : null };
    });
    public void DeleteFine(long number) => Commit(() =>
    {
        var index = State.Fines.FindIndex(f => f.Number == number && !f.Deleted);
        if (index < 0) throw new ArgumentException("Boten finns inte.");
        State.Fines[index] = State.Fines[index] with { Deleted = true };
    });
    public void SaveEvent(Guid? id, DateOnly date, TimeOnly? time, string title, string description)
    {
        var old = State.Events.SingleOrDefault(e => e.Id == id);
        var item = new TeamEvent(id ?? Guid.NewGuid(), date, time, Required(title,"rubrik"), Required(description,"beskrivning"),old?.Source);
        Commit(() => { State.Events.RemoveAll(e => e.Id == item.Id); State.Events.Add(item); });
    }
    public void DeleteEvent(Guid id) => Commit(() => State.Events.RemoveAll(e => e.Id == id));
    public void ImportFixtures(IEnumerable<TeamEvent> events)
    {
        if (State.FixturesImported) return;
        Commit(() => { State.Events.AddRange(events); State.FixturesImported = true; });
    }
    public static DateOnly WeekStart(int year, int week)
    {
        if (year is < 1900 or > 2100 || week < 1 || week > ISOWeek.GetWeeksInYear(year))
            throw new ArgumentException("Ange giltigt ISO-år och veckonummer.");
        return DateOnly.FromDateTime(ISOWeek.ToDateTime(year,week,DayOfWeek.Monday));
    }
}
