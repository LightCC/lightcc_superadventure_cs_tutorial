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
        private Player _player;

        public WorldMap(Player player)
        {
            InitializeComponent();

            _player = player;

            SetImage(pic_0_2, World.LOCATION_ID_ALCHEMIST_GARDEN);
            SetImage(pic_1_2, World.LOCATION_ID_ALCHEMIST_HUT);
            SetImage(pic_2_0, World.LOCATION_ID_FARM_FIELD);
            SetImage(pic_2_1, World.LOCATION_ID_FARMHOUSE);
            SetImage(pic_2_2, World.LOCATION_ID_TOWN_SQUARE);
            SetImage(pic_2_3, World.LOCATION_ID_GUARD_POST);
            SetImage(pic_2_4, World.LOCATION_ID_BRIDGE);
            SetImage(pic_2_5, World.LOCATION_ID_SPIDER_FIELD);
            SetImage(pic_3_2, World.LOCATION_ID_HOME);
        }

        private void SetImage(PictureBox pictureBoxTarget, int locationId)
        {
            string imageName;
            if (_player.LocationsVisited.Contains(locationId))
            {
                imageName = World.LocationByID(locationId).ImagePngFilename;
            }
            else
            {
                imageName = "FogLocation.png";
            }

            using (Stream resourceStream = _thisAssembly.GetManifestResourceStream(_thisAssembly.GetName().Name + ".Images." + imageName))
            {
                if (resourceStream != null)
                {
                    pictureBoxTarget.Image = new Bitmap(resourceStream);
                }
            }
        }

    }
}
