using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class BuildFantasyTeamViewModel
    {
        [Required(ErrorMessage = "Naziv tima je obavezan.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Naziv tima mora imati između 2 i 80 znakova.")]
        public string TeamName { get; set; } = string.Empty;

        public List<int> SelectedPlayerIds { get; set; } = new();

        public List<Player> AvailablePlayers { get; set; } = new();
        public double Budget { get; set; }
        public int RequiredGk { get; set; }
        public int RequiredDef { get; set; }
        public int RequiredMid { get; set; }
        public int RequiredFwd { get; set; }
        public int MaxPerClub { get; set; }
    }
}
