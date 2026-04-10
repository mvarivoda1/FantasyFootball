using FantasyFootball.Models;

namespace FantasyFootball.Repositories
{
    public class TransferMockRepository
    {
        private readonly List<Transfer> _transfers;

        public TransferMockRepository(PlayerMockRepository playerRepo, FantasyTeamMockRepository teamRepo)
        {
            var p = playerRepo.GetAll();
            var t = teamRepo.GetAll();
            Player byId(int id) => p.First(x => x.Id == id);
            FantasyTeam team(int id) => t.First(x => x.Id == id);

            _transfers = new List<Transfer>
            {
                // Liga 1 transfers
                new Transfer { Id = 1,  Player = byId(15), FromTeam = team(4),  ToTeam = team(1),  TransferDate = new DateTime(2025, 10, 5),  Price = 50.0,  Status = TransferStatus.Accepted },
                new Transfer { Id = 2,  Player = byId(9),  FromTeam = null,     ToTeam = team(3),  TransferDate = new DateTime(2025, 10, 10), Price = 110.0, Status = TransferStatus.Accepted },
                new Transfer { Id = 3,  Player = byId(6),  FromTeam = team(2),  ToTeam = team(1),  TransferDate = new DateTime(2025, 11, 1),  Price = 120.0, Status = TransferStatus.Rejected },
                new Transfer { Id = 4,  Player = byId(10), FromTeam = team(3),  ToTeam = team(2),  TransferDate = new DateTime(2025, 12, 15), Price = 95.0,  Status = TransferStatus.Pending },
                new Transfer { Id = 5,  Player = byId(46), FromTeam = null,     ToTeam = team(1),  TransferDate = new DateTime(2025, 10, 22), Price = 65.0,  Status = TransferStatus.Accepted },

                // Liga 2 transfers
                new Transfer { Id = 6,  Player = byId(14), FromTeam = null,     ToTeam = team(7),  TransferDate = new DateTime(2025, 10, 20), Price = 65.0,  Status = TransferStatus.Accepted },
                new Transfer { Id = 7,  Player = byId(22), FromTeam = team(6),  ToTeam = team(9),  TransferDate = new DateTime(2025, 11, 5),  Price = 55.0,  Status = TransferStatus.Cancelled },
                new Transfer { Id = 8,  Player = byId(90), FromTeam = team(9),  ToTeam = team(7),  TransferDate = new DateTime(2025, 11, 12), Price = 30.0,  Status = TransferStatus.Accepted },
                new Transfer { Id = 9,  Player = byId(63), FromTeam = null,     ToTeam = team(8),  TransferDate = new DateTime(2025, 12, 1),  Price = 60.0,  Status = TransferStatus.Pending },
                new Transfer { Id = 10, Player = byId(23), FromTeam = team(6),  ToTeam = team(10), TransferDate = new DateTime(2025, 12, 5),  Price = 110.0, Status = TransferStatus.Rejected },

                // Liga 3 transfers
                new Transfer { Id = 11, Player = byId(44), FromTeam = team(11), ToTeam = team(12), TransferDate = new DateTime(2025, 10, 15), Price = 100.0, Status = TransferStatus.Accepted },
                new Transfer { Id = 12, Player = byId(84), FromTeam = null,     ToTeam = team(13), TransferDate = new DateTime(2025, 10, 28), Price = 60.0,  Status = TransferStatus.Accepted },
                new Transfer { Id = 13, Player = byId(1),  FromTeam = team(11), ToTeam = team(15), TransferDate = new DateTime(2025, 11, 10), Price = 180.0, Status = TransferStatus.Rejected },
                new Transfer { Id = 14, Player = byId(62), FromTeam = team(12), ToTeam = team(14), TransferDate = new DateTime(2025, 11, 20), Price = 50.0,  Status = TransferStatus.Pending },
                new Transfer { Id = 15, Player = byId(26), FromTeam = team(15), ToTeam = team(11), TransferDate = new DateTime(2025, 12, 8),  Price = 55.0,  Status = TransferStatus.Accepted },
            };
        }

        public List<Transfer> GetAll() => _transfers;
        public Transfer? GetById(int id) => _transfers.FirstOrDefault(t => t.Id == id);
    }
}
