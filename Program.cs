using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RazFractal
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//[STAThread]
		static void Main()
		{
			ThreadStart ts = new ThreadStart(StartUIThread);
			uiThread = new Thread(ts);
			uiThread.SetApartmentState(ApartmentState.STA);
			uiThread.Start();
		}

		static Thread uiThread;
		static MainForm mainForm; 

		static void StartUIThread()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			mainForm = new MainForm();
			mainForm.StartRender += mainForm_StartRender;
			mainForm.StopRender += mainForm_StopRender;
			mainForm.ConfigChanged += mainForm_ConfigChanged;
			Application.Run(mainForm);
		}

		static void mainForm_ConfigChanged(object sender, MainForm.ConfigChangedEventArgs args)
		{
			stopRendering = true;
			restart = true;
			config = args.Config;
			mainForm_StartRender(sender,args);
		}

		static void mainForm_StopRender(object sender, EventArgs e)
		{
			stopRendering = true;
		}

		static void mainForm_StartRender(object sender, EventArgs e)
		{
			if (isRendering) { return; }
			do {
				restart = false;
				var sw = new Stopwatch();
				sw.Start();
				DoRender();
				double t = sw.ElapsedMilliseconds;
				mainForm.Status = "Time to render "+(t/1000);
			} while(restart);
		}

		static double escape = 2.0;
		static bool isRendering = false;
		static bool stopRendering = false;
		static bool restart = false;
		static int width = 1024;
		static int height = 1024;
		static FracConfig config;

		static void DoRender()
		{
			if (isRendering) { return; }
			isRendering = true;
			stopRendering = false;

			object[,] lockmania = new object[width,height];
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					lockmania[x,y] = new object();
				}
			}
			double[,] data = new double[width,height];
			var pp = Parallel.For(0,height,(i) => {
				RenderPart(config,i,width,height,data,lockmania);
			});

			Bitmap img = new Bitmap(width,height,PixelFormat.Format32bppArgb);
			var lb = new LockBitmap(img);
			lb.LockBits();
			for(int y=0; y<height; y++) {
				for(int x=0; x<width; x++) {
					double d = data[x,y];
					Color c;
					if (d < 0) {
						c = Color.Black;
					} else {
						var q = d % 100.0;
						int w = (int)Math.Min(255.0,q*(255.0/100.0));
						c = Color.FromArgb(w,w,w);
					}
					lb.SetPixel(x,y,c);
				}
			}
			lb.UnlockBits();
			mainForm.FractalImage = img;
			isRendering = false;
		}

		//static void RenderPart1(int px, int py, int wth, int hth, double[,] data)
		//{
		//	//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
		//	double x,xx,y,cx,cy;
		//	int iter,hx,hy;
		//	double magnify=1.0;
		//	int hxres = data.GetLength(0);
		//	int hyres = data.GetLength(1);
		//	double dist;

		//	for(hy=py; hy<py+hth; hy++) {
		//		for(hx=px; hx<px+wth; hx++) {
		//			cx = (((double)hx)/((double)hxres)-0.8)/magnify * 3.0;
		//			cy = (((double)hy)/((double)hyres)-0.5)/magnify * 3.0;
		//			x = 0.0; y = 0.0;

		//			dist = 0.0;
		//			for(iter = 0; iter < itermax; iter++) {
		//				if (stopRendering) { return; }
		//				xx = x*x-y*y+cx;
		//				y = 2.0*x*y+cy;
		//				x = xx;
		//				dist = x*x+y*y;
		//				if (dist > escape) { break; }
		//			}
		//			//Debug.WriteLine(hx+" "+hy+" "+iter);
		//			data[hx,hy] = iter;
		//		}
		//	}
		//}

		static void RenderPart(FracConfig conf, int hy, int wth, int hth, double[,] data, object[,] lockmania)
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
//main()
//{
//	double x,xx,y,cx,cy;
//	int iteration,hx,hy;
//	int itermax = 100;		/* how many iterations to do	*/
//	double magnify=1.0;		/* no magnification		*/
//	int hxres = 500;		/* horizonal resolution		*/
//	int hyres = 500;		/* vertical resolution		*/

//	/* header for PPM output */
//	printf("P6\n# CREATOR: Eric R Weeks / mandel program\n");
//	printf("%d %d\n255\n",hxres,hyres);

//	for (hy=1;hy<=hyres;hy++)  {
//		for (hx=1;hx<=hxres;hx++)  {
//			cx = (((float)hx)/((float)hxres)-0.5)/magnify*3.0-0.7;
//			cy = (((float)hy)/((float)hyres)-0.5)/magnify*3.0;
//			x = 0.0; y = 0.0;
//			for (iteration=1;iteration<itermax;iteration++)  {
//				xx = x*x-y*y+cx;
//				y = 2.0*x*y+cy;
//				x = xx;
//				if (x*x+y*y>100.0)  iteration = 999999;
//			}
//			if (iteration<99999)  color(0,255,255);
//			else color(180,0,0);
//		}
//	}
//}

//void color(int red, int green, int blue)  {
//	fputc((char)red,stdout);
//	fputc((char)green,stdout);
//	fputc((char)blue,stdout);
//}