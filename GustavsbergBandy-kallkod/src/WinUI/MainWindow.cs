using System.Globalization;
using Gustavsberg.Application;
using Gustavsberg.Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Gustavsberg.WinUI;

// Thin presentation layer: use cases and persistence live in separate projects.
public sealed class MainWindow : Window
{
    private readonly TeamService service;
    private readonly NavigationView navigation = new() { IsSettingsVisible = false, PaneDisplayMode = NavigationViewPaneDisplayMode.Left, OpenPaneLength = 240 };
    private readonly StackPanel page = new() { Spacing = 16, Padding = new Thickness(32), MaxWidth = 1100, HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly InfoBar feedback = new() { IsClosable = true };
    private int selectedPage;
    private DateOnly start = DateOnly.FromDateTime(DateTime.Today).AddDays(-((int)DateTime.Today.DayOfWeek + 6) % 7);
    private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");
    public MainWindow(TeamService service)
    {
        this.service = service;
        Title = "Gustavsbergs Bandy · Böteslista A-lag";
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200,850));
        navigation.PaneTitle = "GUSTAVSBERG\nA-LAG · 2026/27";
        string[] labels = ["Startsida", "Truppen", "Böteskategorier", "Böteslista", "Sammanställning", "Tvåveckorsschema"];
        string[] icons = ["\uE80F","\uE716","\uE8D4","\uE8A5","\uE9D9","\uE787"];
        for(var i=0;i<labels.Length;i++) navigation.MenuItems.Add(new NavigationViewItem { Content=labels[i], Tag=i, Icon=new FontIcon { Glyph=icons[i] } });
        navigation.Content = new ScrollViewer { Content=page, HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled };
        navigation.SelectionChanged += (_, e) => { if(e.SelectedItem is NavigationViewItem item) { selectedPage=(int)item.Tag; Render(); } };
        Content = navigation;
        navigation.SelectedItem = navigation.MenuItems[0];
    }
    private static TextBlock Text(string text, double size=16) => new() { Text=text, FontSize=size, TextWrapping=TextWrapping.Wrap };
    private static TextBox Field(string label,string value="") => new() { Header=label, Text=value, MaxLength=500, MinWidth=150 };
    private static StackPanel Row(params UIElement[] items)
    {
        var row = new StackPanel { Orientation=Orientation.Horizontal, Spacing=12 };
        foreach(var item in items) row.Children.Add(item);
        return row;
    }
    private Button Button(string text,Action action)
    {
        var button = new Button { Content=text };
        button.Click += (_,_) => Run(action);
        return button;
    }
    private void Run(Action action)
    {
        try { action(); }
        catch(Exception error) { feedback.Title="Kunde inte spara"; feedback.Message=error.Message; feedback.Severity=InfoBarSeverity.Error; feedback.IsOpen=true; }
    }
    private static Border Card(UIElement child) => new() { Child=child, Padding=new Thickness(20), CornerRadius=new CornerRadius(12), Background=new SolidColorBrush(Windows.UI.Color.FromArgb(18,128,128,128)) };
    private void Render()
    {
        page.Children.Clear(); page.Children.Add(feedback); feedback.IsOpen=false;
        switch(selectedPage) { case 0: Home(); break; case 1: Players(); break; case 2: Categories(); break; case 3: Fines(); break; case 4: Summary(); break; case 5: Schedule(); break; }
    }
    private void Home()
    {
        var logo = new Image { Source=new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory,"Assets","club.webp"))), Height=160, HorizontalAlignment=HorizontalAlignment.Left };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(logo,"Gustavsbergs IF Bandyklubb");
        page.Children.Add(logo);
        page.Children.Add(Text("Böteslista A-lag",40));
        page.Children.Add(Text("Välkommen till Gustavsbergs Bandy.\nTruppen, lagkassan och de kommande två veckorna – samlat på ett ställe.",20));
        page.Children.Add(Card(Text($"{service.State.Players.Count} spelare     ·     {service.ActiveFines.Count()} böter     ·     {service.Outstanding.ToString("C",Swedish)} kvar att betala",22)));
        page.Children.Add(Row(Button("Registrera en bot",()=> { selectedPage=3; navigation.SelectedItem=navigation.MenuItems[3]; Render(); }),Button("Visa schemat",()=> { selectedPage=5; navigation.SelectedItem=navigation.MenuItems[5]; Render(); })));
        page.Children.Add(Text("Informationen sparas automatiskt på den här datorn. Gemensam synkronisering för laget ingår ännu inte.",14));
    }
    private void Players()
    {
        page.Children.Add(Text("Truppen",32));
        var number=Field("Tröjnummer"); var first=Field("Förnamn"); var last=Field("Efternamn"); var position=Field("Position"); var age=Field("Ålder");
        foreach(var field in new[] { number,first,last,position,age }) page.Children.Add(field);
        page.Children.Add(Button("Lägg till spelare",()=> { service.AddPlayer(Integer(number),first.Text,last.Text,position.Text,Integer(age)); Render(); }));
        if(service.State.Players.Count==0) page.Children.Add(Text("Truppen är tom. Lägg till lagets första spelare ovan."));
        foreach(var player in service.State.Players.OrderBy(p=>p.Number)) page.Children.Add(Card(Text($"{player.Name}     ·     {player.Position}     ·     {player.Age} år")));
    }
    private static int Integer(TextBox field) => int.TryParse(field.Text,out var value) ? value : throw new ArgumentException($"{field.Header} måste vara ett heltal.");
    private void Categories()
    {
        page.Children.Add(Text("Böteskategorier",32));
        var reason=Field("Orsak"); var description=Field("Kort beskrivning"); var amount=Field("Kostnad (kr)");
        page.Children.Add(reason); page.Children.Add(description); page.Children.Add(amount);
        page.Children.Add(Button("Lägg till kategori",()=> {
            if(!decimal.TryParse(amount.Text,NumberStyles.AllowDecimalPoint,Swedish,out var value)) throw new ArgumentException("Ange belopp i kronor, t.ex. 50,50.");
            service.AddCategory(reason.Text,description.Text,value); Render();
        }));
        if(service.State.Categories.Count==0) page.Children.Add(Text("Inga kategorier ännu. Bestäm orsak och belopp ovan."));
        foreach(var category in service.State.Categories) page.Children.Add(Card(Text($"{category.Name}\n{category.Description}")));
    }
    private void Fines()
    {
        page.Children.Add(Text("Böteslista",32));
        page.Children.Add(Text("Löpnummer följer registreringsordningen. Betalda böter finns kvar."));
        var players=new ComboBox { Header="Spelare", ItemsSource=service.State.Players.OrderBy(p=>p.Number).ToList(),DisplayMemberPath="Name",MinWidth=260 };
        var categories=new ComboBox { Header="Kategori",ItemsSource=service.State.Categories,DisplayMemberPath="Name",MinWidth=260 };
        page.Children.Add(Row(players,categories));
        page.Children.Add(Button("Registrera bot",()=> {
            if(players.SelectedItem is not Player player || categories.SelectedItem is not Category category) throw new ArgumentException("Välj spelare och kategori. Lägg till dem först om listorna är tomma.");
            service.IssueFine(player.Id,category.Id); Render();
        }));
        if(!service.ActiveFines.Any()) page.Children.Add(Text("Inga böter registrerade."));
        foreach(var fine in service.ActiveFines)
        {
            var content=new StackPanel { Spacing=10 };
            content.Children.Add(Text($"#{fine.Number:D4}   {fine.PlayerName}   ·   {fine.Amount.ToString("C",Swedish)}",20));
            content.Children.Add(Text($"{fine.Reason} · {fine.Description}\nRegistrerad {fine.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm} · {(fine.PaidAt.HasValue ? $"Betald {fine.PaidAt:yyyy-MM-dd}" : "Obetald")}"));
            var remove = new Button { Content="Ta bort" };
            remove.Click += async (_,_) => {
                var dialog=new ContentDialog { Title=$"Ta bort bot #{fine.Number}?",Content="Boten tas bort ur listan och totalsumman. Löpnumret återanvänds inte.",PrimaryButtonText="Ta bort",CloseButtonText="Avbryt",DefaultButton=ContentDialogButton.Close,XamlRoot=page.XamlRoot };
                if(await dialog.ShowAsync()==ContentDialogResult.Primary) Run(()=> { service.DeleteFine(fine.Number); Render(); });
            };
            content.Children.Add(Row(Button(fine.PaidAt is null ? "Markera betald" : "Markera obetald",()=> { service.SetPaid(fine.Number,fine.PaidAt is null); Render(); }),remove));
            page.Children.Add(Card(content));
        }
    }
    private void Summary()
    {
        page.Children.Add(Text("Sammanställning",32));
        page.Children.Add(Card(Text($"Totalt: {service.Total.ToString("C",Swedish)}\nBetalt: {service.Paid.ToString("C",Swedish)}\nKvar att betala: {service.Outstanding.ToString("C",Swedish)}",26)));
        page.Children.Add(Text("Betalning ändrar saldot men tar aldrig bort en bot. Borttagna böter räknas inte med."));
        foreach(var player in service.State.Players.OrderBy(p=>p.Number))
        {
            var fines=service.ActiveFines.Where(f=>f.PlayerId==player.Id).ToList();
            page.Children.Add(Card(Text($"{player.Name}\nTotalt {fines.Sum(f=>f.Amount):0.00} kr  ·  Betalt {fines.Where(f=>f.PaidAt!=null).Sum(f=>f.Amount):0.00} kr  ·  Kvar {fines.Where(f=>f.PaidAt==null).Sum(f=>f.Amount):0.00} kr")));
        }
    }
    private void Schedule()
    {
        page.Children.Add(Text("Tvåveckorsschema",32));
        var date=start.ToDateTime(TimeOnly.MinValue);
        var year=Field("ISO-år",ISOWeek.GetYear(date).ToString()); var week=Field("Startvecka",ISOWeek.GetWeekOfYear(date).ToString());
        page.Children.Add(Row(year,week,Button("Visa",()=> { start=TeamService.WeekStart(Integer(year),Integer(week)); Render(); })));
        page.Children.Add(Row(Button("← Två veckor",()=> { start=start.AddDays(-14); Render(); }),Button("Två veckor →",()=> { start=start.AddDays(14); Render(); }),Button("Nytt event",()=> EditEvent(null))));
        page.Children.Add(Text("Matcher 2026/27: samtliga 15 publicerade seriematcher från Bandygrytan, kontrollerade 21 oktober 2026, samt fem träningsmatcher från klubben. Tider anges i svensk lokal tid. Ändringar hos källan hämtas inte automatiskt.",14));
        for(var i=0;i<14;i++)
        {
            var day=start.AddDays(i);
            var content=new StackPanel { Spacing=8 };
            content.Children.Add(Text(day.ToString("dddd d MMMM",Swedish),20));
            var events=service.State.Events.Where(e=>e.Date==day).OrderBy(e=>e.Time).ToList();
            if(events.Count==0) content.Children.Add(Text("Inga aktiviteter",14));
            foreach(var item in events)
            {
                content.Children.Add(Text($"{(item.Time?.ToString("HH:mm") ?? "Tid ej fastställd")}   {item.Title}\n{item.Description}"));
                content.Children.Add(Button("Redigera event",()=> EditEvent(item)));
                if(item.Source is not null) content.Children.Add(new HyperlinkButton { Content="Matchkälla",NavigateUri=new Uri(item.Source) });
            }
            page.Children.Add(Card(content));
        }
    }
    private async void EditEvent(TeamEvent? item)
    {
        try
        {
            var date=Field("Datum (ÅÅÅÅ-MM-DD)",(item?.Date ?? start).ToString("yyyy-MM-dd"));
            var time=Field("Tid (HH:mm, tomt om okänd)",item?.Time?.ToString("HH:mm") ?? "");
            var title=Field("Rubrik",item?.Title ?? ""); var description=Field("Kort beskrivning",item?.Description ?? "");
            var error=Text(""); var form=new StackPanel { Spacing=12 };
            foreach(var field in new UIElement[] { date,time,title,description,error }) form.Children.Add(field);
            var dialog=new ContentDialog { Title=item is null ? "Nytt event" : "Redigera event",Content=form,PrimaryButtonText="Spara",CloseButtonText="Avbryt",SecondaryButtonText=item is null ? "" : "Ta bort event",DefaultButton=ContentDialogButton.Primary,XamlRoot=page.XamlRoot };
            dialog.PrimaryButtonClick += (_,args) => {
                try {
                    if(!DateOnly.TryParseExact(date.Text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var selected)) throw new ArgumentException("Ange datum som ÅÅÅÅ-MM-DD.");
                    TimeOnly? selectedTime=null;
                    if(!string.IsNullOrWhiteSpace(time.Text)) {
                        if(!TimeOnly.TryParseExact(time.Text,"HH:mm",CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsed)) throw new ArgumentException("Ange tid som HH:mm.");
                        selectedTime=parsed;
                    }
                    service.SaveEvent(item?.Id,selected,selectedTime,title.Text,description.Text);
                } catch(Exception ex) { args.Cancel=true; error.Text=ex.Message; }
            };
            var result=await dialog.ShowAsync();
            if(result==ContentDialogResult.Secondary && item is not null) service.DeleteEvent(item.Id);
            Render();
        }
        catch(Exception ex) { feedback.Title="Kunde inte spara event"; feedback.Message=ex.Message; feedback.Severity=InfoBarSeverity.Error; feedback.IsOpen=true; }
    }
}
