using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        #region FIELDS

        private int _gold;
        private int _experiencePoints;
        private Monster _currentMonster;
        private Location _currentLocation;

        #endregion FIELDS

        #region PROPERTIES

        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged("Gold");
            }
        }

        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            private set
            {
                _experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");
            }
        }

        public int Level
        {
            // Level will always increase after 100 XP are gained
            get { return ((ExperiencePoints / 100) + 1); }
        }

        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

        public Weapon CurrentWeapon { get; set; }

        public BindingList<InventoryItem> Inventory { get; set; }

        public List<Weapon> Weapons
        {
            get
            {
                return Inventory.Where(x => x.Details is Weapon).Select(
                    x => x.Details as Weapon).ToList();
            }
        }

        public List<HealingPotion> Potions
        {
            get
            {
                return Inventory.Where(x => x.Details is HealingPotion).Select(
                    x => x.Details as HealingPotion).ToList();
            }
        }

        public BindingList<PlayerQuest> Quests { get; set; }

        public List<int> LocationsVisited { get; set; }

        public HealingPotion CurrentPotion { get; set; }

#endregion PROPERTIES

#region PRIVATE CONSTRUCTOR

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints)
            : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
            LocationsVisited = new List<int>();
        }

#endregion PRIVATE CONSTRUCTOR

#region PUBLIC CONSTRUCTOR

        /// <summary>
        /// Creates a Default Player at the Default Location
        /// </summary>
        /// <returns></returns>
        public static Player CreateDefaultPlayer()
        {
            int DEFAULT_HIT_POINTS = 10;
            int DEFAULT_GOLD = 20;
            int DEFAULT_EXPERIENCE_POINTS = 0;

            Player player = new Player(
                DEFAULT_HIT_POINTS,  // CurrentHitPoints
                DEFAULT_HIT_POINTS,  // MaximumHitPoints
                DEFAULT_GOLD,
                DEFAULT_EXPERIENCE_POINTS);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            // TODO - Remove this for production
            // add a club as well to test out the weapon combobox logic
            //player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_CLUB), 1));

            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public static Player CreatePlayerFromDatabase(
            int currentHitPoints, int maximumHitPoints, int gold,
            int experiencePoints, int currentLocationID)
        {
            Player player = new Player(currentHitPoints,
                maximumHitPoints, gold, experiencePoints);

            player.MoveTo(World.LocationByID(currentLocationID));

            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/LocationsVisited/LocationVisited"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);

                    player.LocationsVisited.Add(id);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                // If there was an error with the XML data, return a default player object
                return CreateDefaultPlayer();
            }
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            // Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Create the "Stats" child node to hold the other player statistics nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Create the child nodes for the "Stats" node
            CreateNewChildXmlNode(playerData, stats, "CurrentHitPoints", CurrentHitPoints);
            CreateNewChildXmlNode(playerData, stats, "MaximumHitPoints", MaximumHitPoints);
            CreateNewChildXmlNode(playerData, stats, "Gold", Gold);
            CreateNewChildXmlNode(playerData, stats, "ExperiencePoints", ExperiencePoints);
            CreateNewChildXmlNode(playerData, stats, "CurrentLocation", CurrentLocation.ID);

            if (CurrentWeapon != null)
            {
                CreateNewChildXmlNode(playerData, stats, "CurrentWeapon", CurrentWeapon.ID);
            }

            // Create the "LocationsVisited" child node to hold each LocationVisited node
            XmlNode locationsVisited = playerData.CreateElement("LocationsVisited");
            player.AppendChild(locationsVisited);

            // Create an "LocationVisited" node for each item in the player's inventory
            foreach (int locationID in LocationsVisited)
            {
                XmlNode locationVisited = playerData.CreateElement("LocationVisited");

                AddXmlAttributeToNode(playerData, locationVisited, "ID", locationID);

                locationsVisited.AppendChild(locationVisited);
            }

            // Create the "InventoryItems" child node to hold each InventoryItem node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create an "InventoryItem" node for each item in the player's inventory
            foreach (InventoryItem item in Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                AddXmlAttributeToNode(playerData, inventoryItem, "ID", item.Details.ID);
                AddXmlAttributeToNode(playerData, inventoryItem, "Quantity", item.Quantity);

                inventoryItems.AppendChild(inventoryItem);
            }

            // Create the "PlayerQuests" child node to hold each PlayerQuest node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            // Create a "PlayerQuest" node for each quest the player has acquired
            foreach (PlayerQuest quest in Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                AddXmlAttributeToNode(playerData, playerQuest, "ID", quest.Details.ID);
                AddXmlAttributeToNode(playerData, playerQuest, "IsCompleted", quest.IsCompleted);

                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; // The XML document, as a string, so we can save the data to disk
        }

        private void CreateNewChildXmlNode(XmlDocument document, XmlNode parentNode, string elementName, object value)
        {
            XmlNode node = document.CreateElement(elementName);
            node.AppendChild(document.CreateTextNode(value.ToString()));
            parentNode.AppendChild(node);
        }

        private void AddXmlAttributeToNode(XmlDocument document, XmlNode node, string attributeName, object value)
        {
            XmlAttribute attribute = document.CreateAttribute(attributeName);
            attribute.Value = value.ToString();
            node.Attributes.Append(attribute);

        }

#endregion PUBLIC CONSTRUCTOR

        #region METHODS - FOR PROPERTIES

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }

#endregion METHODS - FOR FIELDS

#region METHODS - EVENTS

        private void RaiseInventoryChangedEvent(Item item)
        {
            if(item is Weapon) { OnPropertyChanged("Weapons"); }
            if(item is HealingPotion) { OnPropertyChanged("Potions"); }
        }

        // Event to be used with RaiseMessage
        // Bind a function from the UI meant to receive the messages
        // to this EventHandler, it will be used/triggered in RaiseMessage
        public event EventHandler<MessageEventArgs> OnMessage;

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            if (OnMessage != null)
            {
                OnMessage(this, new MessageEventArgs(message, addExtraNewLine));
            }
        }

#endregion METHODS - EVENTS

#region METHODS - INVENTORY

        /// <summary>
        /// Remove a quantity of an item from inventory.
        /// If all the quantity of the item is removed, remove the entry from the inventory
        /// If the item is not present, or has less than quantity, removed from list.
        /// </summary>
        /// <param name="itemtoRemove">An item currently in Inventory</param>
        /// <param name="quantity">Default = 1; Amount of the item to remove</param>
        public void RemoveItemFromInventory(Item itemtoRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemtoRemove.ID);

            if(item == null)
            {
                // The item is not in the player's inventory, so ignore it
                // We might want to raise an error for this situation...
            }
            else
            {
                // They have the item in inventory
                item.Quantity -= quantity;

                // Don't allow negative quantities (might want an error here...)
                if(item.Quantity < 0)
                {
                    item.Quantity = 0;
                }

                // if the quantity is zero, remove the item from the list
                if(item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }

                // Notify the UI that the inventory has changed (item removed)
                RaiseInventoryChangedEvent(itemtoRemove);
            }
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // There is no required item for this location, so return "true"
                return true;
            }

            // See if the player has the required item in their inventory
            return Inventory.Any(ii => ii.Details.ID ==
                location.ItemRequiredToEnter.ID);
        }

        public int ItemQuantity(Item item)
        {
            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == item.ID)
                {
                    return ii.Quantity;
                }
            }

            // Did not find the item in this player's inventory
            return 0;
        }

        /// <summary>
        /// Add 'quantity' of 'itemToAdd' to inventory, or increase existing quantity
        /// </summary>
        /// <param name="itemToAdd">Item to be added to inventory (can already be in inventory)</param>
        /// <param name="quantity">Amount of item to be added (Default = 1)</param>
        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                // They didn't have the item, so add it to their inventory

                // If there are none of this type of item, and it has a "CurrentItem"
                // property, then set this to it, so indexing will work right
                // NOTE: The UI code will follow-thru with the updates, based on
                // RaiseInventoryChangedEvent and Player.PropertyChanged event
                if (itemToAdd is Weapon && !Weapons.Any())
                {
                    CurrentWeapon = (Weapon)itemToAdd;
                }

                if (itemToAdd is HealingPotion && !Potions.Any())
                {
                    CurrentPotion = (HealingPotion)itemToAdd;
                }

                Inventory.Add(new InventoryItem(itemToAdd, quantity));

            }
            else
            {
                // They already have the item in inventory, so just increase quantity
                item.Quantity += quantity;
            }

            // An inventory quantity was changed, raise an event
            RaiseInventoryChangedEvent(itemToAdd);
        }

#endregion METHODS - INVENTORY

#region METHODS - QUESTS

        public bool PlayerDoesNotHaveThisQuest(Quest questToCheck)
        {
            return !Quests.Any(playerQuest => playerQuest.Details.ID == questToCheck.ID);
        }

        public bool PlayerHasNotCompleted(Quest questToCheck)
        {
            return !(bool)Quests.Any(playerQuest => (playerQuest.Details.ID == questToCheck.ID)
                && playerQuest.IsCompleted);
        }

        private void GiveQuestToPlayer(Quest questToGive)
        {
            // Display the messages
            RaiseMessage("You receive the " + questToGive.Name +
                " quest.");
            RaiseMessage(questToGive.Description);
            RaiseMessage("To complete it, return with:");
            foreach (QuestCompletionItem qci in
                questToGive.QuestCompletionItems)
            {
                if (qci.Quantity == 1)
                {
                    RaiseMessage(qci.Quantity.ToString() + " " + qci.Details.Name);
                }
                else
                {
                    RaiseMessage(qci.Quantity.ToString() + " " + qci.Details.NamePlural);
                }
            }

            // Add the quest to the player's quest list
            Quests.Add(new PlayerQuest(questToGive));
        }

        public bool PlayerHasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Check each item in the player's inventory, to see if they have it, and enough of it
                if (!Inventory.Any(ii => ii.Details.ID ==
                     qci.Details.ID && ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
                // The quest completion item for this loop must be in inventory, and have enough quantity
            }

            // If we got here, then player must have all the required items, and enough of them, to complete the quest.
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            //NOTE: if the player doesn't have the item, this will do nothing
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Subtract the quantity from the player's inventory that was needed to complete the quest
                InventoryItem item = Inventory.SingleOrDefault(
                    ii => ii.Details.ID == qci.Details.ID);

                if (item != null)  // Don't have to check for this if we know he has the item?
                {
                    RemoveItemFromInventory(item.Details, qci.Quantity);
                }
            }
        }

        private void CompleteQuestAndGiveRewards(Quest questToComplete)
        {
            // Display message
            RaiseMessage("");
            RaiseMessage("You complete the " +
                         questToComplete.Name + " quest.");

            // Remove quest items from inventory
            RemoveQuestCompletionItems(questToComplete);

            // Give quest rewards
            RaiseMessage("You receive: ");
            RaiseMessage(questToComplete.RewardExperiencePoints.ToString() +
                " experience points");
            RaiseMessage(questToComplete.RewardGold.ToString() +
                " gold");
            RaiseMessage(questToComplete.RewardItem.Name);

            AddExperiencePoints(
                questToComplete.RewardExperiencePoints);
            Gold += questToComplete.RewardGold;

            // Add the reward item to the player's inventory
            AddItemToInventory(questToComplete.RewardItem);
            
            // Mark the quest as completed
            MarkQuestCompleted(questToComplete);
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // NOTE:  If the player does not have 'quest', this will do nothing

            // Find the quest in the player's quest list
            PlayerQuest playerQuest = Quests.SingleOrDefault(
                pq => pq.Details.ID == quest.ID);

            if (playerQuest != null)
            {
                // if we found the quest in player's quest list, complete it
                playerQuest.IsCompleted = true;
            }
        }

        #endregion METHODS - QUESTS

#region METHODS - MOVEMENT/TURN

        public void MoveNorth()
        {
            if(CurrentLocation.LocationToNorth != null)
            {
                MoveTo(CurrentLocation.LocationToNorth);
            }
        }

        public void MoveEast()
        {
            if (CurrentLocation.LocationToEast != null)
            {
                MoveTo(CurrentLocation.LocationToEast);
            }
        }

        public void MoveSouth()
        {
            if (CurrentLocation.LocationToSouth != null)
            {
                MoveTo(CurrentLocation.LocationToSouth);
            }
        }

        public void MoveWest()
        {
            if (CurrentLocation.LocationToWest != null)
            {
                MoveTo(CurrentLocation.LocationToWest);
            }
        }

        private void MoveHome() 
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        public void MoveTo(Location newLocation)
        {
            if (!HasRequiredItemToEnterThisLocation(newLocation))
            {
                RaiseMessage("You must have a " + newLocation.ItemRequiredToEnter.Name +
                    " to enter this location.");
                return;  // return without moving to the new location
            }

            // We have any required item, or there wasn't an item required, the player can enter this new, requested location
            CurrentLocation = newLocation;

            if (!LocationsVisited.Contains(CurrentLocation.ID))
            {
                LocationsVisited.Add(CurrentLocation.ID);
            }

            CompletelyHeal();

            #region QuestInNewLocation
            if (newLocation.HasAQuest)
            {
                if (PlayerDoesNotHaveThisQuest(newLocation.QuestAvailableHere))
                {
                    GiveQuestToPlayer(newLocation.QuestAvailableHere);
                }
                else // Player doesn't have the quest yet - so add it to his quest list
                {
                    if (PlayerHasNotCompleted(newLocation.QuestAvailableHere) && 
                        PlayerHasAllQuestCompletionItems(newLocation.QuestAvailableHere))
                    {
                        // Quest is completed!!
                        CompleteQuestAndGiveRewards(newLocation.QuestAvailableHere);
                    }

                } // end player has this quest / or not
            } // end quest is available here
            #endregion

            #region MonsterLivingHere
            // Does the location have a monster?
            if (newLocation.HasAMonster)
            {
                // Create a new random monster from those available in this location
                _currentMonster = newLocation.NewInstanceOfMonsterLivingHere();

                RaiseMessage("You see a " + _currentMonster.Name);
            }
            else // There is not a MonsterLivingHere
            {
                _currentMonster = null;
            }
            #endregion
        }

        private void CompletelyHeal()
        {
            // Completely heal the player
            CurrentHitPoints = MaximumHitPoints;
        }

        public void UseItemInBattle(Item itemUsed)
        {
            if (itemUsed is Weapon)
            {
                UseWeaponInBattle((Weapon)itemUsed);
            }
            else if (itemUsed is HealingPotion)
            {
                UsePotionInBattle((HealingPotion)itemUsed);
            }
            else
            {
                // Do nothing, the item sent is not usable in battle
                // This should be effectively the same as not using the item
            }
        }

        private void UseWeaponInBattle(Weapon weapon)
        {
            // Player gets to swing first
            // Determine amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(
                weapon.MinimumDamage, weapon.MaximumDamage);

            // Apply the damage to the monster's CurrentHitPoints
            _currentMonster.CurrentHitPoints -= damageToMonster;

            RaiseMessage("You hit the " + _currentMonster.Name +
                " for " + damageToMonster + " points.");

            // Check if the monster is dead
            if(_currentMonster.CurrentHitPoints <= 0)
            {
                // Monster is dead
                RaiseMessage("");
                RaiseMessage("You defeated the " + _currentMonster.Name);

                // Give player experience points for killing the monster
                AddExperiencePoints(_currentMonster.RewardExperiencePoints);
                RaiseMessage("You receive " + _currentMonster.RewardExperiencePoints +
                    " experience points");

                // Give player gold for killing the monster
                Gold += _currentMonster.RewardGold;
                RaiseMessage("You receive " + _currentMonster.RewardGold + " gold");

                // Add the LootItems from the Monster to Inventory
                foreach(InventoryItem inventoryItem in _currentMonster.LootItems)
                {
                    AddItemToInventory(inventoryItem.Details);

                    if(inventoryItem.Quantity == 1)
                    {
                        RaiseMessage("You loot " + inventoryItem.Quantity + " " +
                            inventoryItem.Details.Name);
                    }
                    else
                    {
                        RaiseMessage("You loot " + inventoryItem.Quantity + " " +
                            inventoryItem.Details.NamePlural);
                    }
                }

                // Add a blank line to the messages box, just for appearance
                RaiseMessage("");

                // Move player to current location (to heal player and create a new monster to fight)
                MoveTo(CurrentLocation);
            }
            else
            {
                // Monster is still alive after attack

                // Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(
                    0, _currentMonster.MaximumDamage);

                // Display message about the damage to player
                RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer +
                    " points of damage.");

                // Subtract damage from player
                CurrentHitPoints -= damageToPlayer;

                if(CurrentHitPoints <= 0) // Player died!!
                {
                    // Display message about player's untimely demise
                    RaiseMessage("The " + _currentMonster.Name + " killed you.");

                    // Move player to "Home"
                    MoveHome();
                }
            }
        }

        private void UsePotionInBattle(HealingPotion potion)
        {
            // Add healing amount to the player's current hit points
            CurrentHitPoints += potion.AmountToHeal;

            // CurrentHitPoints cannot exceed player's MaximumHitPoints
            if(CurrentHitPoints > MaximumHitPoints)
            {
                CurrentHitPoints = MaximumHitPoints;
            }

            // Remove the potion from the player's Inventory
            RemoveItemFromInventory(potion, 1);

            // Display message
            RaiseMessage("You drink a " + potion.Name);

            // Monster gets their turn to attack

            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(
                0, _currentMonster.MaximumDamage);

            // Display message about the damage to player
            RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer +
                " points of damage.");

            // Subtract damage from player
            CurrentHitPoints -= damageToPlayer;

            if (CurrentHitPoints <= 0) // Player died!!
            {
                // Display message about player's untimely demise
                RaiseMessage("The " + _currentMonster.Name + " killed you.");

                // Move player to "Home"
                MoveHome();
            }
        }

#endregion METHODS - MOVEMENT
    }
}
