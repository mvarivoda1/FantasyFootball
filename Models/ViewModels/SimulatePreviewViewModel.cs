using FantasyFootball.Services;

namespace FantasyFootball.Models.ViewModels
{
    // Pregled simuliranog kola prije potvrde. Seed nosi nasumični rezultat kroz
    // potvrdu (ista vrijednost => isti rezultat).
    public class SimulatePreviewViewModel
    {
        public int GameweekId { get; set; }
        public int WeekNumber { get; set; }
        public int Seed { get; set; }
        public SimulationDraft Draft { get; set; } = new();
    }
}
