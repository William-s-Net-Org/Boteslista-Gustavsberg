using Gustavsberg.Application;
using Gustavsberg.Infrastructure;
using Gustavsberg.Domain;

var directory=Path.Combine(Path.GetTempPath(),"BandyChecks",Guid.NewGuid().ToString());
var path=Path.Combine(directory,"test.db");
void Check(bool condition,string name) { if(!condition) throw new Exception(name); Console.WriteLine($"PASS {name}"); }
using(var repository=new SqliteTeamRepository(path))
{
    var service=new TeamService(repository);
    service.AddPlayer(7,"Åke","Öberg","Back",24);
    try { service.AddPlayer(7,"Anna","Andersson","Forward",22); throw new Exception("Duplicate allowed"); } catch(ArgumentException) { }
    Check(service.State.Players.Count==1,"Duplicate shirt number rejected");
    service.AddCategory("Sen ankomst","Mer än fem minuter",50.50m);
    var player=service.State.Players[0]; var category=service.State.Categories[0];
    service.IssueFine(player.Id,category.Id); service.IssueFine(player.Id,category.Id);
    service.SetPaid(1,true);
    Check(service.Total==101m && service.Paid==50.50m && service.Outstanding==50.50m,"Paid fines remain in total");
    service.DeleteFine(2); service.IssueFine(player.Id,category.Id);
    Check(service.ActiveFines.Select(f=>f.Number).SequenceEqual(new long[] {1,3}),"Monotonic numbers after deletion");
    var eventId=Guid.NewGuid();
    service.SaveEvent(eventId,new DateOnly(2026,12,31),null,"Match","Okänd tid");
    service.SaveEvent(eventId,new DateOnly(2027,1,1),new TimeOnly(13,15),"Flyttad match","Ny tid");
    Check(service.State.Events.Count==1 && service.State.Events[0].Date.Year==2027,"Edit event across year boundary");
    Check(TeamService.WeekStart(2026,53)==new DateOnly(2026,12,28),"ISO week across year boundary");
    var fixtures=new[] {new TeamEvent(Guid.NewGuid(),new DateOnly(2026,11,1),null,"Match","Test")};
    service.ImportFixtures(fixtures); service.ImportFixtures(fixtures);
    Check(service.State.Events.Count==2,"Seed only once");
}
using(var repository=new SqliteTeamRepository(path))
{
    var service=new TeamService(repository);
    Check(service.State.Players[0].FirstName=="Åke" && service.Paid==50.50m && service.State.NextFineNumber==4,"SQLite restart and Unicode persistence");
    service.SetPaid(1,false);
    Check(service.Paid==0m && service.Total==101m,"Undo payment preserves total");
}
Directory.Delete(directory,true);
Console.WriteLine("All checks passed.");
