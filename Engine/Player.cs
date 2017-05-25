using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; private set; }
        public int Level
        {
            // Level will always increase after 100 XP are gained
            get { return ((ExperiencePoints / 100) + 1); }
        }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        public Weapon CurrentWeapon { get; set; }

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints)
            : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            // add a club as well to test out the weapon combobox logic
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_CLUB), 1));

            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(
                    currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeaponID") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode(
                        "/Player/Stats/CurrentWeaponID").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes(
                    "/Player/InventoryItems/InventoryItem"))
                {
                    int itemID = Convert.ToInt32(node.Attributes["ID"].Value);
                    int itemQuantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    // refactored from tutorial due to overloading AddItemToInventory with quantity
                    player.AddItemToInventory(World.ItemByID(itemID), itemQuantity);
                }

                foreach (XmlNode node in playerData.SelectNodes(
                    "/Player/PlayerQuests/PlayerQuest"))
                {
                    int questID = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool questIsCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(questID));
                    playerQuest.IsCompleted = questIsCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                // If there was an error with the XML data, return a default player object
                return Player.CreateDefaultPlayer();
            }
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // There is no required item for this location, so return "true"
                return true;
            }

            // See if the player has the required item in their inventory
            return Inventory.Exists(ii => ii.Details.ID ==
                location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest questToCheck)
        {
            return Quests.Exists(playerQuest => playerQuest.Details.ID == questToCheck.ID);
        }

        public bool CompletedThisQuest(Quest questToCheck)
        {
            return Quests.Exists(playerQuest => (playerQuest.Details.ID == questToCheck.ID)
                && playerQuest.IsCompleted);
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

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Check each item in the player's inventory, to see if they have it, and enough of it
                if (!Inventory.Exists(ii => ii.Details.ID ==
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
            //NOTE2: If the player doesn't have enough of the item... not sure - negative quantity?
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(
                    ii => ii.Details.ID == qci.Details.ID);

                if (item != null)  // Don't have to check for this if we know he has the item?
                {
                    // Subtract the quantity from the player's inventory that was needed to complete the quest
                    item.Quantity -= qci.Quantity;
                }
            }
        }

        /// <summary>
        /// Add a single item to inventory or increment the quantity of that item by 1
        /// </summary>
        /// <param name="itemToAdd">The Item that is being added to inventory in quantity of 1</param>
        public void AddItemToInventory(Item itemToAdd)
        {
            AddItemToInventory(itemToAdd, 1);
        }

        /// <summary>
        /// Add 'quantity' of 'itemToAdd' to inventory, or increase existing quantity
        /// </summary>
        /// <param name="itemToAdd">Item to be added to inventory (can already be in inventory)</param>
        /// <param name="quantity">Amount of item to be added</param>
        public void AddItemToInventory(Item itemToAdd, int quantity)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                // They didn't have the item, so add it to their inventory (qty 1)
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                // They have the item in inventory, so just increment quantity
                item.Quantity += quantity;
            }
        }

        // NOTE:  If the player does not have 'quest', this will do nothing
        public void MarkQuestCompleted(Quest quest)
        {
            // Find the quest in the player's quest list
            PlayerQuest playerQuest = Quests.SingleOrDefault(
                pq => pq.Details.ID == quest.ID);

            if(playerQuest != null)
            {
                // if we found the quest in player's quest list, complete it
                playerQuest.IsCompleted = true;
            }
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            // Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Create the "Stats" child node to hold the other player stats nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Create the child nodes for the "Stats" node
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(
                this.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(
                this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if(CurrentWeapon != null)
            {
                XmlNode currentWeaponID = playerData.CreateElement("CurrentWeaponID");
                currentWeaponID.AppendChild(playerData.CreateTextNode(
                    this.CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeaponID);
            }

            // Create the "InventoryItems" node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create an "InventoryItem" node for each item in the player's inventory
            foreach(InventoryItem Item in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = Item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = Item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }

            // Create the "PlayerQuests" node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            // Create a "PlayerQuests" child node to hold each PlayerQuest node
            foreach(PlayerQuest Quest in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = Quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompleted = playerData.CreateAttribute("IsCompleted");
                isCompleted.Value = Quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompleted);

                // Applied the playerQuest item for this Quest as a child node to the playerQuests node
                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; // The XML document, as a string
        }

    }
}
