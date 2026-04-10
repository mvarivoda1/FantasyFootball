using FantasyFootball.Models;

namespace FantasyFootball.Repositories
{
    public class GameweekMockRepository
    {
        private readonly List<Gameweek> _gameweeks;

        public GameweekMockRepository(PlayerMockRepository playerRepo)
        {
            var p = playerRepo.GetAll();
            Player byId(int id) => p.First(x => x.Id == id);

            // ==========================================
            // GAMEWEEK 1
            // ==========================================
            var gw1 = new Gameweek
            {
                Id = 1, WeekNumber = 1,
                StartDate = new DateTime(2025, 8, 16),
                EndDate = new DateTime(2025, 8, 18),
            };
            gw1.Performances.AddRange(new[]
            {
                // Man City vs Chelsea
                new MatchPerformance { Id = 1,  Player = byId(1),  MatchDate = new DateTime(2025, 8, 17), Opponent = "Chelsea",   Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 13 },
                new MatchPerformance { Id = 2,  Player = byId(22), MatchDate = new DateTime(2025, 8, 17), Opponent = "Chelsea",   Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
                new MatchPerformance { Id = 3,  Player = byId(23), MatchDate = new DateTime(2025, 8, 17), Opponent = "Chelsea",   Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 85, PointsEarned = 7 },
                new MatchPerformance { Id = 4,  Player = byId(9),  MatchDate = new DateTime(2025, 8, 17), Opponent = "Man City",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
                // Liverpool vs Wolves
                new MatchPerformance { Id = 5,  Player = byId(3),  MatchDate = new DateTime(2025, 8, 17), Opponent = "Wolves",    Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 85, PointsEarned = 10 },
                new MatchPerformance { Id = 6,  Player = byId(5),  MatchDate = new DateTime(2025, 8, 17), Opponent = "Wolves",    Goals = 0, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
                new MatchPerformance { Id = 7,  Player = byId(10), MatchDate = new DateTime(2025, 8, 17), Opponent = "Wolves",    Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                // Arsenal vs Forest
                new MatchPerformance { Id = 8,  Player = byId(6),  MatchDate = new DateTime(2025, 8, 18), Opponent = "Forest",    Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
                new MatchPerformance { Id = 9,  Player = byId(44), MatchDate = new DateTime(2025, 8, 18), Opponent = "Forest",    Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
                new MatchPerformance { Id = 10, Player = byId(8),  MatchDate = new DateTime(2025, 8, 18), Opponent = "Forest",    Goals = 0, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
                // Newcastle vs Aston Villa
                new MatchPerformance { Id = 11, Player = byId(84), MatchDate = new DateTime(2025, 8, 18), Opponent = "Aston Villa", Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 12, Player = byId(14), MatchDate = new DateTime(2025, 8, 18), Opponent = "Newcastle",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 78, PointsEarned = 6 },
            });

            // ==========================================
            // GAMEWEEK 2
            // ==========================================
            var gw2 = new Gameweek
            {
                Id = 2, WeekNumber = 2,
                StartDate = new DateTime(2025, 8, 23),
                EndDate = new DateTime(2025, 8, 25),
            };
            gw2.Performances.AddRange(new[]
            {
                // Man City vs Newcastle
                new MatchPerformance { Id = 13, Player = byId(1),  MatchDate = new DateTime(2025, 8, 23), Opponent = "Newcastle",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 14, Player = byId(13), MatchDate = new DateTime(2025, 8, 23), Opponent = "Newcastle",  Goals = 1, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 15, Player = byId(84), MatchDate = new DateTime(2025, 8, 23), Opponent = "Man City",   Goals = 0, Assists = 0, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 2 },
                // Liverpool vs Brentford
                new MatchPerformance { Id = 16, Player = byId(4),  MatchDate = new DateTime(2025, 8, 24), Opponent = "Brentford",  Goals = 1, Assists = 0, CleanSheet = true,  YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 17, Player = byId(2),  MatchDate = new DateTime(2025, 8, 24), Opponent = "Brentford",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 88, PointsEarned = 10 },
                // Chelsea vs Everton
                new MatchPerformance { Id = 18, Player = byId(9),  MatchDate = new DateTime(2025, 8, 24), Opponent = "Everton",    Goals = 3, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 20 },
                new MatchPerformance { Id = 19, Player = byId(62), MatchDate = new DateTime(2025, 8, 24), Opponent = "Everton",    Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 75, PointsEarned = 6 },
                // Aston Villa vs West Ham
                new MatchPerformance { Id = 20, Player = byId(14), MatchDate = new DateTime(2025, 8, 25), Opponent = "West Ham",   Goals = 2, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 15 },
                new MatchPerformance { Id = 21, Player = byId(101),MatchDate = new DateTime(2025, 8, 25), Opponent = "West Ham",   Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
                // Man Utd vs Brighton
                new MatchPerformance { Id = 22, Player = byId(15), MatchDate = new DateTime(2025, 8, 25), Opponent = "Leicester",  Goals = 1, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 88, PointsEarned = 12 },
                new MatchPerformance { Id = 23, Player = byId(11), MatchDate = new DateTime(2025, 8, 24), Opponent = "Brighton",   Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 4 },
            });

            // ==========================================
            // GAMEWEEK 3
            // ==========================================
            var gw3 = new Gameweek
            {
                Id = 3, WeekNumber = 3,
                StartDate = new DateTime(2025, 8, 30),
                EndDate = new DateTime(2025, 9, 1),
            };
            gw3.Performances.AddRange(new[]
            {
                // Liverpool vs Arsenal (big match)
                new MatchPerformance { Id = 24, Player = byId(3),  MatchDate = new DateTime(2025, 8, 30), Opponent = "Arsenal",    Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 25, Player = byId(30), MatchDate = new DateTime(2025, 8, 30), Opponent = "Arsenal",    Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
                new MatchPerformance { Id = 26, Player = byId(6),  MatchDate = new DateTime(2025, 8, 30), Opponent = "Liverpool",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 27, Player = byId(45), MatchDate = new DateTime(2025, 8, 30), Opponent = "Liverpool",  Goals = 0, Assists = 0, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 2 },
                // Chelsea vs Crystal Palace
                new MatchPerformance { Id = 28, Player = byId(9),  MatchDate = new DateTime(2025, 8, 31), Opponent = "Crystal Palace", Goals = 1, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 13 },
                new MatchPerformance { Id = 29, Player = byId(61), MatchDate = new DateTime(2025, 8, 31), Opponent = "Crystal Palace", Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 82, PointsEarned = 11 },
                new MatchPerformance { Id = 30, Player = byId(55), MatchDate = new DateTime(2025, 8, 31), Opponent = "Crystal Palace", Goals = 0, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
                // Man City vs Fulham
                new MatchPerformance { Id = 31, Player = byId(1),  MatchDate = new DateTime(2025, 8, 31), Opponent = "Fulham",     Goals = 3, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 17 },
                new MatchPerformance { Id = 32, Player = byId(26), MatchDate = new DateTime(2025, 8, 31), Opponent = "Fulham",     Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 78, PointsEarned = 7 },
                // Newcastle vs Tottenham
                new MatchPerformance { Id = 33, Player = byId(86), MatchDate = new DateTime(2025, 9, 1),  Opponent = "Tottenham",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
                new MatchPerformance { Id = 34, Player = byId(90), MatchDate = new DateTime(2025, 9, 1),  Opponent = "Tottenham",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 85, PointsEarned = 9 },
                // Aston Villa vs Bournemouth
                new MatchPerformance { Id = 35, Player = byId(104),MatchDate = new DateTime(2025, 9, 1),  Opponent = "Bournemouth", Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 68, PointsEarned = 11 },
                new MatchPerformance { Id = 36, Player = byId(92), MatchDate = new DateTime(2025, 9, 1),  Opponent = "Bournemouth", Goals = 0, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
            });

            // ==========================================
            // GAMEWEEK 4
            // ==========================================
            var gw4 = new Gameweek
            {
                Id = 4, WeekNumber = 4,
                StartDate = new DateTime(2025, 9, 13),
                EndDate = new DateTime(2025, 9, 15),
            };
            gw4.Performances.AddRange(new[]
            {
                // Man Utd vs Man City (derby)
                new MatchPerformance { Id = 37, Player = byId(15), MatchDate = new DateTime(2025, 9, 13), Opponent = "Man City",   Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
                new MatchPerformance { Id = 38, Player = byId(74), MatchDate = new DateTime(2025, 9, 13), Opponent = "Man City",   Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
                new MatchPerformance { Id = 39, Player = byId(1),  MatchDate = new DateTime(2025, 9, 13), Opponent = "Man United", Goals = 2, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 15 },
                new MatchPerformance { Id = 40, Player = byId(22), MatchDate = new DateTime(2025, 9, 13), Opponent = "Man United", Goals = 1, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 88, PointsEarned = 13 },
                // Liverpool vs Bournemouth
                new MatchPerformance { Id = 41, Player = byId(10), MatchDate = new DateTime(2025, 9, 14), Opponent = "Bournemouth", Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 42, Player = byId(34), MatchDate = new DateTime(2025, 9, 14), Opponent = "Bournemouth", Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 9 },
                // Arsenal vs Everton
                new MatchPerformance { Id = 43, Player = byId(46), MatchDate = new DateTime(2025, 9, 14), Opponent = "Everton",    Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 44, Player = byId(48), MatchDate = new DateTime(2025, 9, 14), Opponent = "Everton",    Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 72, PointsEarned = 8 },
                new MatchPerformance { Id = 45, Player = byId(7),  MatchDate = new DateTime(2025, 9, 14), Opponent = "Everton",    Goals = 1, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                // Newcastle vs Wolves
                new MatchPerformance { Id = 46, Player = byId(89), MatchDate = new DateTime(2025, 9, 15), Opponent = "Wolves",     Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 67, PointsEarned = 5 },
                new MatchPerformance { Id = 47, Player = byId(12), MatchDate = new DateTime(2025, 9, 15), Opponent = "Wolves",     Goals = 0, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 8 },
            });

            // ==========================================
            // GAMEWEEK 5
            // ==========================================
            var gw5 = new Gameweek
            {
                Id = 5, WeekNumber = 5,
                StartDate = new DateTime(2025, 9, 20),
                EndDate = new DateTime(2025, 9, 22),
            };
            gw5.Performances.AddRange(new[]
            {
                // Chelsea vs Liverpool
                new MatchPerformance { Id = 48, Player = byId(9),  MatchDate = new DateTime(2025, 9, 20), Opponent = "Liverpool",  Goals = 2, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 14 },
                new MatchPerformance { Id = 49, Player = byId(63), MatchDate = new DateTime(2025, 9, 20), Opponent = "Liverpool",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 70, PointsEarned = 6 },
                new MatchPerformance { Id = 50, Player = byId(3),  MatchDate = new DateTime(2025, 9, 20), Opponent = "Chelsea",    Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 6 },
                new MatchPerformance { Id = 51, Player = byId(36), MatchDate = new DateTime(2025, 9, 20), Opponent = "Chelsea",    Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 3 },
                // Arsenal vs Man City
                new MatchPerformance { Id = 52, Player = byId(6),  MatchDate = new DateTime(2025, 9, 21), Opponent = "Man City",   Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 5 },
                new MatchPerformance { Id = 53, Player = byId(23), MatchDate = new DateTime(2025, 9, 21), Opponent = "Arsenal",    Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 84, PointsEarned = 6 },
                new MatchPerformance { Id = 54, Player = byId(18), MatchDate = new DateTime(2025, 9, 21), Opponent = "Arsenal",    Goals = 0, Assists = 0, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 1 },
                // Aston Villa vs Chelsea
                new MatchPerformance { Id = 55, Player = byId(14), MatchDate = new DateTime(2025, 9, 22), Opponent = "Chelsea",    Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 56, Player = byId(99), MatchDate = new DateTime(2025, 9, 22), Opponent = "Chelsea",    Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 4 },
                // Man Utd vs Crystal Palace
                new MatchPerformance { Id = 57, Player = byId(11), MatchDate = new DateTime(2025, 9, 22), Opponent = "Crystal Palace", Goals = 1, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 12 },
                new MatchPerformance { Id = 58, Player = byId(77), MatchDate = new DateTime(2025, 9, 22), Opponent = "Crystal Palace", Goals = 2, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 82, PointsEarned = 11 },
            });

            // ==========================================
            // GAMEWEEK 6
            // ==========================================
            var gw6 = new Gameweek
            {
                Id = 6, WeekNumber = 6,
                StartDate = new DateTime(2025, 9, 27),
                EndDate = new DateTime(2025, 9, 29),
            };
            gw6.Performances.AddRange(new[]
            {
                // Liverpool vs Man City (title clash)
                new MatchPerformance { Id = 59, Player = byId(3),  MatchDate = new DateTime(2025, 9, 27), Opponent = "Man City",   Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 60, Player = byId(2),  MatchDate = new DateTime(2025, 9, 27), Opponent = "Man City",   Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
                new MatchPerformance { Id = 61, Player = byId(1),  MatchDate = new DateTime(2025, 9, 27), Opponent = "Liverpool",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 6 },
                new MatchPerformance { Id = 62, Player = byId(24), MatchDate = new DateTime(2025, 9, 27), Opponent = "Liverpool",  Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 4 },
                // Arsenal vs Newcastle
                new MatchPerformance { Id = 63, Player = byId(44), MatchDate = new DateTime(2025, 9, 28), Opponent = "Newcastle",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 10 },
                new MatchPerformance { Id = 64, Player = byId(49), MatchDate = new DateTime(2025, 9, 28), Opponent = "Newcastle",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 62, PointsEarned = 6 },
                new MatchPerformance { Id = 65, Player = byId(40), MatchDate = new DateTime(2025, 9, 28), Opponent = "Newcastle",  Goals = 1, Assists = 0, CleanSheet = true,  YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 13 },
                new MatchPerformance { Id = 66, Player = byId(90), MatchDate = new DateTime(2025, 9, 28), Opponent = "Arsenal",    Goals = 0, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 1, MinutesPlayed = 55, PointsEarned = -1 },
                // Man Utd vs Aston Villa
                new MatchPerformance { Id = 67, Player = byId(72), MatchDate = new DateTime(2025, 9, 29), Opponent = "Aston Villa", Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 6 },
                new MatchPerformance { Id = 68, Player = byId(104),MatchDate = new DateTime(2025, 9, 29), Opponent = "Man United",  Goals = 1, Assists = 1, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 75, PointsEarned = 8 },
                new MatchPerformance { Id = 69, Player = byId(100),MatchDate = new DateTime(2025, 9, 29), Opponent = "Man United",  Goals = 0, Assists = 1, CleanSheet = false, YellowCards = 1, RedCards = 0, MinutesPlayed = 90, PointsEarned = 3 },
                // Chelsea vs Tottenham
                new MatchPerformance { Id = 70, Player = byId(9),  MatchDate = new DateTime(2025, 9, 29), Opponent = "Tottenham",  Goals = 1, Assists = 0, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
                new MatchPerformance { Id = 71, Player = byId(58), MatchDate = new DateTime(2025, 9, 29), Opponent = "Tottenham",  Goals = 0, Assists = 2, CleanSheet = false, YellowCards = 0, RedCards = 0, MinutesPlayed = 90, PointsEarned = 7 },
            });

            _gameweeks = new List<Gameweek> { gw1, gw2, gw3, gw4, gw5, gw6 };
        }

        public List<Gameweek> GetAll() => _gameweeks;
        public Gameweek? GetById(int id) => _gameweeks.FirstOrDefault(g => g.Id == id);
    }
}
