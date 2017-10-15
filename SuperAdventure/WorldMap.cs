using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
    public partial class WorldMap : Form
    {
        readonly Assembly _thisAssembly = Assembly.GetExecutingAssembly();

        public WorldMap(Player player)
        {
            InitializeComponent();

            SetImage(pic_0_2, player.LocationsVisited.Contains(World.LOCATION_ID_ALCHEMIST_GARDEN) ? "HerbalistsGarden" : "FogLocation");
            SetImage(pic_1_2, player.LocationsVisited.Contains(World.LOCATION_ID_ALCHEMIST_HUT) ? "HerbalistsHut" : "FogLocation");
            SetImage(pic_2_0, player.LocationsVisited.Contains(World.LOCATION_ID_FARM_FIELD) ? "FarmFields" : "FogLocation");
            SetImage(pic_2_1, player.LocationsVisited.Contains(World.LOCATION_ID_FARMHOUSE) ? "Farmhouse" : "FogLocation");
            SetImage(pic_2_2, player.LocationsVisited.Contains(World.LOCATION_ID_TOWN_SQUARE) ? "TownSquare" : "FogLocation");
            SetImage(pic_2_3, player.LocationsVisited.Contains(World.LOCATION_ID_GUARD_POST) ? "TownGate" : "FogLocation");
            SetImage(pic_2_4, player.LocationsVisited.Contains(World.LOCATION_ID_BRIDGE) ? "Bridge" : "FogLocation");
            SetImage(pic_2_5, player.LocationsVisited.Contains(World.LOCATION_ID_SPIDER_FIELD) ? "SpiderForest" : "FogLocation");
            SetImage(pic_3_2, player.LocationsVisited.Contains(World.LOCATION_ID_HOME) ? "Home" : "FogLocation");
        }

        private void SetImage(PictureBox pictureBoxTarget, string imageName)
        {
            using (Stream resourceStream = _thisAssembly.GetManifestResourceStream(_thisAssembly.GetName().Name + ".Images." + imageName + ".png"))
            {
                if (resourceStream != null)
                {
                    pictureBoxTarget.Image = new Bitmap(resourceStream);
                }
            }
        }

    }
}
