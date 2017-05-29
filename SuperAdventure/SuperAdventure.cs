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

        private void BindPotionsToComboBox(List<HealingPotion> potions, HealingPotion currentPotion = null)
        {
            // Remove the event that would cause the index to be saved
            // to Player.CurrentPotion when the DataSource is connected
            cboPotions.SelectedIndexChanged -= cboPotions_SelectedIndexChanged;

            cboPotions.DataSource = _player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            if (currentPotion != null)
            {
                cboPotions.SelectedItem = currentPotion;
            }

            // After setting the DataSource, and selecting any Player.CurrentPotion,
            // add the event handler back so that if the player changes the index, it will be saved
            cboPotions.SelectedIndexChanged += cboPotions_SelectedIndexChanged;
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
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

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

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            _player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.UsePotion(potion);
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void cboPotions_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.CurrentPotion = (HealingPotion)cboPotions.SelectedItem;
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }

    }
}
