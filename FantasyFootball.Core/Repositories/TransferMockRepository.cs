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
                // Liga 1 — buy/sell aktivnost
                new Transfer { Id = 1,  Player = byId(15), Team = team(1),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 5),  Price = 50.0  },
                new Transfer { Id = 2,  Player = byId(9),  Team = team(3),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 10), Price = 110.0 },
                new Transfer { Id = 3,  Player = byId(6),  Team = team(2),  Direction = TransferDirection.Out, TransferDate = new DateTime(2025, 11, 1),  Price = 120.0 },
                new Transfer { Id = 4,  Player = byId(10), Team = team(2),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 12, 15), Price = 95.0  },
                new Transfer { Id = 5,  Player = byId(46), Team = team(1),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 22), Price = 65.0  },

                // Liga 2
                new Transfer { Id = 6,  Player = byId(14), Team = team(7),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 20), Price = 65.0  },
                new Transfer { Id = 7,  Player = byId(22), Team = team(9),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 11, 5),  Price = 55.0  },
                new Transfer { Id = 8,  Player = byId(90), Team = team(9),  Direction = TransferDirection.Out, TransferDate = new DateTime(2025, 11, 12), Price = 30.0  },
                new Transfer { Id = 9,  Player = byId(63), Team = team(8),  Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 12, 1),  Price = 60.0  },
                new Transfer { Id = 10, Player = byId(23), Team = team(10), Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 12, 5),  Price = 110.0 },

                // Liga 3
                new Transfer { Id = 11, Player = byId(44), Team = team(12), Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 15), Price = 100.0 },
                new Transfer { Id = 12, Player = byId(84), Team = team(13), Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 10, 28), Price = 60.0  },
                new Transfer { Id = 13, Player = byId(1),  Team = team(11), Direction = TransferDirection.Out, TransferDate = new DateTime(2025, 11, 10), Price = 180.0 },
                new Transfer { Id = 14, Player = byId(62), Team = team(14), Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 11, 20), Price = 50.0  },
                new Transfer { Id = 15, Player = byId(26), Team = team(11), Direction = TransferDirection.In,  TransferDate = new DateTime(2025, 12, 8),  Price = 55.0  },
            };
        }

        public List<Transfer> GetAll() => _transfers;
        public Transfer? GetById(int id) => _transfers.FirstOrDefault(t => t.Id == id);
    }
}
