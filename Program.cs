using FantasyFootball.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// =============================================
// OBJEKTNI MODEL - Kreiranje podataka
// =============================================

// --- Igrači ---
var players = new List<Player>
{
    new Player { Id = 1, FirstName = "Erling", LastName = "Haaland", Position = Position.Forward, Club = "Manchester City", Nationality = "Norway", DateOfBirth = new DateTime(2000, 7, 21), MarketValue = 180.0, Goals = 27, Assists = 5, CleanSheets = 0, TotalPoints = 275 },
    new Player { Id = 2, FirstName = "Florian", LastName = "Wirtz", Position = Position.Midfielder, Club = "Liverpool", Nationality = "Germany", DateOfBirth = new DateTime(2003, 5, 3), MarketValue = 75.0, Goals = 7, Assists = 16, CleanSheets = 0, TotalPoints = 210 },
    new Player { Id = 3, FirstName = "Mohamed", LastName = "Salah", Position = Position.Forward, Club = "Liverpool", Nationality = "Egypt", DateOfBirth = new DateTime(1992, 6, 15), MarketValue = 70.0, Goals = 19, Assists = 13, CleanSheets = 0, TotalPoints = 250 },
    new Player { Id = 4, FirstName = "Virgil", LastName = "van Dijk", Position = Position.Defender, Club = "Liverpool", Nationality = "Netherlands", DateOfBirth = new DateTime(1991, 7, 8), MarketValue = 45.0, Goals = 3, Assists = 1, CleanSheets = 15, TotalPoints = 180 },
    new Player { Id = 5, FirstName = "Alisson", LastName = "Becker", Position = Position.Goalkeeper, Club = "Liverpool", Nationality = "Brazil", DateOfBirth = new DateTime(1992, 10, 2), MarketValue = 40.0, Goals = 0, Assists = 0, CleanSheets = 16, TotalPoints = 165 },
    new Player { Id = 6, FirstName = "Bukayo", LastName = "Saka", Position = Position.Midfielder, Club = "Arsenal", Nationality = "England", DateOfBirth = new DateTime(2001, 9, 5), MarketValue = 120.0, Goals = 14, Assists = 11, CleanSheets = 0, TotalPoints = 220 },
    new Player { Id = 7, FirstName = "William", LastName = "Saliba", Position = Position.Defender, Club = "Arsenal", Nationality = "France", DateOfBirth = new DateTime(2001, 3, 24), MarketValue = 90.0, Goals = 2, Assists = 1, CleanSheets = 14, TotalPoints = 170 },
    new Player { Id = 8, FirstName = "David", LastName = "Raya", Position = Position.Goalkeeper, Club = "Arsenal", Nationality = "Spain", DateOfBirth = new DateTime(1995, 9, 15), MarketValue = 35.0, Goals = 0, Assists = 0, CleanSheets = 13, TotalPoints = 155 },
    new Player { Id = 9, FirstName = "Cole", LastName = "Palmer", Position = Position.Midfielder, Club = "Chelsea", Nationality = "England", DateOfBirth = new DateTime(2002, 5, 6), MarketValue = 110.0, Goals = 22, Assists = 11, CleanSheets = 0, TotalPoints = 260 },
    new Player { Id = 10, FirstName = "Alexander", LastName = "Isak", Position = Position.Forward, Club = "Liverpool", Nationality = "Sweden", DateOfBirth = new DateTime(1999, 9, 21), MarketValue = 95.0, Goals = 21, Assists = 3, CleanSheets = 0, TotalPoints = 230 },
    new Player { Id = 11, FirstName = "Bruno", LastName = "Fernandes", Position = Position.Midfielder, Club = "Manchester United", Nationality = "Portugal", DateOfBirth = new DateTime(1994, 9, 8), MarketValue = 60.0, Goals = 8, Assists = 8, CleanSheets = 0, TotalPoints = 155 },
    new Player { Id = 12, FirstName = "Nick", LastName = "Pope", Position = Position.Goalkeeper, Club = "Newcastle", Nationality = "England", DateOfBirth = new DateTime(1992, 4, 19), MarketValue = 20.0, Goals = 0, Assists = 0, CleanSheets = 10, TotalPoints = 130 },
    new Player { Id = 13, FirstName = "Josko", LastName = "Gvardiol", Position = Position.Defender, Club = "Manchester City", Nationality = "Croatia", DateOfBirth = new DateTime(2002, 1, 23), MarketValue = 75.0, Goals = 5, Assists = 4, CleanSheets = 12, TotalPoints = 160 },
    new Player { Id = 14, FirstName = "Ollie", LastName = "Watkins", Position = Position.Forward, Club = "Aston Villa", Nationality = "England", DateOfBirth = new DateTime(1995, 12, 30), MarketValue = 65.0, Goals = 15, Assists = 9, CleanSheets = 0, TotalPoints = 195 },
    new Player { Id = 15, FirstName = "Benjamin", LastName = "Sesko", Position = Position.Forward, Club = "Manchester United", Nationality = "Slovenia", DateOfBirth = new DateTime(2003, 5, 31), MarketValue = 50.0, Goals = 12, Assists = 7, CleanSheets = 0, TotalPoints = 175 },
};

// --- 3 Lige ---
var league1 = new League
{
    Id = 1,
    Name = "Premier Fantasy Liga",
    Season = "2025/2026",
    CreatedAt = new DateTime(2025, 8, 1),
    MaxTeams = 8,
    Description = "Glavna liga za ozbiljne fantasy managere"
};

var league2 = new League
{
    Id = 2,
    Name = "Prijatelji Liga",
    Season = "2025/2026",
    CreatedAt = new DateTime(2025, 8, 10),
    MaxTeams = 6,
    Description = "Liga za prijatelje"
};

var league3 = new League
{
    Id = 3,
    Name = "Studentska Liga",
    Season = "2025/2026",
    CreatedAt = new DateTime(2025, 9, 1),
    MaxTeams = 10,
    Description = "Liga za studente"
};

// --- Fantasy timovi za Ligu 1 (4 tima) ---
var team1 = new FantasyTeam { Id = 1, Name = "Haaland FC", OwnerName = "Marko", CreatedAt = new DateTime(2025, 8, 2), Budget = 5.0, TotalPoints = 520, League = league1 };
team1.Players.AddRange(new[] { players[0], players[1], players[4], players[6] });

var team2 = new FantasyTeam { Id = 2, Name = "Salah Stars", OwnerName = "Ana", CreatedAt = new DateTime(2025, 8, 3), Budget = 8.5, TotalPoints = 485, League = league1 };
team2.Players.AddRange(new[] { players[2], players[3], players[5], players[11] });

var team3 = new FantasyTeam { Id = 3, Name = "Palmer XI", OwnerName = "Ivan", CreatedAt = new DateTime(2025, 8, 3), Budget = 12.0, TotalPoints = 510, League = league1 };
team3.Players.AddRange(new[] { players[8], players[9], players[7], players[12] });

var team4 = new FantasyTeam { Id = 4, Name = "Red Devils", OwnerName = "Petra", CreatedAt = new DateTime(2025, 8, 5), Budget = 15.0, TotalPoints = 430, League = league1 };
team4.Players.AddRange(new[] { players[10], players[14], players[13], players[3] });

league1.Teams.AddRange(new[] { team1, team2, team3, team4 });

// --- Fantasy timovi za Ligu 2 (3 tima) ---
var team5 = new FantasyTeam { Id = 5, Name = "City Killers", OwnerName = "Luka", CreatedAt = new DateTime(2025, 8, 11), Budget = 10.0, TotalPoints = 475, League = league2 };
team5.Players.AddRange(new[] { players[0], players[12], players[8], players[5] });

var team6 = new FantasyTeam { Id = 6, Name = "Golden Boot", OwnerName = "Maja", CreatedAt = new DateTime(2025, 8, 12), Budget = 7.0, TotalPoints = 460, League = league2 };
team6.Players.AddRange(new[] { players[2], players[9], players[1], players[6] });

var team7 = new FantasyTeam { Id = 7, Name = "Clean Sheet FC", OwnerName = "Dario", CreatedAt = new DateTime(2025, 8, 13), Budget = 20.0, TotalPoints = 390, League = league2 };
team7.Players.AddRange(new[] { players[4], players[3], players[7], players[14] });

league2.Teams.AddRange(new[] { team5, team6, team7 });

// --- Fantasy timovi za Ligu 3 (3 tima) ---
var team8 = new FantasyTeam { Id = 8, Name = "TVZ United", OwnerName = "Ante", CreatedAt = new DateTime(2025, 9, 2), Budget = 6.0, TotalPoints = 500, League = league3 };
team8.Players.AddRange(new[] { players[0], players[2], players[5], players[3] });

var team9 = new FantasyTeam { Id = 9, Name = "Debug FC", OwnerName = "Josipa", CreatedAt = new DateTime(2025, 9, 2), Budget = 9.0, TotalPoints = 445, League = league3 };
team9.Players.AddRange(new[] { players[8], players[10], players[12], players[4] });

var team10 = new FantasyTeam { Id = 10, Name = "Stack Overflow XI", OwnerName = "Toni", CreatedAt = new DateTime(2025, 9, 3), Budget = 11.0, TotalPoints = 410, League = league3 };
team10.Players.AddRange(new[] { players[9], players[13], players[11], players[7] });

league3.Teams.AddRange(new[] { team8, team9, team10 });

// N-N veza: igrači referenciraju natrag na timove
var allTeams = new List<FantasyTeam> { team1, team2, team3, team4, team5, team6, team7, team8, team9, team10 };
foreach (var team in allTeams)
{
    foreach (var player in team.Players)
    {
        if (!player.FantasyTeams.Contains(team))
            player.FantasyTeams.Add(team);
    }
}

var allLeagues = new List<League> { league1, league2, league3 };

// --- Transferi ---
league1.Transfers.AddRange(new[]
{
    new Transfer { Id = 1, Player = players[14], FromTeam = team4, ToTeam = team1, TransferDate = new DateTime(2025, 10, 5), Price = 50.0, Status = TransferStatus.Accepted },
    new Transfer { Id = 2, Player = players[8], FromTeam = null, ToTeam = team3, TransferDate = new DateTime(2025, 10, 10), Price = 110.0, Status = TransferStatus.Accepted },
    new Transfer { Id = 3, Player = players[5], FromTeam = team2, ToTeam = team1, TransferDate = new DateTime(2025, 11, 1), Price = 120.0, Status = TransferStatus.Rejected },
    new Transfer { Id = 4, Player = players[9], FromTeam = team3, ToTeam = team2, TransferDate = new DateTime(2025, 12, 15), Price = 95.0, Status = TransferStatus.Pending },
});

league2.Transfers.AddRange(new[]
{
    new Transfer { Id = 5, Player = players[13], FromTeam = null, ToTeam = team5, TransferDate = new DateTime(2025, 10, 20), Price = 65.0, Status = TransferStatus.Accepted },
    new Transfer { Id = 6, Player = players[1], FromTeam = team6, ToTeam = team7, TransferDate = new DateTime(2025, 11, 5), Price = 75.0, Status = TransferStatus.Cancelled },
});

// --- Gameweekovi s performansama ---
var gameweek1 = new Gameweek
{
    Id = 1, WeekNumber = 1,
    StartDate = new DateTime(2025, 8, 16),
    EndDate = new DateTime(2025, 8, 18),
};
gameweek1.Performances.AddRange(new[]
{
    new MatchPerformance { Id = 1, Player = players[0], MatchDate = new DateTime(2025, 8, 17), Opponent = "Chelsea", Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 13 },
    new MatchPerformance { Id = 2, Player = players[1], MatchDate = new DateTime(2025, 8, 17), Opponent = "Chelsea", Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
    new MatchPerformance { Id = 3, Player = players[2], MatchDate = new DateTime(2025, 8, 17), Opponent = "Wolves", Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 85, PointsEarned = 10 },
    new MatchPerformance { Id = 4, Player = players[4], MatchDate = new DateTime(2025, 8, 17), Opponent = "Wolves", Goals = 0, Assists = 0, CleanSheet = true, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
    new MatchPerformance { Id = 5, Player = players[8], MatchDate = new DateTime(2025, 8, 18), Opponent = "Everton", Goals = 3, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 18 },
    new MatchPerformance { Id = 6, Player = players[5], MatchDate = new DateTime(2025, 8, 18), Opponent = "Forest", Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
});

var gameweek2 = new Gameweek
{
    Id = 2, WeekNumber = 2,
    StartDate = new DateTime(2025, 8, 23),
    EndDate = new DateTime(2025, 8, 25),
};
gameweek2.Performances.AddRange(new[]
{
    new MatchPerformance { Id = 7, Player = players[0], MatchDate = new DateTime(2025, 8, 23), Opponent = "Newcastle", Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
    new MatchPerformance { Id = 8, Player = players[3], MatchDate = new DateTime(2025, 8, 24), Opponent = "Brentford", Goals = 1, Assists = 0, CleanSheet = true, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
    new MatchPerformance { Id = 9, Player = players[9], MatchDate = new DateTime(2025, 8, 24), Opponent = "Man City", Goals = 0, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 1, MinutesPlayed = 35, PointsEarned = -1 },
    new MatchPerformance { Id = 10, Player = players[13], MatchDate = new DateTime(2025, 8, 25), Opponent = "West Ham", Goals = 2, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 15 },
    new MatchPerformance { Id = 11, Player = players[14], MatchDate = new DateTime(2025, 8, 25), Opponent = "Leicester", Goals = 1, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 88, PointsEarned = 12 },
    new MatchPerformance { Id = 12, Player = players[10], MatchDate = new DateTime(2025, 8, 24), Opponent = "Brighton", Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 4 },
});

var allGameweeks = new List<Gameweek> { gameweek1, gameweek2 };
var allPerformances = allGameweeks.SelectMany(gw => gw.Performances).ToList();
var allTransfers = allLeagues.SelectMany(l => l.Transfers).ToList();

// --- Standings ---
var standingsLeague1 = league1.Teams
    .OrderByDescending(t => t.TotalPoints)
    .Select((t, index) => new Standing { Rank = index + 1, Team = t, Points = t.TotalPoints, GamesPlayed = 20 })
    .ToList();

// =============================================
// LINQ UPITI
// =============================================

Console.WriteLine("========================================");
Console.WriteLine("  FANTASY FOOTBALL - LINQ UPITI");
Console.WriteLine("========================================\n");

// 1. Svi napadači koji su zabili više od 15 golova
Console.WriteLine("1. Napadači s više od 15 golova:");
var topForwards = players
    .Where(p => p.Position == Position.Forward && p.Goals > 15)
    .OrderByDescending(p => p.Goals);
foreach (var p in topForwards)
    Console.WriteLine($"   {p.FullName} ({p.Club}) - {p.Goals} golova");

// 2. Top 5 igrača po ukupnim bodovima
Console.WriteLine("\n2. Top 5 igrača po bodovima:");
var top5Players = players
    .OrderByDescending(p => p.TotalPoints)
    .Take(5);
foreach (var p in top5Players)
    Console.WriteLine($"   {p.FullName} - {p.TotalPoints} bodova ({p.Position})");

// 3. Prosječna tržišna vrijednost igrača po poziciji
Console.WriteLine("\n3. Prosječna tržišna vrijednost po poziciji (mil. €):");
var avgValueByPosition = players
    .GroupBy(p => p.Position)
    .Select(g => new { Position = g.Key, AvgValue = g.Average(p => p.MarketValue) })
    .OrderByDescending(x => x.AvgValue);
foreach (var x in avgValueByPosition)
    Console.WriteLine($"   {x.Position}: {x.AvgValue:F1}");

// 4. Ljestvica lige 1 - timovi poredani po bodovima
Console.WriteLine($"\n4. Ljestvica - {league1.Name}:");
foreach (var s in standingsLeague1)
    Console.WriteLine($"   {s.Rank}. {s.Team.Name} ({s.Team.OwnerName}) - {s.Points} bodova");

// 5. Svi prihvaćeni transferi i njihova ukupna vrijednost
Console.WriteLine("\n5. Prihvaćeni transferi:");
var acceptedTransfers = allTransfers
    .Where(t => t.Status == TransferStatus.Accepted);
foreach (var t in acceptedTransfers)
    Console.WriteLine($"   {t.Player.FullName} -> {t.ToTeam.Name} za {t.Price} mil. € ({t.TransferDate:dd.MM.yyyy.})");
Console.WriteLine($"   Ukupna vrijednost: {acceptedTransfers.Sum(t => t.Price)} mil. €");

// 6. Igrač koji je u najviše fantasy timova (N-N veza)
Console.WriteLine("\n6. Najpopularniji igrači (u najviše timova):");
var mostPopular = players
    .OrderByDescending(p => p.FantasyTeams.Count)
    .Take(3);
foreach (var p in mostPopular)
    Console.WriteLine($"   {p.FullName} - u {p.FantasyTeams.Count} tima/timova");

// 7. Gameweek s najviše golova
Console.WriteLine("\n7. Golovi po gameweeku:");
var goalsPerGameweek = allGameweeks
    .Select(gw => new { gw.WeekNumber, TotalGoals = gw.Performances.Sum(p => p.Goals) })
    .OrderByDescending(x => x.TotalGoals);
foreach (var gw in goalsPerGameweek)
    Console.WriteLine($"   Gameweek {gw.WeekNumber}: {gw.TotalGoals} golova");

// 8. Igrači koji imaju i golove i asistencije (dual contributors)
Console.WriteLine("\n8. Igrači s golovima I asistencijama (dual contributors):");
var dualContributors = players
    .Where(p => p.Goals > 0 && p.Assists > 0)
    .OrderByDescending(p => p.Goals + p.Assists);
foreach (var p in dualContributors)
    Console.WriteLine($"   {p.FullName} - {p.Goals}G / {p.Assists}A");

// 9. Tim s najskupljim sastavom u ligi 1
Console.WriteLine($"\n9. Najskuplji sastav u {league1.Name}:");
var mostExpensiveTeam = league1.Teams
    .OrderByDescending(t => t.Players.Sum(p => p.MarketValue))
    .First();
Console.WriteLine($"   {mostExpensiveTeam.Name} - ukupna vrijednost: {mostExpensiveTeam.Players.Sum(p => p.MarketValue)} mil. €");

// 10. Lige koje imaju više od 3 tima
Console.WriteLine("\n10. Lige s više od 3 tima:");
var largeLeagues = allLeagues
    .Where(l => l.Teams.Count > 3);
foreach (var l in largeLeagues)
    Console.WriteLine($"   {l.Name} - {l.Teams.Count} timova");

// 11. Igrači rođeni nakon 2000. godine (mladi talenti)
Console.WriteLine("\n11. Mladi talenti (rođeni nakon 2000.):");
var youngTalents = players
    .Where(p => p.DateOfBirth.Year > 2000)
    .OrderBy(p => p.DateOfBirth);
foreach (var p in youngTalents)
    Console.WriteLine($"   {p.FullName} ({p.DateOfBirth:dd.MM.yyyy.}) - {p.Club}, {p.TotalPoints} bodova");

// 12. Performanse u gameweeku 1 gdje je igrač dobio više od 10 bodova
Console.WriteLine("\n12. Najbolje performanse u Gameweek 1 (10+ bodova):");
var bestPerformances = gameweek1.Performances
    .Where(mp => mp.PointsEarned >= 10)
    .OrderByDescending(mp => mp.PointsEarned);
foreach (var mp in bestPerformances)
    Console.WriteLine($"   {mp.Player.FullName} vs {mp.Opponent}: {mp.Goals}G {mp.Assists}A - {mp.PointsEarned} bodova");

// 13. Broj pending transfera po ligi
Console.WriteLine("\n13. Pending transferi po ligi:");
var pendingPerLeague = allLeagues
    .Select(l => new { l.Name, PendingCount = l.Transfers.Count(t => t.Status == TransferStatus.Pending) })
    .Where(x => x.PendingCount > 0);
foreach (var x in pendingPerLeague)
    Console.WriteLine($"   {x.Name}: {x.PendingCount} pending");

// 14. Golmani poredani po broju clean sheetova
Console.WriteLine("\n14. Golmani - clean sheet rang lista:");
var goalkeepers = players
    .Where(p => p.Position == Position.Goalkeeper)
    .OrderByDescending(p => p.CleanSheets);
foreach (var gk in goalkeepers)
    Console.WriteLine($"   {gk.FullName} ({gk.Club}) - {gk.CleanSheets} clean sheets");

// 15. Ukupan broj igrača po klubu u fantasy sustavu
Console.WriteLine("\n15. Broj igrača po klubu:");
var playersPerClub = players
    .GroupBy(p => p.Club)
    .Select(g => new { Club = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count);
foreach (var x in playersPerClub)
    Console.WriteLine($"   {x.Club}: {x.Count} igrača");

Console.WriteLine("\n========================================\n");

app.Run();
