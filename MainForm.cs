using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RazFractal
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (StopRender != null) {
				StopRender.Invoke(null,new EventArgs());
			}

			Application.Exit();
		}

		public Image FractalImage { get {
			return pictureBox1.Image;
		} set {
			pictureBox1.Size = value.Size;
			pictureBox1.Image = value;
		}}

		public event EventHandler StartRender;
		public event EventHandler StopRender;

		public string Status {
			get { return this.toolStripStatusLabel1.Text; }
			set { this.toolStripStatusLabel1.Text = value; }
		}

		private void testToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (StartRender != null) {
				StartRender.Invoke(null,new EventArgs());
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pictureBox1.Image.Save("image.png",System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
