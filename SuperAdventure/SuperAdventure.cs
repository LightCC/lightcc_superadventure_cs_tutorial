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

            BindUiElementsToNewPlayer(_player);

            // Move to the player's location to reset UI/status of everything
            _player.MoveTo(_player.CurrentLocation);
        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

            if (messageEventArgs.AddExtraNewLine)
            {
                rtbMessages.Text += Environment.NewLine;
            }

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void BindUiElementsToNewPlayer(Player player)
        {
            BindPlayerStatsToLabels(player);
            BindInventoryToGrid(player.Inventory);
            BindQuestsToGrid(player.Quests);
            BindWeaponsToComboBox(player.Weapons, player.CurrentWeapon);
            BindPotionsToComboBox(player.Potions);

            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += DisplayMessage;
        }

        private void BindPlayerStatsToLabels(Player player)
        {
            lblHitPoints.DataBindings.Clear();
            lblGold.DataBindings.Clear();
            lblExperience.DataBindings.Clear();
            lblLevel.DataBindings.Clear();

            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", player, "Level");
        }

        private void BindInventoryToGrid(BindingList<InventoryItem> inventory)
        {
            // Clear all columns if anything is already setup
            dgvInventory.Columns.Clear();

            // Setup the view, bind the data source, and add the columns.
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;
            dgvInventory.DataSource = inventory;
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Name",
                    Width = 197,
                    DataPropertyName = "Description"
                });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Quantity",
                    DataPropertyName = "Quantity"
                });
        }

        private void BindQuestsToGrid(BindingList<PlayerQuest> quest)
        {
            // Clear all columns if anything is already setup
            dgvQuests.Columns.Clear();

            // Setup the view, bind the data source, and add the columns.
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;
            dgvQuests.DataSource = quest;
            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Name",
                    Width = 197,
                    DataPropertyName = "Name"
                });
            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Done?",
                    DataPropertyName = "IsCompleted"
                });
        }

        /// <summary>
        /// When switching to a new active player, need to rebind the UI elements.
        /// This rebinds the weapons list to the cboWeapons combobox
        /// </summary>
        /// <param name="weapons">The list of weapons to bind, for the player to select from</param>
        /// <param name="currentWeapon">The current weapon in that list to show as selected</param>
        private void BindWeaponsToComboBox(List<Weapon> weapons, Weapon currentWeapon = null)
        {
            //Remove the function that would cause the index to be saved
            // to.CurrentWeapon when the DataSource is connected
            cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged;

            cboWeapons.DataSource = weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (currentWeapon != null)
            {
                cboWeapons.SelectedItem = currentWeapon;
            }

            //After setting the DataSource, and selecting any currentWeapon,
            // add the function back in so that if the player changes the index, it will be saved
            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
        }

        private void BindPotionsToComboBox(List<HealingPotion> potions)
        {
            cboPotions.DataSource = _player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";
        }

        private void btnCreateNewPlayer_Click(object sender, EventArgs e)
        {
            _player = Player.CreateDefaultPlayer();

            // Bind UI Elements to the new player object
            BindUiElementsToNewPlayer(_player);

            // Clean-up interface after creating a new player
            _player.MoveTo(_player.CurrentLocation);
            rtbMessages.Clear(); // clear messages text box
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = _player.Weapons;

                if (!_player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (e.PropertyName == "Potions")
            {
                cboPotions.DataSource = _player.Potions;

                if (!_player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }

            if (e.PropertyName == "CurrentLocation")
            {
                // Show/hide available movement buttons
                btnNorth.Visible = (_player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = (_player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = (_player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = (_player.CurrentLocation.LocationToWest != null);

                // Display current location name and description
                rtbLocation.Text = _player.CurrentLocation.Name + Environment.NewLine;
                rtbLocation.Text += _player.CurrentLocation.Description + Environment.NewLine;

                if(_player.CurrentLocation.MonsterLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = _player.Weapons.Any();
                    cboPotions.Visible = _player.Potions.Any();
                    btnUseWeapon.Visible = _player.Weapons.Any();
                    btnUsePotion.Visible = _player.Potions.Any();
                }
            }
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            _player.MoveNorth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            _player.MoveEast();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            _player.MoveSouth();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            _player.MoveWest();
        }

        /* private void MoveTo(Location newLocation)
        {
        } */

        /// <summary>
        /// Add text to a target RichTextBox, then scroll to the bottom of that box
        /// </summary>
        /// <param name="rtb">RichTextBox form element that text will be added to</param>
        /// <param name="textToAdd">Text string added to the bottom of the current 
        /// text in [rtb]</param>
        //private void AddTextToRichTextBoxAndScrollDown(RichTextBox rtb, String textToAdd)
        //{
        //    rtb.Text += textToAdd;
        //    ScrollToBottomOfRichTextBox(rtb);
        //}

        //private void ScrollToBottomOfRichTextBox(RichTextBox rtb)
        //{
        //    rtb.SelectionStart = rtb.Text.Length;
        //    rtb.ScrollToCaret();
        //}

        //private void UpdateWeaponListInUI()
        //{
        //    // Refresh player's weapons combobox
        //    List<Weapon> weapons = new List<Weapon>();

        //    foreach (InventoryItem inventoryItem in _player.Inventory)
        //    {
        //        if (inventoryItem.Details is Weapon)
        //        {
        //            if (inventoryItem.Quantity > 0)
        //            {
        //                weapons.Add((Weapon)inventoryItem.Details);
        //            }
        //        }
        //    }

        //    if (weapons.Count == 0)
        //    {
        //        // The player has no weapons, so hide the weapon combobox and the "Use" button
        //        cboWeapons.Visible = false;
        //        btnUseWeapon.Visible = false;
        //    }
        //    else // the player has > 0 weapons
        //    {
        //        // Remove the function that would cause the index to be saved
        //        // to .CurrentWeapon when the DataSource is connected
        //        cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged; 
        //        cboWeapons.DataSource = weapons;
        //        // After setting the DataSource, add the function back in so that
        //        // if the player changes the index, it will be saved
        //        cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
        //        cboWeapons.DisplayMember = "Name";
        //        cboWeapons.ValueMember = "ID";

        //        if (_player.CurrentWeapon != null)
        //        {
        //            cboWeapons.SelectedItem = _player.CurrentWeapon;
        //        }
        //        else
        //        {
        //            cboWeapons.SelectedIndex = 0;
        //        }
        //    }
        //}

        //private void UpdatePotionListInUI()
        //{
        //    // Refresh player's potions combobox
        //    List<HealingPotion> healingPotions = new List<HealingPotion>();

        //    foreach (InventoryItem inventoryItem2 in _player.Inventory)
        //    {
        //        if(inventoryItem2.Details is HealingPotion)
        //        {
        //            if(inventoryItem2.Quantity > 0)
        //            {
        //                healingPotions.Add((HealingPotion) inventoryItem2.Details);
        //            }
        //        }
        //    }

        //    if(healingPotions.Count == 0)
        //    {
        //        // The player doesn't have any potions, so hide the potion combobox
        //        // and the "Use" button
        //        cboPotions.Visible = false;
        //        btnUsePotion.Visible = false;
        //    }
        //    else // the player has > 0 potions
        //    {
        //        cboPotions.DataSource = healingPotions;
        //        cboPotions.DisplayMember = "Name";
        //        cboPotions.ValueMember = "ID";

        //        cboPotions.SelectedIndex = 0;
        //    }
        //}

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            _player.UseWeapon(currentWeapon);
        }

        /* private void btnUseWeapon_Click(object sender, EventArgs e)
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
                _player.AddExperiencePoints(_currentMonster.RewardExperiencePoints);
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

                //UpdateWeaponListInUI();
                //UpdatePotionListInUI();

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
        */

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.UsePotion(potion);
        }

        /* private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);
            if(_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            // The potion exists (it was in the combobox) so remove 1 from inventory
            _player.RemoveItemFromInventory(potion, 1);

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
                AddTextToRichTextBoxAndScrollDown(rtbMessages,  "The " + _currentMonster.Name +
                    " killed you." + Environment.NewLine);

                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh player data in UI
            //lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            //UpdatePotionListInUI();
        }
        */

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
