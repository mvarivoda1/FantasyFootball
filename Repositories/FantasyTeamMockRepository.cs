using FantasyFootball.Models;

namespace FantasyFootball.Repositories
{
    public class FantasyTeamMockRepository
    {
        private readonly List<FantasyTeam> _teams;

        public FantasyTeamMockRepository(PlayerMockRepository playerRepo)
        {
            var p = playerRepo.GetAll();

            // Helper: find player by Id
            Player byId(int id) => p.First(x => x.Id == id);

            // ============================================
            // LIGA 1 — "Premier Fantasy Liga" (teams 1-5)
            // ============================================

            // Team 1: Haaland FC — napadacki tim oko Haalanda
            var team1 = new FantasyTeam { Id = 1, Name = "Haaland FC", OwnerName = "Marko", CreatedAt = new DateTime(2025, 8, 2), Budget = 2.5, TotalPoints = 1850 };
            team1.Players.AddRange(new[] {
                byId(16),  // Ederson (GK)
                byId(4),   // van Dijk (DEF)
                byId(18),  // Dias (DEF)
                byId(30),  // TAA (DEF)
                byId(40),  // Gabriel (DEF)
                byId(22),  // De Bruyne (MID)
                byId(6),   // Saka (MID)
                byId(2),   // Wirtz (MID)
                byId(9),   // Palmer (MID)
                byId(1),   // Haaland (FWD)
                byId(3),   // Salah (FWD)
            });

            // Team 2: Salah Stars — Liverpool heavy
            var team2 = new FantasyTeam { Id = 2, Name = "Salah Stars", OwnerName = "Ana", CreatedAt = new DateTime(2025, 8, 3), Budget = 6.0, TotalPoints = 1780 };
            team2.Players.AddRange(new[] {
                byId(5),   // Alisson (GK)
                byId(4),   // van Dijk (DEF)
                byId(30),  // TAA (DEF)
                byId(32),  // Konate (DEF)
                byId(7),   // Saliba (DEF)
                byId(2),   // Wirtz (MID)
                byId(35),  // Mac Allister (MID)
                byId(34),  // Szoboszlai (MID)
                byId(44),  // Odegaard (MID)
                byId(3),   // Salah (FWD)
                byId(10),  // Isak (FWD)
            });

            // Team 3: Palmer XI — Chelsea + Man City hybrid
            var team3 = new FantasyTeam { Id = 3, Name = "Palmer XI", OwnerName = "Ivan", CreatedAt = new DateTime(2025, 8, 3), Budget = 8.5, TotalPoints = 1820 };
            team3.Players.AddRange(new[] {
                byId(8),   // Raya (GK)
                byId(55),  // Colwill (DEF)
                byId(13),  // Gvardiol (DEF)
                byId(41),  // White (DEF)
                byId(56),  // Cucurella (DEF)
                byId(9),   // Palmer (MID)
                byId(61),  // Madueke (MID)
                byId(23),  // Foden (MID)
                byId(45),  // Rice (MID)
                byId(62),  // Jackson (FWD)
                byId(1),   // Haaland (FWD)
            });

            // Team 4: Red Devils — Man United fokus
            var team4 = new FantasyTeam { Id = 4, Name = "Red Devils", OwnerName = "Petra", CreatedAt = new DateTime(2025, 8, 5), Budget = 12.0, TotalPoints = 1520 };
            team4.Players.AddRange(new[] {
                byId(65),  // Onana (GK)
                byId(67),  // Martinez (DEF)
                byId(69),  // Dalot (DEF)
                byId(68),  // de Ligt (DEF)
                byId(42),  // Timber (DEF)
                byId(11),  // B. Fernandes (MID)
                byId(72),  // Mainoo (MID)
                byId(74),  // Diallo (MID)
                byId(86),  // Gordon (MID)
                byId(15),  // Sesko (FWD)
                byId(77),  // Hojlund (FWD)
            });

            // Team 5: Villa Thrilla — Aston Villa fokus
            var team5 = new FantasyTeam { Id = 5, Name = "Villa Thrilla", OwnerName = "Luka", CreatedAt = new DateTime(2025, 8, 6), Budget = 10.0, TotalPoints = 1580 };
            team5.Players.AddRange(new[] {
                byId(92),  // E. Martinez (GK)
                byId(94),  // Konsa (DEF)
                byId(95),  // Pau Torres (DEF)
                byId(96),  // Digne (DEF)
                byId(79),  // Trippier (DEF)
                byId(99),  // McGinn (MID)
                byId(101), // Rogers (MID)
                byId(100), // Tielemans (MID)
                byId(102), // Bailey (MID)
                byId(14),  // Watkins (FWD)
                byId(104), // Duran (FWD)
            });

            // ============================================
            // LIGA 2 — "Prijatelji Liga" (teams 6-10)
            // ============================================

            // Team 6: City Killers — Man City best XI
            var team6 = new FantasyTeam { Id = 6, Name = "City Killers", OwnerName = "Maja", CreatedAt = new DateTime(2025, 8, 11), Budget = 4.0, TotalPoints = 1750 };
            team6.Players.AddRange(new[] {
                byId(16),  // Ederson (GK)
                byId(13),  // Gvardiol (DEF)
                byId(18),  // Dias (DEF)
                byId(19),  // Stones (DEF)
                byId(21),  // Akanji (DEF)
                byId(22),  // De Bruyne (MID)
                byId(23),  // Foden (MID)
                byId(24),  // B. Silva (MID)
                byId(26),  // Savinho (MID)
                byId(1),   // Haaland (FWD)
                byId(27),  // Doku (FWD)
            });

            // Team 7: Golden Boot — top strikers
            var team7 = new FantasyTeam { Id = 7, Name = "Golden Boot", OwnerName = "Dario", CreatedAt = new DateTime(2025, 8, 12), Budget = 3.0, TotalPoints = 1880 };
            team7.Players.AddRange(new[] {
                byId(5),   // Alisson (GK)
                byId(30),  // TAA (DEF)
                byId(40),  // Gabriel (DEF)
                byId(13),  // Gvardiol (DEF)
                byId(94),  // Konsa (DEF)
                byId(9),   // Palmer (MID)
                byId(6),   // Saka (MID)
                byId(46),  // Havertz (MID)
                byId(84),  // Gordon (MID)
                byId(1),   // Haaland (FWD)
                byId(3),   // Salah (FWD)
            });

            // Team 8: Clean Sheet FC — defensive focus
            var team8 = new FantasyTeam { Id = 8, Name = "Clean Sheet FC", OwnerName = "Nina", CreatedAt = new DateTime(2025, 8, 13), Budget = 15.0, TotalPoints = 1450 };
            team8.Players.AddRange(new[] {
                byId(8),   // Raya (GK)
                byId(7),   // Saliba (DEF)
                byId(4),   // van Dijk (DEF)
                byId(18),  // Dias (DEF)
                byId(80),  // Botman (DEF)
                byId(31),  // Robertson (DEF)
                byId(45),  // Rice (MID)
                byId(59),  // Caicedo (MID)
                byId(25),  // Kovacic (MID)
                byId(14),  // Watkins (FWD)
                byId(63),  // Nkunku (FWD)
            });

            // Team 9: Toon Army — Newcastle heavy
            var team9 = new FantasyTeam { Id = 9, Name = "Toon Army", OwnerName = "Filip", CreatedAt = new DateTime(2025, 8, 14), Budget = 11.0, TotalPoints = 1380 };
            team9.Players.AddRange(new[] {
                byId(12),  // Pope (GK)
                byId(79),  // Trippier (DEF)
                byId(80),  // Botman (DEF)
                byId(81),  // Burn (DEF)
                byId(83),  // Hall (DEF)
                byId(84),  // Guimaraes (MID)
                byId(86),  // Gordon (MID)
                byId(85),  // Joelinton (MID)
                byId(22),  // De Bruyne (MID)
                byId(89),  // Wilson (FWD)
                byId(90),  // Barnes (FWD)
            });

            // Team 10: Template FC — balanced budget team
            var team10 = new FantasyTeam { Id = 10, Name = "Template FC", OwnerName = "Sara", CreatedAt = new DateTime(2025, 8, 15), Budget = 7.5, TotalPoints = 1620 };
            team10.Players.AddRange(new[] {
                byId(92),  // E. Martinez (GK)
                byId(30),  // TAA (DEF)
                byId(7),   // Saliba (DEF)
                byId(55),  // Colwill (DEF)
                byId(69),  // Dalot (DEF)
                byId(9),   // Palmer (MID)
                byId(2),   // Wirtz (MID)
                byId(11),  // B. Fernandes (MID)
                byId(101), // Rogers (MID)
                byId(10),  // Isak (FWD)
                byId(62),  // Jackson (FWD)
            });

            // ============================================
            // LIGA 3 — "Studentska Liga" (teams 11-15)
            // ============================================

            // Team 11: TVZ United
            var team11 = new FantasyTeam { Id = 11, Name = "TVZ United", OwnerName = "Ante", CreatedAt = new DateTime(2025, 9, 2), Budget = 5.0, TotalPoints = 1790 };
            team11.Players.AddRange(new[] {
                byId(16),  // Ederson (GK)
                byId(4),   // van Dijk (DEF)
                byId(40),  // Gabriel (DEF)
                byId(13),  // Gvardiol (DEF)
                byId(30),  // TAA (DEF)
                byId(6),   // Saka (MID)
                byId(9),   // Palmer (MID)
                byId(22),  // De Bruyne (MID)
                byId(44),  // Odegaard (MID)
                byId(3),   // Salah (FWD)
                byId(1),   // Haaland (FWD)
            });

            // Team 12: Debug FC
            var team12 = new FantasyTeam { Id = 12, Name = "Debug FC", OwnerName = "Josipa", CreatedAt = new DateTime(2025, 9, 2), Budget = 8.0, TotalPoints = 1650 };
            team12.Players.AddRange(new[] {
                byId(5),   // Alisson (GK)
                byId(7),   // Saliba (DEF)
                byId(32),  // Konate (DEF)
                byId(56),  // Cucurella (DEF)
                byId(67),  // Martinez (DEF)
                byId(2),   // Wirtz (MID)
                byId(23),  // Foden (MID)
                byId(58),  // E. Fernandez (MID)
                byId(72),  // Mainoo (MID)
                byId(10),  // Isak (FWD)
                byId(15),  // Sesko (FWD)
            });

            // Team 13: Stack Overflow XI
            var team13 = new FantasyTeam { Id = 13, Name = "Stack Overflow XI", OwnerName = "Toni", CreatedAt = new DateTime(2025, 9, 3), Budget = 9.5, TotalPoints = 1560 };
            team13.Players.AddRange(new[] {
                byId(8),   // Raya (GK)
                byId(41),  // White (DEF)
                byId(42),  // Timber (DEF)
                byId(95),  // Pau Torres (DEF)
                byId(79),  // Trippier (DEF)
                byId(45),  // Rice (MID)
                byId(84),  // Guimaraes (MID)
                byId(100), // Tielemans (MID)
                byId(61),  // Madueke (MID)
                byId(14),  // Watkins (FWD)
                byId(38),  // Diaz (FWD)
            });

            // Team 14: Null Pointer FC
            var team14 = new FantasyTeam { Id = 14, Name = "Null Pointer FC", OwnerName = "Matej", CreatedAt = new DateTime(2025, 9, 4), Budget = 6.5, TotalPoints = 1480 };
            team14.Players.AddRange(new[] {
                byId(12),  // Pope (GK)
                byId(18),  // Dias (DEF)
                byId(31),  // Robertson (DEF)
                byId(43),  // Calafiori (DEF)
                byId(97),  // Cash (DEF)
                byId(11),  // B. Fernandes (MID)
                byId(46),  // Havertz (MID)
                byId(74),  // Diallo (MID)
                byId(99),  // McGinn (MID)
                byId(77),  // Hojlund (FWD)
                byId(104), // Duran (FWD)
            });

            // Team 15: Git Push FC
            var team15 = new FantasyTeam { Id = 15, Name = "Git Push FC", OwnerName = "Lea", CreatedAt = new DateTime(2025, 9, 5), Budget = 7.0, TotalPoints = 1710 };
            team15.Players.AddRange(new[] {
                byId(92),  // E. Martinez (GK)
                byId(4),   // van Dijk (DEF)
                byId(55),  // Colwill (DEF)
                byId(80),  // Botman (DEF)
                byId(96),  // Digne (DEF)
                byId(36),  // Gravenberch (MID)
                byId(26),  // Savinho (MID)
                byId(86),  // Gordon (MID)
                byId(59),  // Caicedo (MID)
                byId(3),   // Salah (FWD)
                byId(90),  // Barnes (FWD)
            });

            _teams = new List<FantasyTeam> { team1, team2, team3, team4, team5, team6, team7, team8, team9, team10, team11, team12, team13, team14, team15 };

            // N-N: players reference back to teams
            foreach (var team in _teams)
                foreach (var player in team.Players)
                    if (!player.FantasyTeams.Contains(team))
                        player.FantasyTeams.Add(team);
        }

        public List<FantasyTeam> GetAll() => _teams;
        public FantasyTeam? GetById(int id) => _teams.FirstOrDefault(t => t.Id == id);
    }
}
