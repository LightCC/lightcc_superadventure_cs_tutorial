using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level
        {
            // Level will always increase after 100 XP are gained
            // It is auto-calculated when read
            // i.e. 0 xp = level 1, 100 = level 2, 1000 = level 10, etc.
            get { return ((ExperiencePoints / 100) + 1); }
        }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
        
        public Player(int gold, int experiencePoints, 
            int currentHitPoints, int maximumHitPoints)
            : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if(location.ItemRequiredToEnter == null)
            {
                // There is no required item for this location, so return "true"
                return true;
            }

            // See if the player has the required item in their inventory
            foreach (InventoryItem ii in Inventory)
            {
                if(ii.Details.ID == location.ItemRequiredToEnter.ID)
                {
                    // We found the required item, so return "true"
                    return true;
                }
            }

            // We didn't find the required item in their inventory, so return "false"
            return false;
        }

        public bool HasThisQuest(Quest questToCheck)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if(questToCheck.ID == playerQuest.Details.ID)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CompletedThisQuest(Quest questToCheck)
        {
            foreach(PlayerQuest playerQuest in Quests)
            {
                if(playerQuest.Details.ID == questToCheck.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }

            return false;
        }

        public int ItemQuantity(Item item)
        {
            foreach (InventoryItem ii in Inventory)
            {
                if(ii.Details.ID == item.ID)
                {
                    return ii.Quantity;
                }
            }

            // Did not find the item in this player's inventory
            return 0;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed
            // to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;

                // Check each item in the player's inventory,
                // to see if they have it, and enough of it
                foreach(InventoryItem ii in Inventory)
                {
                    // The player has the item in their inventory
                    if(ii.Details.ID == qci.Details.ID)
                    {
                        foundItemInPlayersInventory = true;
                        // The player does not have enough of this item
                        // to complete the quest
                        if(ii.Quantity < qci.Quantity)
                        {
                            return false;
                        }
                    }
                }

                // The player does not have any of this quest
                // completion item in their inventory
                if (!foundItemInPlayersInventory)
                {
                    return false;
                }
            }

            // If we got here, then the player must have all the required
            // items, and enough of them, to complete the quest.
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                foreach (InventoryItem ii in Inventory)
                {
                    if(ii.Details.ID == qci.Details.ID)
                    {
                        // Subtract quantity from player's inv
                        // that is needed to complete quest
                        ii.Quantity -= qci.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach (InventoryItem ii in Inventory)
            {
                if(ii.Details.ID == itemToAdd.ID)
                {
                    // They have the item in their inventory, so
                    // increase the quantity by one
                    ii.Quantity++;

                    return; // We added the item, and are done
                }
            }

            // They didn't have the item, so add it to their inventory
            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        // NOTE:  If the player does not have 'quest', this will do nothing
        public void MarkQuestCompleted(Quest quest)
        {
            // Find the quest in the player's quest list
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    // Mark it as completed
                    pq.IsCompleted = true;

                    // We found the quest, and marked it complete, so get
                    // out of this function
                    return;
                }
            }
        }

    }
}
