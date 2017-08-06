using System;
using System.Linq;
using Engine;
using System.ComponentModel;
using System.IO;

namespace SuperAdventureConsole

{
    class Program
    {
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        private static Player _player;

        private static void Main(string[] args)
        {
            // Load the player
            LoadGameData();

            Console.WriteLine("Type 'Help' to see a list of commands");
            Console.WriteLine("");

            DisplayCurrentLocation();
        }

        private static void Player_OnPropertyChanged(
            object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Player_OnMessage(object sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ParseInput(string input)
        {
            throw new NotImplementedException();
        }

        private static void DisplayCurrentLocation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the SuperAdventureConsole game data from:
        /// * the XML save file, if it exists where expected
        /// * otherwise, from the the SuperAdventure database
        /// </summary>
        private static void LoadGameData()
        {
            _player = PlayerDataMapper.CreateFromDatabase();

            if(_player == null)
            {
                if (File.Exists(PLAYER_DATA_FILE_NAME))
                {
                    _player = Player.CreatePlayerFromXmlString(
                        File.ReadAllText(PLAYER_DATA_FILE_NAME));
                }
                else
                {
                    _player = Player.CreateDefaultPlayer();
                }
            }
        }

        /// <summary>
        /// Saves the SuperAdventureConsole game data to both:
        /// * an XML save file
        /// * the SuperAdventure database
        /// </summary>
        private static void SaveGameData()
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());

            PlayerDataMapper.SaveToDatabase(_player);
        }

    }
}
