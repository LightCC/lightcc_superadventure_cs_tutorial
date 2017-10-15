using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class WorldMap : Form
    {
        readonly Assembly _thisAssembly = Assembly.GetExecutingAssembly();

        public WorldMap()
        {
            InitializeComponent();

            SetImage(pic_0_2, "HerbalistsGarden");
            SetImage(pic_1_2, "HerbalistsHut");
            SetImage(pic_2_0, "FarmFields");
            SetImage(pic_2_1, "Farmhouse");
            SetImage(pic_2_2, "TownSquare");
            SetImage(pic_2_3, "TownGate");
            SetImage(pic_2_4, "Bridge");
            SetImage(pic_2_5, "SpiderForest");
            SetImage(pic_3_2, "Home");
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
