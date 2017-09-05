using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;

namespace Engine
{
    public static class PlayerDataMapper
    {
        // TBD - Configuration to side-step uninstalled DBs for the moment
        public static bool USE_DATABASE = false;

        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";
        private static readonly string _connectionString =
            "Data Source=(local);Initial Catalog=SuperAdventure;Integrated Security=True";

        /// <summary>
        /// Loads the SuperAdventureConsole game data from:
        /// * First, the SuperAdventure database, if it exists
        /// * Then, the XML save file, if it exists where expected
        /// * otherwise, the default player in default location
        /// </summary>
        public static Player LoadGameData()
        {
            Player player = null;

            if (USE_DATABASE)
            {
                player = CreatePlayerFromDatabase();
            }

            if (player == null)
            {
                if (File.Exists(PLAYER_DATA_FILE_NAME))
                {
                    player = CreatePlayerFromXmlString(
                        File.ReadAllText(PLAYER_DATA_FILE_NAME));
                }
                else
                {
                    player = Player.CreateDefaultPlayer();
                }
            }
            return player;
        }

        /// <summary>
        /// Saves the SuperAdventureConsole game data to both:
        /// * an XML save file
        /// * the SuperAdventure database
        /// </summary>
        public static void SaveGameData(Player player)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, ToXmlString(player));

            if (USE_DATABASE)
            {
                SaveToDatabase(player);
            }
        }

        private static Player CreatePlayerFromDatabase()
        {
            try
            {
                // This is our connection to the database
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Open the connection, so we can perform SQL commands
                    connection.Open();

                    Player player;

                    // Create a SQL command object, that uses the connection to
                    // our database. The SqlCommand object is where we create our
                    // SQL statement.
                    using (SqlCommand savedGameCommand = connection.CreateCommand())
                    {
                        savedGameCommand.CommandType = CommandType.Text;
                        // This SQL statement reads the first rows in the SavedGame table.
                        // For this program, we should only ever have one row,
                        // but this will ensure we only get one record in our 
                        // SQL query results.
                        savedGameCommand.CommandText = "SELECT TOP 1 * FROM SavedGame";

                        // Use ExecuteReader when you expect the query to return a row,
                        // or rows
                        SqlDataReader reader = savedGameCommand.ExecuteReader();

                        // Check if the query did not return a row/record of data
                        if (!reader.HasRows)
                        {
                            // There is no data in the SavedGame table,
                            // so return null (no saved player data)
                            return null;
                        }

                        // Get the row/record from the data reader
                        reader.Read();

                        // Get the column values for the row/record
                        int currentHitPoints = (int)reader["CurrentHitPoints"];
                        int maximumHitPoints = (int)reader["MaximumHitPoints"];
                        int gold = (int)reader["Gold"];
                        int experiencePoints = (int)reader["ExperiencePoints"];
                        int currentLocationID = (int)reader["CurrentLocationID"];

                        // Create the Player object, with the saved game values
                        player = Player.CreateBaseSavedPlayer(currentHitPoints,
                            maximumHitPoints,
                            gold,
                            experiencePoints,
                            currentLocationID);
                    }

                    // Read the rows/records from the Quest table, and add them
                    // to the player
                    using (SqlCommand questCommand = connection.CreateCommand())
                    {
                        questCommand.CommandType = CommandType.Text;
                        questCommand.CommandText = "SELECT * FROM Quest";

                        SqlDataReader reader = questCommand.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int questID = (int)reader["QuestID"];
                                bool isCompleted = (bool)reader["IsCompleted"];

                                // Build the PlayerQuest item, for this row
                                PlayerQuest playerQuest = new PlayerQuest(
                                    World.QuestByID(questID));
                                playerQuest.IsCompleted = isCompleted;

                                // Add the PlayerQuest to the player's property
                                player.Quests.Add(playerQuest);
                            }
                        }
                    }

                    // Read the rows/records from the Inventory table, and add
                    // them to the player
                    using (SqlCommand inventoryCommand = connection.CreateCommand())
                    {
                        inventoryCommand.CommandType = CommandType.Text;
                        inventoryCommand.CommandText = "SELECT * FROM Inventory";

                        SqlDataReader reader = inventoryCommand.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int inventoryItemID = (int)reader["InventoryItemID"];
                                int quantity = (int)reader["Quantity"];

                                // Add the item to the player's inventory
                                player.AddItemToInventory(
                                    World.ItemByID(inventoryItemID), quantity);
                            }
                        }
                    }

                    // Now that the player has been built from the database, return it
                    return player;
                }
            }
            catch(Exception ex)
            {
                // Ignore errors. If there is an error, this function will return
                // a "null" player
            }

        return null;
        }

        private static void SaveToDatabase(Player player)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Open the connection, so we can perform SQL commands
                    connection.Open();

                    // Insert/Update data in SavedGame table
                    using (SqlCommand existingRowCountCommand = connection.CreateCommand())
                    {
                        existingRowCountCommand.CommandType = CommandType.Text;
                        existingRowCountCommand.CommandText = "SELECT count(*) FROM SavedGame";

                        // Use ExecuteScalar when your query will return one value
                        int existingRowCount = (int)existingRowCountCommand.ExecuteScalar();

                        if (existingRowCount == 0)
                        {
                            // There is no existing row, so do an INSERT
                            using (SqlCommand insertSavedGame = connection.CreateCommand())
                            {
                                insertSavedGame.CommandType = CommandType.Text;
                                insertSavedGame.CommandText =
                                    "INSERT INTO SavedGame " +
                                    "(CurrentHitPoint, MaximumHitPoints, Gold, " +
                                        "ExperiencePoints, CurrentLocationID) " +
                                    "VALUES " +
                                    "(@CurrentHitPoints, @MaximumHitPoints, @Gold, " +
                                        "@ExperiencePoints, @CurrentLocationID)";

                                // Pass the values from the player object, to the SQL
                                // query, using parameters
                                insertSavedGame.Parameters.Add(
                                    "@CurrentHitPoints", SqlDbType.Int);
                                insertSavedGame.Parameters["@CurrentHitPoints"].Value =
                                    player.CurrentHitPoints;
                                insertSavedGame.Parameters.Add(
                                    "@MaximumHitPoints", SqlDbType.Int);
                                insertSavedGame.Parameters["@MaximumHitPoints"].Value =
                                    player.MaximumHitPoints;
                                insertSavedGame.Parameters.Add(
                                    "@Gold", SqlDbType.Int);
                                insertSavedGame.Parameters["@Gold"].Value =
                                    player.Gold;
                                insertSavedGame.Parameters.Add(
                                    "@ExperiencePoints", SqlDbType.Int);
                                insertSavedGame.Parameters["@ExperiencePoints"].Value =
                                    player.ExperiencePoints;
                                insertSavedGame.Parameters.Add(
                                    "@CurrentLocationID", SqlDbType.Int);
                                insertSavedGame.Parameters["@CurrentLocationID"].Value =
                                    player.CurrentLocation.ID;

                                // Perform the SQL command.
                                // Use ExecuteNonQuery, because this query does not 
                                // return any results.
                                insertSavedGame.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // There is an existing row, so do an UPDATE
                            using (SqlCommand updateSavedGame = connection.CreateCommand())
                            {
                                updateSavedGame.CommandType = CommandType.Text;
                                updateSavedGame.CommandText =
                                    "UPDATE SavedGame " +
                                    "SET CurrentHitPoints = @CurrentHitPoints, " +
                                    "MaximumHitPoints = @MaximumHitPoints, " +
                                    "Gold = @Gold, " +
                                    "ExperiencePoints = @ExperiencePoints, " +
                                    "CurrentLocationID = @CurrentLocationID";

                                // Pass the values from the player object, to the SQL
                                // query, using parameters
                                // Using parameters helps make your program more secure.
                                // It will prevent SQL injection attacks
                                updateSavedGame.Parameters.Add(
                                    "@CurrentHitPoints", SqlDbType.Int);
                                updateSavedGame.Parameters["@CurrentHitPoints"].Value =
                                    player.CurrentHitPoints;
                                updateSavedGame.Parameters.Add(
                                    "@MaximumHitPoints", SqlDbType.Int);
                                updateSavedGame.Parameters["@MaximumHitPoints"].Value =
                                    player.MaximumHitPoints;
                                updateSavedGame.Parameters.Add(
                                    "@Gold", SqlDbType.Int);
                                updateSavedGame.Parameters["@Gold"].Value =
                                    player.Gold;
                                updateSavedGame.Parameters.Add(
                                    "@ExperiencePoints", SqlDbType.Int);
                                updateSavedGame.Parameters["@ExperiencePoints"].Value =
                                    player.ExperiencePoints;
                                updateSavedGame.Parameters.Add(
                                    "@CurrentLocationID", SqlDbType.Int);
                                updateSavedGame.Parameters["@CurrentLocationID"].Value =
                                    player.CurrentLocation.ID;

                                // Perform the SQL command.
                                // Use ExcecuteNonQuery, because this query does not
                                // return any results.
                                updateSavedGame.ExecuteNonQuery();
                            }
                        }
                    }

                    // The Quest and Inventory tables might have more, or less, rows in
                    // the database than what the player has in their properties.
                    // So, when we save the player's game, we will delete all the old rows
                    // and add in all new rows.
                    // This is easier than trying to add/delete/update each individual rows

                    // Delete existing Quest rows
                    using (SqlCommand deleteQuestsCommand = connection.CreateCommand())
                    {
                        deleteQuestsCommand.CommandType = CommandType.Text;
                        deleteQuestsCommand.CommandText = "DELETE FROM Quest";

                        deleteQuestsCommand.ExecuteNonQuery();
                    }

                    // Insert Quest rows, from the player object
                    foreach(PlayerQuest playerQuest in player.Quests)
                    {
                        using (SqlCommand insertQuestCommand = connection.CreateCommand())
                        {
                            insertQuestCommand.CommandType = CommandType.Text;
                            insertQuestCommand.CommandText =
                                "INSERT INTO Quest (QuestID, IsCompleted) " +
                                "VALUES (@QuestID, @IsCompleted)";

                            insertQuestCommand.Parameters.Add(
                                "@QuestID", SqlDbType.Int);
                            insertQuestCommand.Parameters["@QuestID"].Value =
                                playerQuest.Details.ID;
                            insertQuestCommand.Parameters.Add(
                                "@IsCompleted", SqlDbType.Bit);
                            insertQuestCommand.Parameters["@IsCompleted"].Value =
                                playerQuest.IsCompleted;

                            insertQuestCommand.ExecuteNonQuery();
                        }
                    }

                    // Delete existing Inventory rows
                    using (SqlCommand deleteInventoryCommand = connection.CreateCommand())
                    {
                        deleteInventoryCommand.CommandType = CommandType.Text;
                        deleteInventoryCommand.CommandText = "DELETE FROM Inventory";

                        deleteInventoryCommand.ExecuteNonQuery();
                    }

                    // Insert Inventory rows, from the player object
                    foreach (InventoryItem inventoryItem in player.Inventory)
                    {
                        using (SqlCommand insertInventoryCommand =
                            connection.CreateCommand())
                        {
                            insertInventoryCommand.CommandType = CommandType.Text;
                            insertInventoryCommand.CommandText =
                                "INSERT INTO Inventory (InventoryItemID, Quantity) " +
                                "VALUES (@InventoryItemID, @Quantity)";
                            insertInventoryCommand.Parameters.Add(
                                "@InventoryItemID", SqlDbType.Int);
                            insertInventoryCommand.Parameters[
                                "@InventoryItemID"].Value = inventoryItem.Details.ID;
                            insertInventoryCommand.Parameters.Add(
                                "@Quantity", SqlDbType.Int);
                            insertInventoryCommand.Parameters["@Quantity"].Value =
                                inventoryItem.Quantity;
                            insertInventoryCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // We are going to ignore errors, for now.
            }
        }

        private static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                //  playerData.PreserveWhitespace = true;
                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                int currentLocationID = Convert.ToInt32(
                    playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);

                Player player = Player.CreateBaseSavedPlayer(currentHitPoints, maximumHitPoints,
                    gold, experiencePoints, currentLocationID);

                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeaponID") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode(
                        "/Player/Stats/CurrentWeaponID").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                if (playerData.SelectSingleNode("/Player/Stats/CurrentPotionID") != null)
                {
                    int currentPotionID = Convert.ToInt32(playerData.SelectSingleNode(
                        "/Player/Stats/CurrentPotionID").InnerText);
                    player.CurrentPotion = (HealingPotion)World.ItemByID(currentPotionID);
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

        private static string ToXmlString(Player _player)
        {
            XmlDocument playerData = new XmlDocument();
            //playerData.PreserveWhitespace = true;

            // Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);
            //playerData.CreateWhitespace("\n"); // (Environment.NewLine);

            // Create the "Stats" child node to hold the other player stats nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Create the child nodes for the "Stats" node
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(
                _player.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(
                _player.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(_player.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(_player.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(_player.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if (_player.CurrentWeapon != null)
            {
                XmlNode currentWeaponID = playerData.CreateElement("CurrentWeaponID");
                currentWeaponID.AppendChild(playerData.CreateTextNode(
                    _player.CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeaponID);
            }

            if (_player.CurrentPotion != null)
            {
                XmlNode currentPotionID = playerData.CreateElement("CurrentPotionID");
                currentPotionID.AppendChild(playerData.CreateTextNode(
                    _player.CurrentPotion.ID.ToString()));
                stats.AppendChild(currentPotionID);
            }

            // Create the "InventoryItems" node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create an "InventoryItem" node for each item in the player's inventory
            foreach (InventoryItem Item in _player.Inventory)
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
            foreach (PlayerQuest Quest in _player.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = Quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompleted = playerData.CreateAttribute("IsCompleted");
                isCompleted.Value = Quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompleted);

                // Applied the playerQuest item for _player Quest as a child node to the playerQuests node
                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; // The XML document, as a string
        }


    }
}
