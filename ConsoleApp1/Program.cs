using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Engine;

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

            // Connect player events to functions that will display in the UI
            _player.PropertyChanged += Player_OnPropertyChanged;
            _player.OnMessage += Player_OnMessage;

            // Infinite loop, until the user types "exit"
            while (true)
            {
                //Display a prompt, so the user knows to type something
                Console.Write(">");

                // Wait for the user to type something, and press the <Enter> key
                string userInput = Console.ReadLine();

                // If they typed a blank line, loop back and wait for input again
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                // Convert to lower-case, to make comparisons easier
                string cleanedInput = userInput.ToLower();

                // Save the current game data, and brake out of the "while(true)" loop
                if(cleanedInput == "exit")
                {
                    SaveGameData();

                    break;
                }

                // If the user typed something, try to determine what to do
                ParseInput(cleanedInput);
            }
        }

        private static void Player_OnPropertyChanged(
            object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "CurrentLocation")
            {
                DisplayCurrentLocation();

                if(_player.CurrentLocation.VendorWorkingHere != null)
                {
                    Console.WriteLine("You see a vendor here: {0}",
                        _player.CurrentLocation.VendorWorkingHere.Name);
                }
            }
        }

        private static void Player_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message);

            if (e.AddExtraNewLine)
            {
                Console.WriteLine("");
            }
        }

        private static void ParseInput(string input)
        {
            if(input.Contains("help") || input == "?")
            {
                Console.WriteLine("Available commands");
                Console.WriteLine("=====================================");
                Console.WriteLine("TBD - the rest of the list");
            }
            else if(input == "stats")
            {
                Console.WriteLine("TBD - Print the Character Stats here!!!");
            }
            else if(input == "look")
            {
                DisplayCurrentLocation();
            }
            else if (input.Contains("north"))
            {
                if(_player.CurrentLocation.LocationToNorth == null)
                {
                    Console.WriteLine("You cannot move North");
                }
                else
                {
                    _player.MoveNorth();
                }
            }
            else if (input.Contains("east"))
            {
                if (_player.CurrentLocation.LocationToEast == null)
                {
                    Console.WriteLine("You cannot move East");
                }
                else
                {
                    _player.MoveEast();
                }
            }
            else if (input.Contains("south"))
            {
                if (_player.CurrentLocation.LocationToSouth == null)
                {
                    Console.WriteLine("You cannot move South");
                }
                else
                {
                    _player.MoveSouth();
                }
            }
            else if (input.Contains("west"))
            {
                if (_player.CurrentLocation.LocationToWest == null)
                {
                    Console.WriteLine("You cannot move West");
                }
                else
                {
                    _player.MoveWest();
                }
            }
            else if (input == "inventory")
            {
                foreach(InventoryItem inventoryItem in _player.Inventory)
                {
                    Console.WriteLine("{0}: {1}",
                        inventoryItem.Description, inventoryItem.Quantity);
                }
            }
            else if(input == "quests")
            {
                if(_player.Quests.Count == 0)
                {
                    Console.WriteLine("You do not have any quests");
                }
                else
                {
                    foreach(PlayerQuest playerQuest in _player.Quests)
                    {
                        Console.WriteLine("{0}: {1}", playerQuest.Name,
                            playerQuest.IsCompleted ? "Completed" : "Incomplete");
                    }
                }
            }
            else if (input.Contains("attack"))
            {
                if(_player.CurrentLocation.MonsterLivingHere == null)
                {
                    Console.WriteLine("There is nothing here to attack");
                }
                else
                {
                    if(_player.CurrentWeapon == null)
                    {
                        _player.CurrentWeapon = _player.Weapons.FirstOrDefault();
                    }

                    if(_player.CurrentWeapon == null)
                    {
                        Console.WriteLine("You do not have any weapons");
                    }
                    else
                    {
                        _player.UseWeapon(_player.CurrentWeapon);
                    }
                }
            }
            else if(input.StartsWith("equip "))
            {
                string inputWeaponName = input.Substring(6).Trim();

                if (string.IsNullOrEmpty(inputWeaponName))
                {
                    Console.WriteLine("You must enter the name of the weapon to equip");
                }
                else
                {
                    Weapon weaponToEquip = _player.Weapons.SingleOrDefault(
                        x => x.Name.ToLower() ==
                        inputWeaponName || x.NamePlural.ToLower() == inputWeaponName);

                    if(weaponToEquip == null)
                    {
                        Console.WriteLine("You do not have the weapon: {0}", inputWeaponName);
                    }
                    else
                    {
                        _player.CurrentWeapon = weaponToEquip;

                        Console.WriteLine("You equip your {0}", _player.CurrentWeapon.Name);
                    }
                }
            }
            else if (input.StartsWith("drink "))
            {
                Console.WriteLine("TBD: DRINK X");
            }
            else if(input == "trade")
            {
                Console.WriteLine("TBD: trade");
            }
            else if(input.StartsWith("buy "))
            {
                Console.WriteLine("TBD: BUY X");
            }
            else if (input.StartsWith("sell "))
            {
                Console.WriteLine("TBD: SELL X");
            }
            else
            {
                Console.WriteLine("I do not understand: Invalid Command");
                Console.WriteLine("Type 'Help' to see a list of available commands");
            }

            // Write a blank line, to keep the UI a little cleaner
            Console.WriteLine("");
        }

        private static void DisplayCurrentLocation()
        {
            Console.WriteLine("You are at: {0}", _player.CurrentLocation.Name);

            if(_player.CurrentLocation.Description != "")
            {
                Console.WriteLine(_player.CurrentLocation.Description);
            }
        }

        /// <summary>
        /// Loads the SuperAdventureConsole game data from:
        /// * the XML save file, if it exists where expected
        /// * otherwise, from the the SuperAdventure database
        /// </summary>
        private static void LoadGameData()
        {
            _player = PlayerDataMapper.CreateFromDatabase();

            if (_player == null)
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