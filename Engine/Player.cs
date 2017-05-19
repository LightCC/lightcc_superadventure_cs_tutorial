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
                && playerQuest.IsCompleted );
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
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Check each item in the player's inventory, to see if they have it, and enough of it
                if(!Inventory.Exists(ii => ii.Details.ID == 
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

                if(item != null)  // Don't have to check for this if we know he has the item?
                {
                    // Subtract the quantity from the player's inventory that was needed to complete the quest
                    item.Quantity -= qci.Quantity;
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemToAdd.ID);

            if(item == null)
            {
                // They didn't have the item, so add it to their inventory (qty 1)
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                // They have the item in inventory, so just increment quantity
                item.Quantity++;
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

    }
}
