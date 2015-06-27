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
			Init();

			ThreadStart uts = new ThreadStart(StartUIThread);
			uiThread = new Thread(uts);
			uiThread.SetApartmentState(ApartmentState.STA);
			uiThread.Start();

			ThreadStart rts = new ThreadStart(StartRenderThread);
			renThread = new Thread(rts);
			renThread.SetApartmentState(ApartmentState.STA);
			renThread.Start();
		}

		static void Init()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
	
			Engine = new RenderEngine();
			MainForm = new MainForm();

			MainForm.StartRender += Engine.StartRender;
			MainForm.StopRender += Engine.StopRender;
			MainForm.ConfigChanged += Engine.ConfigChanged;
			Engine.RenderFinished += MainForm.RenderFinished;
			Application.ApplicationExit += Engine.Exit;
		}

		static Thread uiThread;
		static Thread renThread;
		public static MainForm MainForm;
		public static RenderEngine Engine;

		static void StartRenderThread()
		{
			Engine.Start();
		}

		static void StartUIThread()
		{
			Application.Run(MainForm);
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