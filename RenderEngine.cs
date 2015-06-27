using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RazFractal
{
	public class RenderEngine
	{
		public delegate void RenderFinishedEventHandler(object sender, RenderFinishedEventArgs args);
		public event RenderFinishedEventHandler RenderFinished;

		public void Start()
		{
			while(live) {
				Thread.Sleep(500);
			}
		}
		public void Exit(object sender, EventArgs e)
		{
			stopRendering = true;
			restart = false;
			live = false;
		}

		public void ConfigChanged(object sender, ConfigChangedEventArgs args)
		{
			stopRendering = true;
			restart = true;
			config = args.Config;
			StartRender(sender,args);
		}

		public void StopRender(object sender, EventArgs e)
		{
			stopRendering = true;
		}

		public void StartRender(object sender, EventArgs e)
		{
			if (isRendering) { return; }
			do {
				restart = false;
				var sw = new Stopwatch();
				sw.Start();
				Bitmap img = DoRender();
				double t = sw.ElapsedMilliseconds;
				if (RenderFinished != null) {
					RenderFinished.Invoke(this, new RenderFinishedEventArgs(img, t/1000.0));
				}
			} while(restart);
		}

		bool live = true;
		double escape = 4.0;
		bool isRendering = false;
		bool stopRendering = false;
		bool restart = false;
		int width = 1024;
		int height = 1024;
		FracConfig config;
		Bitmap last;

		Bitmap DoRender()
		{
			if (isRendering) { return last; }
			isRendering = true;
			stopRendering = false;

			object[,] lockmania = new object[width,height]; //TODO this is crazy.. but not sure how else to gurantee single pixel thread safety
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					lockmania[x,y] = new object();
				}
			}
			double[,] data = new double[width,height];

			var pp = Parallel.For(0,height,(i) => {
				RenderPart(config,i,width,height,data,lockmania);
			});

			double min = double.MaxValue,max = double.MinValue;
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = data[x,y];
					if (d > 0) { d = Math.Log10(d); }
					if (d < min) { min = d; }
					if (d > max) { max = d; }
				}
			}
			double range = Math.Abs(max - min);
			double mult = 255.0/range;

			Bitmap img = new Bitmap(width,height,PixelFormat.Format32bppArgb);
			var lb = new LockBitmap(img);
			lb.LockBits();
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = data[x,y];
					if (d > 0) { d = Math.Log10(d); }
					Color c;
					if (d <= 0) {
						c = Color.Black;
					} else {
						double q = d*mult - min;
						//int w = (int)Math.Min(255.0,Math.Max(0,q));
						int w = (int)q;
						c = Color.FromArgb(w,w,w);
					}
					lb.SetPixel(x,y,c);
				}
			}
			lb.UnlockBits();
			last = img;
			isRendering = false;
			return img;
		}

		void RenderPart(FracConfig conf, int hy, int wth, int hth, double[,] data, object[,] lockmania)
		{
			//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
			int iter,hx;
			int itermax = 100;
			double magnify = 1.0;
			int hxres = data.GetLength(0);
			int hyres = data.GetLength(1);
			double xoff = -0.8;
			double yoff = -0.5;
			Complex res = new Complex((double)hxres,(double)hyres);

			for(hx=0; hx<wth; hx++)
			{
				double cx = WinToWorld(hx, magnify, hxres, xoff);
				double cy = WinToWorld(hy, magnify, hyres, yoff);

				Complex c,z;
				switch(conf.Plane)
				{
				case Planes.XY: default:
					c = new Complex(cx,cy);
					z = new Complex(conf.X,conf.Y); break;
				case Planes.XW:
					c = new Complex(conf.W,cy);
					z = new Complex(conf.X,cx); break;
				case Planes.XZ:
					c = new Complex(cx,conf.Z);
					z = new Complex(conf.X,cy); break;
				case Planes.YW:
					c = new Complex(conf.W,cx);
					z = new Complex(cy,conf.Y); break;
				case Planes.YZ:
					c = new Complex(cx,conf.Z);
					z = new Complex(cy,conf.Y); break;
				case Planes.WZ:
					c = new Complex(conf.W,conf.Z);
					z = new Complex(cx,cy); break;
				}

				for(iter = 0; iter < itermax; iter++) {
					if (stopRendering) { return; }

					//z = Complex.Pow(z*z,c*4)+c;
					z = z*z + c;
					int bx = WorldToWin(z.Real,magnify,hxres,xoff);
					int by = WorldToWin(z.Imaginary,magnify,hyres,yoff);
					if (bx > 0 && bx < wth && by > 0 && by < hth) {
						lock(lockmania[bx,by]) {
							data[bx,by]++;
						}
					}

					var dist = z.Magnitude;
					if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
				}
				////smooth coloring
				//double index = iter;
				//if (iter < itermax)
				//{
				//	//double zn = Math.Sqrt(z.Real*z.Real+z.Imaginary*z.Imaginary);
				//	double zn = z.Magnitude;
				//	double nu = Math.Log(Math.Log(zn,2),2);
				//	index = iter + 1.0 - nu;
				//}
				//data[hx,hy] = index;
			}
		}

		static double WinToWorld(int h, double magnify, int res, double offset)
		{
			return (((double)h) / ((double)res) + offset) / magnify * 3.0;
		}
		static int WorldToWin(double w, double magnify, int res, double offset)
		{
			return (int)Math.Round((double)res * (magnify * w - 3.0 * offset) / 3.0);
		}
	}
}
