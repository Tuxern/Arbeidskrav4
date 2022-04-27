using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
//Importing visual basic to use input box
using Microsoft.VisualBasic;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;

namespace Arbeidskrav1
{
	public class Instrument
	{
		public Instrument() { } //Constructor

		//Variabler get; set; gjør det mulig å endre (get) og lese (set)
		public string Tagname { get; set; }
		public int lrv { get; set; } = 0;
		public int urv { get; set; } = 0;
		public int alarmlow { get; set; } = 0;
		public int alarmhigh { get; set; } = 0;

	}
}
