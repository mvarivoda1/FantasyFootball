using FantasyFootball.Models;

namespace FantasyFootball.Models.ViewModels
{
    // ViewModel za Transfer screen: lijevo teren s trenutnim sastavom (kao My Team),
    // desno popis SVIH igrača koji se filtrira po poziciji, klubu i cijeni.
    public class TransferMarketViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public double Budget { get; set; }

        // Cijeli sastav tima — prikazuje se na terenu (lijeva strana)
        public List<Player> Squad { get; set; } = new();

        // Svi igrači u bazi — desna lista (kupnja / filtriranje)
        public List<Player> AllPlayers { get; set; } = new();

        public List<Player> SquadGoalkeepers => Squad.Where(p => p.Position == Position.Goalkeeper).ToList();
        public List<Player> SquadDefenders => Squad.Where(p => p.Position == Position.Defender).ToList();
        public List<Player> SquadMidfielders => Squad.Where(p => p.Position == Position.Midfielder).ToList();
        public List<Player> SquadForwards => Squad.Where(p => p.Position == Position.Forward).ToList();

        public HashSet<int> SquadPlayerIds => Squad.Select(p => p.Id).ToHashSet();
        public string FormationLabel =>
            $"{SquadGoalkeepers.Count}-{SquadDefenders.Count}-{SquadMidfielders.Count}-{SquadForwards.Count}";

        // Pozicijska / klupska ograničenja (za prikaz i info)
        public int RequiredGk { get; set; }
        public int RequiredDef { get; set; }
        public int RequiredMid { get; set; }
        public int RequiredFwd { get; set; }
        public int SquadSize { get; set; }
        public int MaxPerClub { get; set; }

        // Klubovi za dropdown filter (desna lista)
        public List<string> Clubs => AllPlayers
            .Select(p => p.Club)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}
