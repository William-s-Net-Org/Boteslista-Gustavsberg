using Gustavsberg.Domain;
namespace Gustavsberg.WinUI;

public static class Fixtures
{
    public static IEnumerable<TeamEvent> Load()
    {
        const string club="https://www.gustavsbergsbandy.se/match/?ID=269134";
        const string series="https://bandygrytan.se/#/competitions/2026/fx_1181126-fx_3413337-/";
        return new[]
        {
            Match("2026-09-12","15:30","Katrineholm U – Gustavsberg","Träningsmatch · Kronfågel Arena",club),
            Match("2026-09-27","18:30","Helenelund – Gustavsberg","Träningsmatch · Serwenthallen",club),
            Match("2026-10-03","10:30","Derby – Gustavsberg","Träningsmatch · Kronfågel Arena",club),
            Match("2026-10-18","13:45","Gustavsberg – Västanfors","Träningsmatch · Serwenthallen",club),
            Match("2026-10-25","13:45","UNIK – Gustavsberg","Träningsmatch · Serwenthallen",club),
            Match("2026-11-01","15:00","IFK Rättvik Bandyklubb U – Gustavsberg","Division 1 Herr Nord",series+"fx_32756040"),
            Match("2026-11-08","17:30","UNIK – Gustavsberg","Division 1 Herr Nord",series+"fx_32756080"),
            Match("2026-11-21","14:00","Västanfors IF Bandyklubb – Gustavsberg","Division 1 Herr Nord",series+"fx_32756047"),
            Match("2026-11-29","14:00","Gustavsberg – Broberg/Söderhamn BIF U","Division 1 Herr Nord",series+"fx_32756051"),
            Match("2026-12-06","14:00","Gustavsberg – SIF Norrtelje","Division 1 Herr Nord",series+"fx_32756086"),
            Match("2026-12-13","14:00","Gustavsberg – Ljusdals BK U","Division 1 Herr Nord",series+"fx_32756087"),
            Match("2026-12-19","13:30","Helenelunds IK – Gustavsberg","Division 1 Herr Nord",series+"fx_32756057"),
            Match("2026-12-27","15:00","GT/76 – Gustavsberg","Division 1 Herr Nord",series+"fx_32756061"),
            Match("2026-12-31","13:15","Gustavsberg – Västanfors IF Bandyklubb","Division 1 Herr Nord",series+"fx_32756091"),
            Match("2027-01-17","14:00","Gustavsberg – IFK Rättvik Bandyklubb U","Division 1 Herr Nord",series+"fx_32756096"),
            Match("2027-01-24","14:00","Broberg/Söderhamn BIF U – Gustavsberg","Division 1 Herr Nord",series+"fx_32756070"),
            Match("2027-01-29","19:30","Gustavsberg – GT/76","Division 1 Herr Nord",series+"fx_32756093"),
            Match("2027-02-07","13:00","SIF Norrtelje – Gustavsberg","Division 1 Herr Nord",series+"fx_32756100"),
            Match("2027-02-14","14:00","Gustavsberg – UNIK","Division 1 Herr Nord",series+"fx_32756101"),
            Match("2027-02-21","14:00","Ljusdals BK U – Gustavsberg","Division 1 Herr Nord",series+"fx_32756105")
        };
    }
    private static TeamEvent Match(string date,string? time,string title,string description,string source) =>
        new(Guid.NewGuid(),DateOnly.ParseExact(date,"yyyy-MM-dd"),time is null ? null : TimeOnly.ParseExact(time,"HH:mm"),title,description,source);
}
