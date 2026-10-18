using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace SmartBrewGUI;

public partial class MainWindow : Window
{
    private record Drink(string Name, int Price, string Description, bool Milk);
    private record Recipe(string Owner, string Name, string Drink, string Size, string Strength, string Milk, string Sugar, string Syrup, string Temperature);
    private record Order(string Owner, string Drink, int Price, string Details, DateTime Time, string Payment);
    private readonly List<Drink> drinks = new()
    {
        new("Espresso",25,"Rich and intense",false), new("Double Espresso",32,"Extra bold",false),
        new("Americano",30,"Smooth black coffee",false), new("Cappuccino",38,"Foamy classic",true),
        new("Caffè Latte",39,"Creamy and mellow",true), new("Flat White",40,"Velvety espresso",true),
        new("Mocha",43,"Chocolate coffee",true), new("Macchiato",34,"Espresso with foam",true),
        new("Hot Chocolate",35,"Sweet cocoa",true), new("Chai Latte",39,"Spiced and comforting",true),
        new("Iced Latte",42,"Cool and creamy",true), new("Iced Americano",34,"Refreshing black coffee",false)
    };
    private readonly Random random = new();
    private readonly Dictionary<string,string> accounts = new(StringComparer.OrdinalIgnoreCase) { ["demo"] = "demo" };
    private readonly List<Recipe> recipes = new();
    private readonly List<Order> orders = new();
    private readonly List<string> errors = new();
    private readonly Dictionary<string,int> stock = new() { ["Beans"] = 60, ["Water"] = 100, ["Milk"] = 60, ["Cups"] = 40, ["Chocolate"] = 30, ["Tea"] = 30 };
    private readonly string[] weather = { "Sunny ☀", "Rainy ☂", "Cloudy ☁", "Snowy ❄", "Windy" };
    private string currentUser = "Guest";
    private Drink selected;
    private string currentWeather = "";
    private string topToday = "";
    private int waste = 0;
    private bool technicianUnlocked;
    private bool offline;
    private bool brewing;
    private readonly DispatcherTimer brewTimer = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private int brewPercent;
    private Order? latest;
    private Grid content = null!;
    private TextBlock status = null!;
    private TextBlock header = null!;
    private TextBlock subtitle = null!;
    private ComboBox size = null!, strength = null!, milk = null!, sugar = null!, syrup = null!, temperature = null!, payment = null!;
    private TextBox recipeName = null!;
    private ProgressBar? progress;
    private TextBlock? progressText;

    private static readonly Brush Fore = Brush("#F4F0E9");
    private static readonly Brush Muted = Brush("#A9BEC2");
    private static readonly Brush Gold = Brush("#E5AF69");
    private static readonly Brush Panel = Brush("#1D3039");
    private static Brush Brush(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    public MainWindow()
    {
        InitializeComponent();
        selected = drinks[0];
        currentWeather = weather[random.Next(weather.Length)];
        topToday = drinks[random.Next(drinks.Count)].Name;
        BuildShell();
        brewTimer.Tick += BrewTick;
        Menu();
    }
    private TextBlock Text(string value, int font = 16, Brush? color = null, bool bold = false)
        => new() { Text = value, Foreground = color ?? Fore, FontSize = font, FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) };
    private Button Button(string label, Action click, bool primary = false)
    {
        var b = new Button { Content = label, FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = primary ? Brush("#1D2428") : Fore,
            Background = primary ? Gold : Brush("#2B4852"), BorderThickness = new Thickness(0), Padding = new Thickness(17, 12, 17, 12),
            Margin = new Thickness(4), Cursor = System.Windows.Input.Cursors.Hand, MinHeight = 44 };
        b.Click += (_, _) => { if (!brewing) click(); };
        return b;
    }
    private TextBox Input(string value = "") => new() { Text = value, FontSize = 16, Padding = new Thickness(10), Margin = new Thickness(0, 5, 0, 13), MinWidth = 220 };
    private ComboBox Options(params string[] choices)
    {
        var c = new ComboBox { FontSize = 16, Height = 38, Margin = new Thickness(0, 5, 0, 14) };
        foreach (string choice in choices) c.Items.Add(choice);
        c.SelectedIndex = 0;
        return c;
    }
    private Border Card(UIElement child) => new() { Child = child, Background = Panel, CornerRadius = new CornerRadius(15), Padding = new Thickness(18), Margin = new Thickness(5) };
    private StackPanel Vertical() => new() { Orientation = Orientation.Vertical };
    private WrapPanel Actions(params Button[] buttons)
    {
        var p = new WrapPanel(); foreach (var b in buttons) p.Children.Add(b); return p;
    }
    private void BuildShell()
    {
        Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var side = Vertical(); side.Margin = new Thickness(12); side.Children.Add(Text("☕ SmartBrew", 25, Gold, true));
        side.Children.Add(Text("COFFEE MACHINE DEMO", 11, Muted));
        foreach (var (name, action) in new (string, Action)[] {
            ("☕ Coffee Menu", Menu), ("★ Recommendation", Recommendation), ("◉ Top Today", Top),
            ("♙ Account / Login", Account), ("♡ My Recipes", Recipes), ("▤ Order History", History),
            ("⚙ Technician", Technician), ("? Demo Guide", Guide) }) side.Children.Add(Button(name, action));
        var offlineBox = new CheckBox { Content = "Demo offline mode", Foreground = Fore, Margin = new Thickness(10, 20, 0, 10) };
        offlineBox.Checked += (_, _) => { offline = true; RefreshStatus(); };
        offlineBox.Unchecked += (_, _) => { offline = false; RefreshStatus(); };
        side.Children.Add(offlineBox);
        status = Text("", 13, Muted); status.Margin = new Thickness(8, 18, 0, 0); side.Children.Add(status);
        var sideBorder = Card(side); sideBorder.Margin = new Thickness(12); Grid.SetColumn(sideBorder, 0); Root.Children.Add(sideBorder);
        var main = new Grid { Margin = new Thickness(18, 18, 22, 18) };
        main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        header = Text("", 34, Fore, true); subtitle = Text("", 15, Muted);
        var top = Vertical(); top.Children.Add(Text("FRESH COFFEE · SIMPLE DEMO", 12, Gold, true)); top.Children.Add(header); top.Children.Add(subtitle);
        Grid.SetRow(top, 0); main.Children.Add(top);
        content = new Grid(); Grid.SetRow(content, 1); main.Children.Add(content);
        Grid.SetColumn(main, 1); Root.Children.Add(main); RefreshStatus();
    }
    private void RefreshStatus() => status.Text = $"Signed in: {currentUser}\nWeather: {currentWeather}\nTop today: {topToday}\n\nBeans: {stock["Beans"]} | Milk: {stock["Milk"]}\nWater: {stock["Water"]} | Cups: {stock["Cups"]}\nWaste: {waste}/20\n\n" + (offline ? "OFFLINE DEMO" : "READY · DEMO MODE");
    private StackPanel Page(string title, string description)
    {
        header.Text = title; subtitle.Text = description; content.Children.Clear();
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        var p = Vertical(); scroll.Content = p; content.Children.Add(scroll); RefreshStatus(); return p;
    }
    private void Menu()
    {
        var p = Page("Choose your coffee", "Select a drink to customize and order. All payments are simulated.");
        var grid = new UniformGrid { Columns = 3 };
        foreach (var d in drinks)
        {
            var drink = d;
            var tile = Vertical();
            tile.Children.Add(Text(d.Milk ? "🥛 ☕" : "☕", 34, Gold));
            tile.Children.Add(Text(d.Name, 21, Fore, true));
            tile.Children.Add(Text(d.Description, 13, Muted));
            tile.Children.Add(Text($"{d.Price} SEK", 17, Gold, true));
            tile.Children.Add(Button("Customize →", () => Customize(drink), true));
            grid.Children.Add(Card(tile));
        }
        p.Children.Add(grid);
    }
    private void Customize(Drink drink, Recipe? saved = null)
    {
        selected = drink;
        var p = Page("Customize " + drink.Name, "Choose your options, then save a recipe or order.");
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition());
        var left = Vertical(); var right = Vertical(); Grid.SetColumn(right, 1); grid.Children.Add(left); grid.Children.Add(right);
        left.Children.Add(Text("Size")); size = Options("Regular", "Large (+5 SEK)"); left.Children.Add(size);
        left.Children.Add(Text("Coffee strength")); strength = Options("Mild", "Normal", "Strong"); left.Children.Add(strength);
        left.Children.Add(Text("Milk")); milk = Options("Regular", "Oat", "Lactose-free", "None"); left.Children.Add(milk);
        left.Children.Add(Text("Sugar")); sugar = Options("None", "1 spoon", "2 spoons", "3 spoons"); left.Children.Add(sugar);
        right.Children.Add(Text("Syrup")); syrup = Options("None", "Vanilla", "Caramel", "Hazelnut", "Chocolate"); right.Children.Add(syrup);
        right.Children.Add(Text("Temperature")); temperature = Options("Normal", "Extra hot", "Warm", "Iced"); right.Children.Add(temperature);
        right.Children.Add(Text("Recipe name (optional)")); recipeName = Input(drink.Name + " favorite"); right.Children.Add(recipeName);
        right.Children.Add(Text("Large size costs 5 SEK extra. Other options are free in this demo.", 13, Muted));
        if (saved != null)
        {
            size.SelectedItem = saved.Size; strength.SelectedItem = saved.Strength; milk.SelectedItem = saved.Milk;
            sugar.SelectedItem = saved.Sugar; syrup.SelectedItem = saved.Syrup; temperature.SelectedItem = saved.Temperature;
            recipeName.Text = saved.Name;
        }
        p.Children.Add(Card(grid));
        p.Children.Add(Actions(Button("← Menu", Menu), Button("♡ Save Recipe", SaveRecipe), Button("Continue to Checkout →", Checkout, true)));
    }
    private Recipe CurrentRecipe() => new(currentUser, recipeName.Text.Trim(), selected.Name,
        size.SelectedItem?.ToString() ?? "Regular", strength.SelectedItem?.ToString() ?? "Normal",
        milk.SelectedItem?.ToString() ?? "Regular", sugar.SelectedItem?.ToString() ?? "None",
        syrup.SelectedItem?.ToString() ?? "None", temperature.SelectedItem?.ToString() ?? "Normal");
    private int CurrentPrice() => selected.Price + (size.SelectedIndex == 1 ? 5 : 0);
    private void SaveRecipe()
    {
        if (currentUser == "Guest") { MessageBox.Show("Use Quick Demo Login or create an account first."); Account(); return; }
        var r = CurrentRecipe(); if (string.IsNullOrWhiteSpace(r.Name)) { MessageBox.Show("Enter a recipe name."); return; }
        recipes.RemoveAll(x => x.Owner == currentUser && x.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
        recipes.Add(r); MessageBox.Show($"Saved '{r.Name}'! Find it under My Recipes.");
    }
    private bool CanBrew(out string reason)
    {
        bool needsMilk = selected.Milk && milk.SelectedIndex != 3;
        bool needsChocolate = selected.Name is "Mocha" or "Hot Chocolate";
        bool needsTea = selected.Name == "Chai Latte";
        reason = waste >= 20 ? "Waste bin full" : stock["Cups"] < 1 ? "No cups" : stock["Water"] < 2 ? "Not enough water" :
            !needsTea && selected.Name != "Hot Chocolate" && stock["Beans"] < 2 ? "Not enough coffee beans" :
            needsMilk && stock["Milk"] < 3 ? "Not enough milk" : needsChocolate && stock["Chocolate"] < 2 ? "Not enough chocolate" :
            needsTea && stock["Tea"] < 2 ? "Not enough tea" : "";
        return reason.Length == 0;
    }
    private void Checkout()
    {
        if (!CanBrew(out var reason)) { errors.Add($"{DateTime.Now:g}: {reason}"); MessageBox.Show(reason + ". Open Technician to refill/empty."); return; }
        var p = Page("Checkout", "Confirm your customized drink. No real payments are processed.");
        var r = CurrentRecipe();
        p.Children.Add(Card(Text($"{selected.Name} — {CurrentPrice()} SEK\n\nSize: {r.Size}\nStrength: {r.Strength}\nMilk: {r.Milk}\nSugar: {r.Sugar}\nSyrup: {r.Syrup}\nTemperature: {r.Temperature}", 18)));
        p.Children.Add(Text("Payment method (simulation)", 18));
        payment = Options("Credit / Debit Card", "Apple Pay / Google Pay", "Prepaid Employee Card");
        p.Children.Add(payment);
        p.Children.Add(Actions(Button("← Customize", () => Customize(selected, r)), Button("Pay & Brew ☕", Pay, true)));
    }
    private void Pay()
    {
        if (offline && payment.SelectedIndex != 2) { MessageBox.Show("In offline demo mode, select Prepaid Employee Card."); return; }
        if (!CanBrew(out var reason)) { errors.Add($"{DateTime.Now:g}: {reason}"); MessageBox.Show(reason); return; }
        var r = CurrentRecipe();
        stock["Cups"]--; stock["Water"] -= 2;
        if (selected.Name != "Hot Chocolate" && selected.Name != "Chai Latte") stock["Beans"] -= 2;
        if (selected.Milk && milk.SelectedIndex != 3) stock["Milk"] -= 3;
        if (selected.Name is "Mocha" or "Hot Chocolate") stock["Chocolate"] -= 2;
        if (selected.Name == "Chai Latte") stock["Tea"] -= 2;
        waste++;
        latest = new Order(currentUser, selected.Name, CurrentPrice(), $"{r.Size}, {r.Strength}, {r.Milk}, {r.Sugar}, {r.Syrup}, {r.Temperature}", DateTime.Now, payment.SelectedItem?.ToString() ?? "Demo");
        orders.Add(latest); brewing = true; brewPercent = 0;
        var p = Page("Brewing your coffee", "Please wait while your virtual coffee machine prepares your drink.");
        p.Children.Add(Text("☕", 70, Gold));
        progress = new ProgressBar { Minimum = 0, Maximum = 100, Height = 26, Margin = new Thickness(8, 20, 8, 15) };
        progressText = Text("Preparing... 0%", 21); p.Children.Add(progress); p.Children.Add(progressText);
        brewTimer.Start();
    }
    private void BrewTick(object? sender, EventArgs e)
    {
        brewPercent += 5;
        if (progress != null) progress.Value = Math.Min(100, brewPercent);
        if (progressText != null) progressText.Text = $"Brewing... {Math.Min(100, brewPercent)}%";
        if (brewPercent < 100) return;
        brewTimer.Stop(); brewing = false;
        var p = Page("Your coffee is ready!", "Payment approved and brewing completed (simulation).");
        p.Children.Add(Text("☕ ✓", 74, Gold));
        p.Children.Add(Text($"Enjoy your {latest?.Drink}!", 25, Fore, true));
        p.Children.Add(Actions(Button("View receipt", () => { if (latest != null) ShowReceipt(latest); }), Button("Order another", Menu, true)));
    }
    private void ShowReceipt(Order o) => MessageBox.Show($"SMARTBREW DEMO RECEIPT\n{ o.Time:g}\nUser: {o.Owner}\nDrink: {o.Drink}\nOptions: {o.Details}\nPayment: {o.Payment}\nTotal: {o.Price} SEK\n\nSIMULATED PAYMENT — NO MONEY CHARGED", "Receipt");
    private void Recommendation()
    {
        var suggestion = drinks[random.Next(drinks.Count)];
        var p = Page("Today's recommendation", "Random drink suggestion for easy testing — no AI involved.");
        p.Children.Add(Card(Text($"★ Try {suggestion.Name}!\n\n{suggestion.Description}\n{suggestion.Price} SEK", 25, Gold, true)));
        p.Children.Add(Actions(Button("↻ Another random drink", Recommendation), Button("Customize this drink", () => Customize(suggestion), true)));
    }
    private void Top()
    {
        var d = drinks.First(x => x.Name == topToday);
        var p = Page("Top Today", "Demo popularity is randomized; it is not real sales analytics.");
        p.Children.Add(Card(Text($"🏆 {d.Name}\n\nToday's featured popular drink (random demo selection).", 25, Gold, true)));
        p.Children.Add(Actions(Button("↻ Randomize Top Today", () => { topToday = drinks[random.Next(drinks.Count)].Name; Top(); }), Button("Order this drink", () => Customize(d), true)));
    }
    private void Account()
    {
        var p = Page("Account & Login", "For testing: use demo / demo, or sign in instantly as a demo user.");
        p.Children.Add(Actions(Button("⚡ Quick Demo Login", () => { currentUser = "demo"; Account(); }, true), Button("Continue as Guest", () => { currentUser = "Guest"; Menu(); })));
        p.Children.Add(Text($"Current session: {currentUser}", 19, Gold, true));
        p.Children.Add(Text("Username")); var user = Input(); p.Children.Add(user);
        p.Children.Add(Text("Password (demo only)")); var pass = new PasswordBox { Padding = new Thickness(10), Height = 40, Margin = new Thickness(0, 5, 0, 12) }; p.Children.Add(pass);
        p.Children.Add(Actions(Button("Create account", () => {
            string name = user.Text.Trim(); if (name.Length < 2 || pass.Password.Length < 1) { MessageBox.Show("Enter a username (2+ characters) and a password."); return; }
            if (accounts.ContainsKey(name)) { MessageBox.Show("That username already exists."); return; }
            accounts[name] = pass.Password; currentUser = name; MessageBox.Show("Account created and logged in!"); Account();
        }), Button("Login", () => {
            if (!accounts.TryGetValue(user.Text.Trim(), out string? pwd) || pwd != pass.Password) { MessageBox.Show("Wrong username or password. Try demo / demo."); return; }
            currentUser = user.Text.Trim(); Account();
        }, true), Button("Logout", () => { currentUser = "Guest"; Account(); })));
        p.Children.Add(Text("Demo accounts are stored only in memory and reset when the application closes. Do not use real passwords.", 13, Muted));
    }
    private void Recipes()
    {
        var p = Page("My Recipes", "Save a customized drink, then reorder, edit or delete it.");
        if (currentUser == "Guest") { p.Children.Add(Text("Log in first to manage recipes.")); p.Children.Add(Button("Quick Demo Login", () => { currentUser = "demo"; Recipes(); }, true)); return; }
        var mine = recipes.Where(r => r.Owner == currentUser).ToList();
        if (mine.Count == 0) p.Children.Add(Text("No recipes yet. Pick a drink from the Coffee Menu and press Save Recipe."));
        foreach (var r in mine)
        {
            var row = Vertical(); row.Children.Add(Text($"♡ {r.Name} — {r.Drink}", 20, Gold, true));
            row.Children.Add(Text($"{r.Size} · {r.Strength} · {r.Milk} · {r.Syrup}", 14, Muted));
            row.Children.Add(Actions(Button("Order / Edit", () => Customize(drinks.First(d => d.Name == r.Drink), r), true),
                Button("Delete", () => { recipes.Remove(r); Recipes(); })));
            p.Children.Add(Card(row));
        }
        p.Children.Add(Button("+ Create a new recipe", Menu));
    }
    private void History()
    {
        var p = Page("Order History", "View orders made during this application session.");
        var mine = orders.Where(o => o.Owner == currentUser).OrderByDescending(o => o.Time).ToList();
        if (mine.Count == 0) p.Children.Add(Text("No orders yet. Try placing a demo order."));
        foreach (var o in mine)
        {
            var row = Vertical(); row.Children.Add(Text($"{o.Drink} · {o.Price} SEK", 19, Gold, true));
            row.Children.Add(Text($"{o.Time:g} · {o.Details}", 14, Muted));
            row.Children.Add(Button("View receipt", () => ShowReceipt(o))); p.Children.Add(Card(row));
        }
    }
    private void Technician()
    {
        var p = Page("Technician Panel", "Demo PIN: 2468. Refill ingredients and manage machine status.");
        if (!technicianUnlocked)
        {
            p.Children.Add(Text("Technician PIN")); var pin = new PasswordBox { Padding = new Thickness(10), Height = 40, Width = 250, HorizontalAlignment = HorizontalAlignment.Left };
            p.Children.Add(pin); p.Children.Add(Button("Unlock", () => { if (pin.Password == "2468") { technicianUnlocked = true; Technician(); } else MessageBox.Show("Incorrect PIN"); }, true)); return;
        }
        p.Children.Add(Text($"Beans: {stock["Beans"]} | Water: {stock["Water"]} | Milk: {stock["Milk"]} | Cups: {stock["Cups"]}\nChocolate: {stock["Chocolate"]} | Tea: {stock["Tea"]} | Waste: {waste}/20", 18));
        p.Children.Add(Actions(Button("Refill everything", () => { foreach (var key in stock.Keys.ToList()) stock[key] = 100; Technician(); }),
            Button("Empty waste bin", () => { waste = 0; Technician(); }),
            Button("Simulate low stock", () => { stock["Beans"] = 0; Technician(); }),
            Button("Add demo error", () => { errors.Add($"{DateTime.Now:g}: Demo heater warning"); Technician(); })));
        p.Children.Add(Text("Error log", 22, Gold, true));
        p.Children.Add(Text(errors.Count == 0 ? "No errors recorded." : string.Join("\n", errors.TakeLast(12)), 15));
        p.Children.Add(Button("Lock technician panel", () => { technicianUnlocked = false; Technician(); }));
    }
    private void Guide()
    {
        var p = Page("Demo Guide", "Quick steps for demonstrating your Task B prototype.");
        p.Children.Add(Card(Text("1. Account → Quick Demo Login (demo / demo).\n\n2. Coffee Menu → select any of 12 drinks.\n\n3. Customize → name and save a recipe.\n\n4. My Recipes → reorder, edit or delete.\n\n5. Checkout → simulated payment → animated brewing → receipt.\n\n6. Recommendation / Top Today → random drinks.\n\n7. Technician → PIN 2468 → refill, empty waste, simulate errors.\n\n8. Toggle offline mode and try Prepaid Employee Card.", 17)));
        p.Children.Add(Actions(Button("Randomize demo weather", () => { currentWeather = weather[random.Next(weather.Length)]; RefreshStatus(); MessageBox.Show("Weather: " + currentWeather); }), Button("Start ordering", Menu, true)));
        p.Children.Add(Text("This is a local demonstration only: no camera, mood detection, LLM, real weather API, NFC, hardware brewing or real payment processing. Accounts, recipes and orders reset on restart.", 14, Muted));
    }
}
