using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using System.IO;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(
                    File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }

            MoveTo(_player.CurrentLocation);
            UpdatePlayerStats();
        }

        private void btnCreateNewPlayer_Click(object sender, EventArgs e)
        {
            _player = Player.CreateDefaultPlayer();

            // Clean-up interface after creating a new player
            MoveTo(_player.CurrentLocation);
            UpdatePlayerStats();
            rtbMessages.Clear(); // clear messages text box
        }

        private void UpdatePlayerStats()
        {
            // Refresh player information and inventory controls
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            // Make sure _player has any required item for the new location
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You must have a " +
                    newLocation.ItemRequiredToEnter.Name +
                    " to enter this location." + Environment.NewLine);
                return;  // return without moving to the new location
            }

            // We have any required item, or there wasn't an item required,
            // so update the player's current location
            _player.CurrentLocation = newLocation;

            // Show/Hide the available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
 
                bool playerAlreadyHasQuest =
                    _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest =
                    _player.CompletedThisQuest(newLocation.QuestAvailableHere);
                
                // See if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    // If the player has not complete the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                        // See if the player has all the items
                        // needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest =
                            _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " +
                                newLocation.QuestAvailableHere.Name +
                                " quest." + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);    

                            // Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += 
                                newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() +
                                " experience points" + Environment.NewLine;
                            rtbMessages.Text += 
                                newLocation.QuestAvailableHere.RewardGold.ToString() +
                                " gold" + Environment.NewLine;
                            AddTextToRichTextBoxAndScrollDown(rtbMessages, 
                                newLocation.QuestAvailableHere.RewardItem.Name +
                                Environment.NewLine);

                            _player.ExperiencePoints +=
                                newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else // !playerAlreadyHasQuest
                {
                    // The player does not already have the quest

                    // Display the messages
                    rtbMessages.Text += "You receive the " +
                        newLocation.QuestAvailableHere.Name +
                        " quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description +
                        Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem qci in
                        newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " +
                                qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " +
                                qci.Details.NamePlural + Environment.NewLine;
                        }
                    }

                    AddTextToRichTextBoxAndScrollDown(rtbMessages,  Environment.NewLine);

                    // Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                } // end player has this quest / or not
            } // end quest is available here

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You see a " + newLocation.MonsterLivingHere.Name +
                    Environment.NewLine);

                // Make a new monster, using the values from the standard monster
                // in the World.Monster list
                Monster standardMonster = World.MonsterByID(
                    newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name,
                    standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints,
                    standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else // There is not a MonsterLivingHere
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            // Refresh the player's stats
            UpdatePlayerStats();

            // Refresh the player's inventory list
            UpdateInventoryListInUI();

            // Refresh the player's quest list
            UpdateQuestListInUI();

            // Refresh player's weapons combobox
            UpdateWeaponListInUI();

            // Refresh player's potions combobox
            UpdatePotionListInUI();
        }

        /// <summary>
        /// Add text to a target RichTextBox, then scroll to the bottom of that box
        /// </summary>
        /// <param name="rtb">RichTextBox form element that text will be added to</param>
        /// <param name="textToAdd">Text string added to the bottom of the current 
        /// text in [rtb]</param>
        private void AddTextToRichTextBoxAndScrollDown(RichTextBox rtb, String textToAdd)
        {
            rtb.Text += textToAdd;
            ScrollToBottomOfRichTextBox(rtb);
        }

        private void ScrollToBottomOfRichTextBox(RichTextBox rtb)
        {
            rtb.SelectionStart = rtb.Text.Length;
            rtb.ScrollToCaret();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name,
                        inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            // Refresh player's quest list
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name,
                    playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            // Refresh player's weapons combobox
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // The player has no weapons, so hide the weapon combobox and the "Use" button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else // the player has > 0 weapons
            {
                // Remove the function that would cause the index to be saved
                // to .CurrentWeapon when the DataSource is connected
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged; 
                cboWeapons.DataSource = weapons;
                // After setting the DataSource, add the function back in so that
                // if the player changes the index, it will be saved
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                //cboWeapons.Visible = true;
                //btnUseWeapon.Visible = true;

                if (_player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = _player.CurrentWeapon;
                }
                else
                {
                    cboWeapons.SelectedIndex = 0;
                }
            }
        }

        private void UpdatePotionListInUI()
        {
            // Refresh player's potions combobox
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem2 in _player.Inventory)
            {
                if(inventoryItem2.Details is HealingPotion)
                {
                    if(inventoryItem2.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion) inventoryItem2.Details);
                    }
                }
            }

            if(healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox
                // and the "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else // the player has > 0 potions
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //AddTextToRichTextBoxAndScrollDown(rtbMessages,  "Trying to use your weapon." + Environment.NewLine);

            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(
                currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            // Apply the damage to the monster's CurrentHitPoints
            _currentMonster.CurrentHitPoints -= damageToMonster;

            // Display message
            AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You hit the " + _currentMonster.Name + " for " +
                damageToMonster.ToString() + " points." + Environment.NewLine);

            // Check if the monster is dead
            if(_currentMonster.CurrentHitPoints <= 0)
            {
                // Monster is dead
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  Environment.NewLine);
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You defeated the " + _currentMonster.Name +
                    Environment.NewLine);

                // Give player experience points for killing the monster
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You receive " +
                    _currentMonster.RewardExperiencePoints.ToString() +
                    " experience points" + Environment.NewLine);

                // Give player gold for killing the monster
                _player.Gold += _currentMonster.RewardGold;
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You receive " +
                    _currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine);

                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Add items to the lootedItems list, comparing a random number to
                // the drop percentage
                foreach(LootItem lootItem in _currentMonster.LootTable)
                {
                    if(RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // If no items were randomly selected, then add the default loot item(s).
                if(lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                // Add the looted items to the player's inventory
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if(inventoryItem.Quantity == 1)
                    {
                        AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You loot " +
                            inventoryItem.Quantity.ToString() + " " +
                            inventoryItem.Details.Name + Environment.NewLine);
                    }
                    else
                    {
                        AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You loot " +
                            inventoryItem.Quantity.ToString() + " " +
                            inventoryItem.Details.NamePlural + Environment.NewLine);
                    }
                }

                UpdatePlayerStats();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                // Add a blank line to the messages box, just for appearance
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  Environment.NewLine);

                // Move player to current location (to heal player and
                // create a new monster to fight)
                MoveTo(_player.CurrentLocation);
            }
            else // Monster is still alive
            {
                // Determine the amount of damage the monster does to the player
                int damageToPlayer =
                    RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                // Display message
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "The " + _currentMonster.Name + " did " +
                    damageToPlayer.ToString() + " points of damage." + Environment.NewLine);

                // Subtract damage from player
                _player.CurrentHitPoints -= damageToPlayer;

                // Refresh player data in UI
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if(_player.CurrentHitPoints <= 0)
                {
                    // Display message
                    AddTextToRichTextBoxAndScrollDown(rtbMessages,  "The " + _currentMonster.Name + " killed you." +
                        Environment.NewLine);

                    // Move player to "Home"
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);
            if(_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            // The potion exists (it was in the combobox) so remove 1 from inventory
            InventoryItem playerItemPotionMatch = _player.Inventory.SingleOrDefault(ii => ii.Details.ID == potion.ID);
            if (playerItemPotionMatch != null)
            {
                playerItemPotionMatch.Quantity--;
            }

            // Display message
            AddTextToRichTextBoxAndScrollDown(rtbMessages,  "You drink a " + potion.Name + Environment.NewLine);

            // Monster gets their turn to attack

            // Determine the amount of damage the monster does to the player
            int damageToPlayer =
                RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Display message
            AddTextToRichTextBoxAndScrollDown(rtbMessages,  "The " + _currentMonster.Name + " did " +
                damageToPlayer.ToString() + " points of damage." + Environment.NewLine);

            // Subtract damage from player
            _player.CurrentHitPoints -= damageToPlayer;

            if(_player.CurrentHitPoints <= 0)
            {
                // Display message
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "The " + _currentMonster.Name + " killed you." +
                    Environment.NewLine);

                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh player data in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }

    }
}
