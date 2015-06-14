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

		public Image FractalImage { get {
			return pictureBox1.Image;
		} set {
			pictureBox1.Size = value.Size;
			pictureBox1.Image = value;
		}}

		public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs args);
		public event ConfigChangedEventHandler ConfigChanged;

		public class ConfigChangedEventArgs : EventArgs
		{
			public ConfigChangedEventArgs(FracConfig conf)
			{
				Config = conf;
			}
			public FracConfig Config { get; private set; }
		}

		public delegate void ScrollChangedEventHandler(object sender, ScrollChangedEventArgs args);
		public event ScrollChangedEventHandler ScrollChanged;

		public class ScrollChangedEventArgs : EventArgs
		{
			public ScrollChangedEventArgs(Point offset)
			{
				Offset = offset;
			}
			public Point Offset { get; private set; }
		}

		public event EventHandler StartRender;
		public event EventHandler StopRender;

		public string Status {
			get { return this.toolStripStatusLabel1.Text; }
			set { this.toolStripStatusLabel1.Text = value; }
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (StopRender != null) {
				StopRender.Invoke(null,new EventArgs());
			}

			Application.Exit();
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

		bool isDown = false;
		bool isFirst = false;
		Point lastLoc = Point.Empty;

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			//Console.WriteLine("m dn "+e.Location);
			if (e.Button == MouseButtons.Middle) {
 				isDown = true;
				lastLoc = e.Location;
			}
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDown) {
				if (isFirst) {
					isFirst = false;
					lastLoc = e.Location;
					return;
				}
				//Console.WriteLine("m move "+e.Location);
				int dx = lastLoc.X - e.X;
				int dy = lastLoc.Y - e.Y;
				lastLoc = e.Location;

				if ((dx != 0 || dy != 0) && ScrollChanged != null)
				{
					ScrollChanged.Invoke(this,new ScrollChangedEventArgs(new Point(dx,dy)));
				}
			}
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			//Console.WriteLine("m up "+e.Location);
			if (e.Button == MouseButtons.Middle) {
 				isDown = false;
				lastLoc = e.Location;
			}
		}

		//void MoveScroll(int dist, bool keyboard = false)
		//{
		//	ScrollChanged

		//	if (sp.Value + dist < sp.Minimum) {
		//		sp.Value = sp.Minimum;
		//	}
		//	else if (sp.Value + dist > sp.Maximum) {
		//		sp.Value = sp.Maximum;
		//	}
		//	else {
		//		sp.Value += dist;
		//		isFirst = !keyboard; //setting the scrollbar moves the mouse location so skip the next move event
		//	}
		//}

		//private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		//{
		//	Console.WriteLine("k dn "+e.KeyCode);
		//	int dx = 0, dy = 0, dist = 0;
		//	switch(e.KeyCode)
		//	{
		//	case Keys.Up:
		//	case Keys.Down:
		//		dist = e.Shift ? pictureBox1.HorizontalScroll.LargeChange : pictureBox1.HorizontalScroll.SmallChange;
		//		break;
		//	case Keys.Right:
		//	case Keys.Left:
		//		dist = e.Shift ? pictureBox1.VerticalScroll.LargeChange : pictureBox1.VerticalScroll.SmallChange;
		//		break;
		//	}

		//	switch(e.KeyCode)
		//	{
		//	case Keys.Up:		dy = -dist; break;
		//	case Keys.Left:		dx = -dist; break;
		//	case Keys.Down:		dy = dist; break;
		//	case Keys.Right:	dx = dist; break;
		//	}
		//	if (dx != 0) { MoveScroll(panel.HorizontalScroll,dx,true); }
		//	if (dy != 0) { MoveScroll(panel.VerticalScroll,dy,true); }
		//}
	}
}
