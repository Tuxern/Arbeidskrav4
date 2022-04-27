using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeidskrav1
{
	public class Instrument
	{
		public Instrument() { } //Constructor

		public string Tagname { get; set; }
		public double lrv { get; set; } = 0.0;
		public double urv { get; set; } = 0.0;
		public int alarmlow { get; set; } = 0.0;
		public int alarmhigh { get; set; } = 0.0;




	}
}

