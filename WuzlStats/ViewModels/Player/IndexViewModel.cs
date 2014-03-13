﻿using System;
using System.Linq;
using WuzlStats.Models;

namespace WuzlStats.ViewModels.Player
{
    public class IndexViewModel
    {
        public IndexViewModel Build(Db db, string playerName)
        {
            Name = playerName;

            var games = db.PlayerPositions.Where(x => x.Player.Name == playerName && x.Position != Position.Both);

            var minDate = DateTime.UtcNow.AddDays(-30);
            LastPlayedDate = games.Max(x => x.Game.DateTime).ToLocalTime();
            AllTimeStats = Calculate(games);
            CurrentStats = Calculate(games.Where(x => x.Game.DateTime >= minDate));

            return this;
        }

        private PlayerStats Calculate(IQueryable<PlayerPosition> games)
        {
            var result = new PlayerStats();

            var blueWins = games.Where(x => x.Game.BlueScore > x.Game.RedScore).ToList();
            var redWins = games.Where(x => x.Game.BlueScore < x.Game.RedScore).ToList();

            result.WinsCount = blueWins.Count(x => x.Team == Team.Blue) + redWins.Count(x => x.Team == Team.Red);
            result.LossesCount = blueWins.Count(x => x.Team == Team.Red) + redWins.Count(x => x.Team == Team.Blue);

            var otherPlayers = games.SelectMany(x => x.Game.Players.Where(y => y.Team == x.Team && y.Player != x.Player)).Select(x => x.Player.Name);
            result.FavoritePartner = (from x in otherPlayers
                                      group x by x
                                          into g
                                          orderby g.Count() descending
                                          select new
                                          {
                                              Name = g.Key
                                          }).First().Name;


            var blueCount = games.Count(x => x.Team == Team.Blue);
            var redCount = games.Count(x => x.Team == Team.Red);
            if (redCount == blueCount)
                result.FavoriteTeam = "No favorite team";
            else if (redCount < blueCount)
                result.FavoriteTeam = "Blue";
            else
                result.FavoriteTeam = "Red";

            var offenseCount = games.Count(x => x.Position == Position.Offense);
            var defenseCount = games.Count(x => x.Position == Position.Defense);
            if (defenseCount == offenseCount)
                result.FavoritePosition = "No favorite position";
            else if (defenseCount < offenseCount)
                result.FavoritePosition = "Offense";
            else
                result.FavoritePosition = "Defense";

            return result;
        }


        public string Name { get; set; }
        public DateTime LastPlayedDate { get; set; }
        public PlayerStats AllTimeStats { get; set; }
        public PlayerStats CurrentStats { get; set; }


        public class PlayerStats
        {
            public int WinsCount { get; set; }
            public int LossesCount { get; set; }
            public string FavoriteTeam { get; set; }
            public string FavoritePartner { get; set; }
            public string FavoritePosition { get; set; }
        }
    }
}