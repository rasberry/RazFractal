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
			Application.Run(mainForm);
		}

		static void mainForm_StopRender(object sender, EventArgs e)
		{
			stopRendering = true;
		}

		static void mainForm_StartRender(object sender, EventArgs e)
		{
			var sw = new Stopwatch();
			sw.Start();
			DoRender();
			double t = sw.ElapsedMilliseconds;
			mainForm.Status = "Time to render "+(t/1000);
		}

		static int itermax = 1000;
		static double escape = 2.0;
		static bool isRendering = false;
		static bool stopRendering = false;
		static void DoRender()
		{
			if (isRendering) { return; }
			isRendering = true;
			stopRendering = false;
			int divs = 4;
			int side = 1024;
			double[,] data = new double[side,side];
			var pp = Parallel.For(0,divs*divs,(i) => {
				int w = side/divs;
				int y = (i / divs)*w;
				int x = (i % divs)*w;
				Debug.WriteLine(x+" "+y+" "+w+" "+data.GetHashCode());
				RenderPart(x,y,w,w,data);
			});

			//RenderPart(128,128,768,768,data);

			Bitmap img = new Bitmap(side,side,PixelFormat.Format32bppArgb);
			var lb = new LockBitmap(img);
			lb.LockBits();
			for(int y=0; y<side; y++) {
				for(int x=0; x<side; x++) {
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

		static void RenderPart1(int px, int py, int wth, int hth, double[,] data)
		{
			//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
			double x,xx,y,cx,cy;
			int iter,hx,hy;
			double magnify=1.0;
			int hxres = data.GetLength(0);
			int hyres = data.GetLength(1);
			double dist;

			for(hy=py; hy<py+hth; hy++) {
				for(hx=px; hx<px+wth; hx++) {
					cx = (((double)hx)/((double)hxres)-0.8)/magnify * 3.0;
					cy = (((double)hy)/((double)hyres)-0.5)/magnify * 3.0;
					x = 0.0; y = 0.0;

					dist = 0.0;
					for(iter = 0; iter < itermax; iter++) {
						if (stopRendering) { return; }
						xx = x*x-y*y+cx;
						y = 2.0*x*y+cy;
						x = xx;
						dist = x*x+y*y;
						if (dist > escape) { break; }
					}
					//Debug.WriteLine(hx+" "+hy+" "+iter);
					data[hx,hy] = iter;
				}
			}
		}

		static void RenderPart(int px, int py, int wth, int hth, double[,] data)
		{
			//http://www.physics.emory.edu/faculty/weeks//software/mandel.c
			int iter,hx,hy;
			int itermax = 100;
			Complex magnify = new Complex(1.0,1.0);
			int hxres = data.GetLength(0);
			int hyres = data.GetLength(1);
			Complex res = new Complex((double)hxres,(double)hyres);

			for(hy=py; hy<py+hth; hy++) {
				for(hx=px; hx<px+wth; hx++) {
					var p = new Complex((double)hx,(double)hy);
					var c = ((p / res) + new Complex(-0.5,-0.3)) / magnify * new Complex(3.0,3.0);

					var z = new Complex(0.0,0.0);
					for(iter = 0; iter < itermax; iter++) {
						if (stopRendering) { return; }
						z = z*z + c;
						var dist = z.Magnitude;
						if (dist > escape || double.IsNaN(dist) || double.IsInfinity(dist)) { dist = -1; break; }
					}
					data[hx,hy] = iter;
				}
			}
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