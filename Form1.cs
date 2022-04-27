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
    public partial class Form1 : Form
    {
        //Global variables
        Instrument lightsensor = new Instrument();
        //Database
        string SoftSensDB = ConfigurationManager.ConnectionStrings["SoftSensDB"].ConnectionString;




        //For plotting
        List<int> light = new List<int>();
        List<float> Scaledlight = new List<float>();
        List<DateTime> time1 = new List<DateTime>();
        List<DateTime> time2 = new List<DateTime>();
        //Used for altering between readstauts and readraw/scaled
        int flag2 = 0;
        //Store sensor data
        List<string> sensordata = new List<string>();




        public Form1()
        {
            InitializeComponent();
            //Plot formatting
            chartSeries.Series[0].XValueType = ChartValueType.DateTime;
            chartSeries.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            chartSeries.Series[0].YValueType = ChartValueType.Auto;
            chartSeries.Titles.Add("Live Graph");


            //SerialPort formatting
            ComPortBox.Items.AddRange(SerialPort.GetPortNames());
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataRecievedHandler);
            ComPortBox.Text = "--Select--";
            comboBox1.Text = "--Select--";
            string[] bitRate = new string[] { "1200", "2400", "4800", "9600",
                                              "19200", "38400", "57600", "115200" };
            BitRateBox.Items.AddRange(bitRate);
            BitRateBox.SelectedIndex = BitRateBox.Items.IndexOf("9600");
        }
        void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string RecievedData = ((SerialPort)sender).ReadLine();
            string[] separateParts = RecievedData.Split(';');

            if (RecievedData != "\r")
            {
                BeginInvoke(new Action(() =>
                {
                    uploadpic.Visible = false;
                }));

                SerialOutputBox.Invoke((MethodInvoker)delegate
                { SerialOutputBox.AppendText("Recieved: " + RecievedData + "\r\n"); });
            }

            //Conditions for signals from the sensor
            if (serialPort1.IsOpen)
            {
                if (separateParts[0] == "read")
                {

                    separateParts[3] = separateParts[3].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    //Storing raw data
                    int Ivab = int.Parse(separateParts[2]);

                    //Dot symbol "." is not used as separator (this depends on Culture settings)
                    //https://stackoverflow.com/questions/1014535/float-parse-doesnt-work-the-way-i-wanted
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    float Scaled_ivab = float.Parse(String.Format("{0:0.00}", separateParts[3]), NumberStyles.Any, ci);
                    int errorcode = int.Parse(separateParts[1]);

                    double Data_Scaled_ivab = double.Parse(separateParts[3], NumberStyles.Any, ci);



                    try
                    {
                        //Oppretter en connection mot databasen med string definert i App.config:
                        SqlConnection con = new SqlConnection(SoftSensDB);
                        //Kaller på database klassen for å uploade datapoints
                        Database.upload(lightsensor.Tagname, Ivab, Scaled_ivab, errorcode, con);
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                    if (buttonraw.Checked)
                    {


                        //Raw data plotting and data handling
                        rawbox.Invoke((Action)delegate
                        {
                            rawbox.Text = separateParts[2];
                        });
                        //Showing scaled data as well
                        BeginInvoke(new Action(() =>
                        {
                            scaledbox.Text = separateParts[3];
                        }));
                        //Making the list the correct format
                        sensordata.Add(DateTime.Now.ToString() + ";" + "Value: " + Ivab);

                        //Code for plotting
                        DateTime dt = DateTime.Now;
                        light.Add(Ivab);
                        time1.Add(dt);

                        BeginInvoke(new Action(() =>
                        {
                            chartSeries.Titles.Clear();
                            chartSeries.Series["Value"].LabelFormat = "{0}";
                            chartSeries.Series["Value"].Points.DataBindXY(time1, light);
                            chartSeries.Series["Value"].BorderWidth = 3;
                            chartSeries.ChartAreas[0].AxisX.Title = "Time";
                            chartSeries.ChartAreas[0].AxisY.Title = "Raw";
                            chartSeries.Titles.Add("Graph of Raw sensor values");
                            chartSeries.Invalidate();
                            //Making sure the graph does not get clutterd with old datapoints
                            if (light.Count > 8)
                            {
                                light.RemoveAt(0);
                                time1.RemoveAt(0);
                            }
                        }));
                    }
                    else if (buttonscaled.Checked)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            scaledbox.Text = separateParts[3];
                        }));

                        //Showing raw data aswell
                        rawbox.Invoke((Action)delegate
                        {
                            rawbox.Text = separateParts[2];
                        });
                        //Making the list the correct format
                        sensordata.Add(DateTime.Now.ToString() + ";" + "Value: " + Scaled_ivab + ";\r\n");

                        //Code for plotting
                        DateTime dt = DateTime.Now;
                        Scaledlight.Add(Scaled_ivab);
                        time2.Add(dt);

                        BeginInvoke(new Action(() =>
                        {
                            chartSeries.Titles.Clear();
                            chartSeries.Series["Value"].LabelFormat = "{0.00}";
                            chartSeries.Series["Value"].Points.DataBindXY(time2, Scaledlight);
                            chartSeries.Series["Value"].BorderWidth = 3;
                            chartSeries.ChartAreas[0].AxisX.Title = "Time";
                            chartSeries.ChartAreas[0].AxisY.Title = "Scaled";
                            chartSeries.Titles.Add("Graph of Scaled sensor values");
                            chartSeries.Invalidate();
                            //Making sure the graph does not get clutterd with old datapoints
                            if (Scaledlight.Count > 8)
                            {
                                Scaledlight.RemoveAt(0);
                                time2.RemoveAt(0);
                            }
                        }));
                    }
                    //Conditonals for alarms
                    if (separateParts[1] == "0")
                        BeginInvoke(new Action(() =>
                        {
                            alarmstatus.Text = "Alarm status: Ok";
                            alarmbox.AppendText("Alarm Status: Ok\r\n");
                            alarmbox.BackColor = Color.White;
                        }));

                    else if (separateParts[1] == "1")
                        BeginInvoke(new Action(() =>
                        {
                            autotimer.Stop();
                            alarmstatus.Text = "Alarm: Fail";
                            MessageBox.Show("Something went wrong, please check the device");
                            alarmbox.AppendText("Alarm: Fail\r\n");
                            alarmbox.BackColor = Color.Red;
                            Disconnected();

                        }));

                    else if (separateParts[1] == "2")
                        BeginInvoke(new Action(() =>
                        {
                            alarmstatus.Text = "Alarm: Low value";
                            alarmbox.AppendText("Alarm: Low Value\r\n");
                            alarmbox.BackColor = Color.Yellow;
                        }));

                    else if (separateParts[1] == "3")
                        BeginInvoke(new Action(() =>
                        {
                            alarmstatus.Text = "Alarm: High value";
                            alarmbox.AppendText("Alarm: High Value\r\n");
                            alarmbox.BackColor = Color.Yellow;
                        }));
                }
                else if (separateParts[0] == "writeconf")
                {
                    BeginInvoke(new Action(() =>
                    {
                        uploadpic.Visible = false;
                    }));

                    //If password is successful
                    if (separateParts[1] == "1\r")
                    //Update sensor
                    {
                        MessageBox.Show("Upload successful");
                        //Dot symbol "." is not used as separator (this depends on Culture settings)
                        //https://stackoverflow.com/questions/1014535/float-parse-doesnt-work-the-way-i-wanted
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";
                        lightsensor.Tagname = namebox.Text;
                        lightsensor.lrv = int.Parse(String.Format("{0}", lrvbox.Text), NumberStyles.Any, ci);
                        lightsensor.urv = int.Parse(String.Format("{0}", urvbox.Text), NumberStyles.Any, ci);
                        lightsensor.alarmlow = int.Parse(String.Format("{0}", alarmlbox.Text), NumberStyles.Any, ci);
                        lightsensor.alarmhigh = int.Parse(String.Format("{0}", alarmhbox.Text), NumberStyles.Any, ci);

                        BeginInvoke(new Action(() =>
                        {
                            textBox5.Text = lightsensor.Tagname;
                            textBox4.Text = lightsensor.lrv.ToString();
                            textBox3.Text = lightsensor.urv.ToString();
                            textBox2.Text = lightsensor.alarmlow.ToString();
                            textBox1.Text = lightsensor.alarmhigh.ToString();
                        }));
                    }                       
                    else
                        MessageBox.Show("Upload failed - Wrong Password");
                }

                else if (separateParts[0] == "readconf")
                {
                    lightsensor.Tagname = separateParts[1];
                    //Dot symbol "." is not used as separator (this depends on Culture settings)
                    //https://stackoverflow.com/questions/1014535/float-parse-doesnt-work-the-way-i-wanted
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";

                    lightsensor.lrv = int.Parse(String.Format("{0}", separateParts[2]), NumberStyles.Any, ci);
                    lightsensor.urv = int.Parse(String.Format("{0}", separateParts[3]), NumberStyles.Any, ci);
                    lightsensor.alarmlow = int.Parse(String.Format("{0}", separateParts[4]), NumberStyles.Any, ci);
                    lightsensor.alarmhigh = int.Parse(String.Format("{0}", separateParts[5]), NumberStyles.Any, ci);
                    BeginInvoke(new Action(() =>
                    {
                        textBox5.Text = lightsensor.Tagname;
                        textBox4.Text = lightsensor.lrv.ToString();
                        textBox3.Text = lightsensor.urv.ToString();
                        textBox2.Text = lightsensor.alarmlow.ToString();
                        textBox1.Text = lightsensor.alarmhigh.ToString();
                    }));
                }
            }
            else
                MessageBox.Show("Are you connected?");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (ComPortBox.Text == "--Select--")
                {
                    MessageBox.Show("Please choose a Com Port");
                    label3.BackColor = Color.Red;
                    constatusbox.Clear();
                    constatusbox.Text = "Connection failed";
                    constatusbox.BackColor = Color.Red;
                }
                else if (ComPortBox.Text.Contains("COM"))
                {
                    serialPort1.PortName = ComPortBox.Text;
                    serialPort1.BaudRate = Int32.Parse(BitRateBox.Text);
                    serialPort1.Open();
                    label3.BackColor = Color.Green;
                    //Disable the buttons I dont want pressed again
                    connectbutton.Enabled = false;
                    ComPortBox.Enabled = false;
                    BitRateBox.Enabled = false;
                    ComPortBox.Enabled = false;
                    constatusbox.Clear();
                    constatusbox.Text = "Connection Established";
                    constatusbox.BackColor = Color.Green;
                    //Get the instrument name and configs as soon as we are connected
                    serialPort1.WriteLine("readconf");
                }
                else
                {
                    constatusbox.Clear();
                    constatusbox.Text = "Connection failed";
                    constatusbox.BackColor = Color.Red;
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Nothing connected, try and plug it in again");
            }
            if (serialPort1.IsOpen)
            {
                label3.Text = "Status: Connected";
                dcbutton.Enabled = true;
            }
            else
            {
                Disconnected();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                connectbutton.Enabled = true;
                ComPortBox.Enabled = true;
                BitRateBox.Enabled = true;
                label3.Text = "Status: Not Connected";
                constatusbox.Text = "Connection failed";
                label3.BackColor = Color.Red;
                connectbutton.Enabled = true;
                dcbutton.Enabled = false;
                constatusbox.Clear();
                constatusbox.Text = "Disconnected";
                constatusbox.BackColor = Color.White;
                ComPortBox.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show("ERROR! " + ex);
            }



        }



        private void ComPortBox_MouseClick(object sender, MouseEventArgs e)
        {
            ComPortBox.Items.Clear();
            ComPortBox.Text = "--Select--";
            ComPortBox.Items.AddRange(SerialPort.GetPortNames());

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (namebox.Text == string.Empty || lrvbox.Text == string.Empty || urvbox.Text == string.Empty || alarmlbox.Text == string.Empty || alarmhbox.Text == string.Empty)
            {
                MessageBox.Show("Please fill all out paramters");
            }
            else
            {
                saveFileDialog1.Filter = "ssc files (*.ssc)|*.ssc";
                saveFileDialog1.ShowDialog();
            }
        }
        public void button5_Click_1(object sender, EventArgs e)
        {
            //Check if user is scanning
            if (flag2 == 0)
            {
                //Make sure user does not apply all empty paramaters
                if (namebox.Text == string.Empty || lrvbox.Text == string.Empty || urvbox.Text == string.Empty || alarmlbox.Text == string.Empty || alarmhbox.Text == string.Empty)
                    MessageBox.Show("Please fill out some valid paramters");
                else
                {
                    string userinput = Interaction.InputBox("Enter Password", "Upload Config", " ", 600, 400);
                    //If the user clicks Cancel, a zero-length string is returned.
                    if (userinput != "")
                    {
                        if (serialPort1.IsOpen)
                        {
                            //Make sure user does not apply all empty paramaters
                            if (namebox.Text == string.Empty || lrvbox.Text == string.Empty || urvbox.Text == string.Empty || alarmlbox.Text == string.Empty || alarmhbox.Text == string.Empty)
                                MessageBox.Show("Please fill out some valid paramters");
                            else
                            {
                                uploadpic.Visible = true;
                                serialPort1.WriteLine("writeconf>" + userinput + ">" + namebox.Text + ";" + lrvbox.Text + ";" + urvbox.Text + ";" + alarmlbox.Text + ";" + alarmhbox.Text);

                            }
                        }
                        else
                        {
                            Disconnected();
                            MessageBox.Show("Upload failed. Are you connected?");
                        }
                    }

                }

            }
            else
            {
                MessageBox.Show("Please stop live data sensor collecting before uploading new paramaters");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("readconf");
                uploadpic.Visible = true;
            }
            else
            {
                Disconnected();
                MessageBox.Show("Are you connected?");
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // Get file name.
            string name = saveFileDialog1.FileName;
            // Making an array of textboxes
            TextBox[] boxer = { namebox, lrvbox, urvbox, alarmlbox, alarmhbox };
            string[] config = new string[5];
            //Iteration through the textboxes to get the config paramters into an array
            for (int i = 0; i < 5; i++)
            {
                config[i] = (boxer[i].Text.ToString());
            }
            File.WriteAllLines(name, config);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string[] fileContent = new string[5];
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "ssc files (*.ssc)|*.ssc";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        fileContent[i] = reader.ReadLine();
                    }
                }
            }
            //Making an array of textboxes
            TextBox[] boxer = { namebox, lrvbox, urvbox, alarmlbox, alarmhbox };
            string[] config = new string[5];
            //Iteration through the textboxes to put in the paramters
            for (int i = 0; i < 5; i++)
            {
                boxer[i].Text = fileContent[i];
            }
        }

        private void autotimer_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (buttonraw.Checked)
                {
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.WriteLine("readalldata");
                    }
                }
                else if (buttonscaled.Checked)
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.WriteLine("readalldata");
                    }
                    else
                    {
                        MessageBox.Show("Please choose either Raw or Scaled");
                        autotimer.Stop();
                    }
            }
            else
            {

                Disconnected();
                //Stop the timer to not go into infinite loop
                autotimer.Stop();
                alarmstatus.Text = "";
                datacollectionstatus.Text = "";
                MessageBox.Show("Device not connected");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                //flag is set to stop user from uploading while scanning
                flag2 = 1;
                pictureBox1.Visible = true;
                if (buttonraw.Checked)
                {
                    serialPort1.WriteLine("readalldata");
                }
                else if (buttonscaled.Checked)
                {
                    serialPort1.WriteLine("readalldata");
                }
                //Starter timer for live data
                //Check to see if user has entered specific frequency if not default = 2500ms
                if (freqbox.Text == "")
                {
                    autotimer.Interval = 2500;
                    autotimer.Start();
                    //Enable label for collection status and alarm status
                    datacollectionstatus.BackColor = Color.Green;
                    datacollectionstatus.Text = "Online";
                    alarmstatus.Text = "No alarms";
                    //Disable the button to avoid problems
                    button6.Enabled = false;
                    buttonraw.Enabled = false;
                    buttonscaled.Enabled = false;
                    //Enable stop button
                    button7.Enabled = true;
                }
                else
                {
                    autotimer.Interval = int.Parse(freqbox.Text);
                    autotimer.Start();
                    //Enable label for collection status and alarm status
                    datacollectionstatus.BackColor = Color.Green;
                    datacollectionstatus.Text = "Online";
                    alarmstatus.Text = "No alarms";
                    //Disable the button to avoid problems
                    button6.Enabled = false;
                    buttonraw.Enabled = false;
                    buttonscaled.Enabled = false;
                    //Enable stop button
                    button7.Enabled = true;
                }

            }
            else
            {
                Disconnected();
                MessageBox.Show("Are you connected?");
            }
        }

        private void lrvbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //https://stackoverflow.com/questions/463299/how-do-i-make-a-textbox-that-only-accepts-numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                    (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void urvbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //https://stackoverflow.com/questions/463299/how-do-i-make-a-textbox-that-only-accepts-numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                    (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void alarmlbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //https://stackoverflow.com/questions/463299/how-do-i-make-a-textbox-that-only-accepts-numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void alarmhbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //https://stackoverflow.com/questions/463299/how-do-i-make-a-textbox-that-only-accepts-numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Set flag to 0 so user can upload
            flag2 = 0;
            //Stop the timer and hide the loading gif
            autotimer.Stop();
            pictureBox1.Visible = false;
            //Reset label for datacollection status
            datacollectionstatus.Text = "";
            //Prompt the user to save the data
            var res = MessageBox.Show("Do you want to save the sensor data locally?", "Sensor Data", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                savedatafile.Filter = "csv files (*.csv)|*.csv";
                savedatafile.ShowDialog();
            }
            //Deletes the data to make room for new data
            else if (res == DialogResult.No)
                sensordata.Clear();
            //Clear chart and get ready for new plotting
            chartSeries.Series["Value"].Points.Clear();
            time1.Clear();
            time2.Clear();
            light.Clear();
            Scaledlight.Clear();
            chartSeries.Titles.Clear();
            scaledbox.Clear();
            rawbox.Clear();
            //Enable start button and radiobuttons
            button6.Enabled = true;
            buttonraw.Enabled = true;
            buttonscaled.Enabled = true;
            //Disable button to avoid problems
            button7.Enabled = false;
        }

        private void savedatafile_FileOk(object sender, CancelEventArgs e)
        {

            // Get file name.
            string name = savedatafile.FileName;
            string joinedlist = String.Join("", sensordata);
            File.WriteAllText(name, joinedlist);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox1 = new AboutBox1();
            aboutBox1.Show(this);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form2 helpinfo = new Form2();
            helpinfo.ShowDialog();
        }

        public void Disconnected()
        {
            //Reset form after a disconnect to not cause bugs
            label3.Text = "Status: Not Connected";
            label3.BackColor = Color.Red;
            connectbutton.Enabled = true;
            dcbutton.Enabled = false;
            ComPortBox.Enabled = true;
            BitRateBox.Enabled = true;
            constatusbox.Clear();
            constatusbox.Text = "Connection failed";
            constatusbox.BackColor = Color.Red;
            chartSeries.Titles.Clear();
            time1.Clear();
            time2.Clear();
            light.Clear();
            Scaledlight.Clear();
            pictureBox1.Visible = false;
            chartSeries.Series["Value"].Points.Clear();
            autotimer.Stop();
            serialPort1.Close();
            button7.Enabled = false;
            button6.Enabled = true;
            rawbox.Clear();
            scaledbox.Clear();
            alarmbox.Clear();
            alarmbox.BackColor = Color.White;
            buttonraw.Enabled = true;
            buttonscaled.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (buttonraw.Checked)
                {
                    serialPort1.WriteLine("readraw");
                }
                else if (buttonscaled.Checked)
                {
                    serialPort1.WriteLine("readscaled");
                }
            }
            else
            {
                Disconnected();
                MessageBox.Show("Are you connected?");
            }
        }

        private void buttonscaled_CheckedChanged(object sender, EventArgs e)
        {
            //Clear chart and get ready for new plotting
            chartSeries.Series["Value"].Points.Clear();
            time1.Clear();
            time2.Clear();
            light.Clear();
            Scaledlight.Clear();
        }

        private void buttonraw_CheckedChanged(object sender, EventArgs e)
        {
            //Clear chart and get ready for new plotting
            chartSeries.Series["Value"].Points.Clear();
            time1.Clear();
            time2.Clear();
            light.Clear();
            Scaledlight.Clear();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //Flag to avoid user from uploading while live data is collecting
            if (flag2 == 0)
            {
                if (serialPort1.IsOpen)
                {
                    uploadpic.Visible = true;
                    serialPort1.WriteLine("writeconf>password>AXP-3000;0.0;500.0;40;440");
                }
                else
                {
                    Disconnected();
                    MessageBox.Show("Upload failed. Are you connected?");
                }
            }
            else
            {
                MessageBox.Show("Please stop live data sensor collecting before uploading new paramaters");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("readalldata");
                uploadpic.Visible = true;
            }
            else
            {
                Disconnected();
                MessageBox.Show("Are you connected?");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {

            try
            {
                SqlConnection con = new SqlConnection(SoftSensDB);
                string name = comboBox1.SelectedItem.ToString();
                //Kaller på database klassen for å laste ned config for instrumentet
                Database.downloadconf(this, name, con);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.Text = "--Select--";
                List<string> sqlResult = new List<string>();
                SqlConnection con = new SqlConnection(SoftSensDB);

                sqlResult = Database.GetInstrumentNamesFromDB(con);
                foreach (string TagName in sqlResult)
                {
                    comboBox1.Items.Add(TagName);
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("ERROR! " + ex);
            }
            
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //Dot symbol "." is not used as separator (this depends on Culture settings)
            //https://stackoverflow.com/questions/1014535/float-parse-doesnt-work-the-way-i-wanted
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            lightsensor.lrv = int.Parse(String.Format("{0}", textBox4.Text), NumberStyles.Any, ci);
            lightsensor.urv = int.Parse(String.Format("{0}", textBox3.Text), NumberStyles.Any, ci);
            lightsensor.alarmlow = int.Parse(String.Format("{0}", textBox2.Text), NumberStyles.Any, ci);
            lightsensor.alarmhigh = int.Parse(String.Format("{0}", textBox1.Text), NumberStyles.Any, ci);
            SqlConnection con = new SqlConnection(SoftSensDB);
            Database.uploadconf(this,lightsensor.Tagname, lightsensor.lrv,lightsensor.urv,lightsensor.alarmlow,lightsensor.alarmhigh, con);
        }


        private void freqbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //https://stackoverflow.com/questions/463299/how-do-i-make-a-textbox-that-only-accepts-numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public void button11_Click_1(object sender, EventArgs e)
        {


            SqlConnection con = new SqlConnection(SoftSensDB);
            Database.Jobs(this, lightsensor.Tagname, con);
            
        }

        private void jobstimer_Tick(object sender, EventArgs e)
        {
            List<string> Task = new List<string>();
            SqlConnection con = new SqlConnection(SoftSensDB);
            Task = Database.Jobs(this, lightsensor.Tagname, con);
            if (Task.Count > 1)
            {
                serialPort1.WriteLine("output;" + Task[2]);
                alarmbox.AppendText("Task completed, output:" + Task[1] + "\r\n");
                idbox.Clear();
                idbox.AppendText(Task[0]);
                Database.updatejobs(Task[0], DateTime.Now, con);
            }
            else
            {
                alarmbox.AppendText("All Tasks completed" + "\r\n");
                jobstimer.Stop();
            }
        }

        private void button11_Click_2(object sender, EventArgs e)
        {
            jobstimer.Start();
        }
    }
}



