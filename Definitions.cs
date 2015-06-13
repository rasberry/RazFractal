using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazFractal
{
	public enum Planes { XY, XW, XZ, YW, YZ, WZ }
	public struct FracConfig
	{
		public double X;
		public double Y;
		public double W;
		public double Z;
		public Planes Plane;
		public double Scale;
	}
}
