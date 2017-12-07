using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GFDFontRenderer.GFD;

namespace GFDFontRenderer
{
    public partial class GFDForm : Form
    {
        public GFDTextRenderer font;

        public GFDForm()
        {
            InitializeComponent();

            //Initialize Renderer
            font = new GFDTextRenderer("use.gfd", "use_", "_AM_NOMIP", 15, 18);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            var img = Graphics.FromImage(bmp);
            img.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));

            //Draw Text
            font.DrawText(img, textBox.Text, new Point(0, 0), Color.White);

            pictureBox1.Image = bmp;
        }
    }
}
