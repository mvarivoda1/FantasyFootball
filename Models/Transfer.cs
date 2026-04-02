namespace FantasyFootball.Models
{
    public class Transfer
    {
        public int Id { get; set; }
        public Player Player { get; set; }
        public FantasyTeam? FromTeam { get; set; }
        public FantasyTeam ToTeam { get; set; }
        public DateTime TransferDate { get; set; }
        public double Price { get; set; }
        public TransferStatus Status { get; set; }
    }
}
