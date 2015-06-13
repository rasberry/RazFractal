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
		public event ConfigChangedEventHandler ConfigChanged;

		public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs args);

		public class ConfigChangedEventArgs : EventArgs
		{
			public ConfigChangedEventArgs(FracConfig conf)
			{
				Config = conf;
			}
			public FracConfig Config { get; private set; }
		}

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

		private void FireConfigChanged()
		{
			double scale = 4;
			FracConfig c = new FracConfig {
				Plane = plane
				,X = trackBar1.Value / (50 / scale) - scale
				,Y = trackBar2.Value / (50 / scale) - scale
				,W = trackBar3.Value / (50 / scale) - scale
				,Z = trackBar4.Value / (50 / scale) - scale
				,Scale = 1
			};

			if (ConfigChanged != null) {
				ConfigChanged.Invoke(this, new ConfigChangedEventArgs(c));
			}
		}

		private Planes plane = Planes.XY;

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			FireConfigChanged(); //X changed
		}
		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			FireConfigChanged(); //Y changed
		}
		private void trackBar3_Scroll(object sender, EventArgs e)
		{
			FireConfigChanged(); //W changed
		}
		private void trackBar4_Scroll(object sender, EventArgs e)
		{
			FireConfigChanged(); //Z changed
		}
		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.XY;
			FireConfigChanged();
		}
		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.XW;
			FireConfigChanged();
		}
		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.XZ;
			FireConfigChanged();
		}
		private void radioButton4_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.YW;
			FireConfigChanged();
		}
		private void radioButton5_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.YZ;
			FireConfigChanged();
		}
		private void radioButton6_CheckedChanged(object sender, EventArgs e)
		{
			plane = Planes.WZ;
			FireConfigChanged();
		}
	}
}
