// PlayerSpawner.cs
// Pure C# class responsible for creating PlayerModel instances.
// Scene-side spawning (instantiating GameObjects) is handled by MatchController
// which also owns the PlayerView references.

using System.Collections.Generic;

namespace DeathMatch.Player
{
    public static class PlayerSpawner
    {
        private static readonly string[] PlayerNames =
        {
            "Alpha", "Bravo", "Charlie", "Delta", "Echo",
            "Foxtrot", "Golf", "Hotel", "India", "Juliet",
            "Kilo", "Lima", "Mike", "November", "Oscar"
        };

        /// <summary>Creates the requested number of PlayerModel instances with unique IDs and names.</summary>
        public static List<PlayerModel> CreateModels(int count)
        {
            var models = new List<PlayerModel>(count);
            for (int i = 0; i < count; i++)
            {
                string name = i < PlayerNames.Length ? PlayerNames[i] : $"Player{i}";
                models.Add(new PlayerModel(i, name));
            }
            return models;
        }
    }
}