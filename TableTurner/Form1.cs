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
using System.Drawing.Drawing2D;
using AForge.Imaging.Filters;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace TableTurner
{
    public partial class Form1 : Form
    {
        int modeTracker = 0;
        String stringIndex = "i/n";
        String readEnc = "e/n";
        String path;
        int degressPerSec = 781;
        Image img;
        int lastAngle = 0;

        public Form1()
        {
            InitializeComponent();
            
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] Ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(Ports);
            try { comboBox1.SelectedIndex = 0; } catch { }

            ClosePort.Enabled = false;

            img = TableTurner.Properties.Resources.Compass_Transparent;
            pictureBox1.Image = img;
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton2.Checked = false;
            radioButton3.Checked = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton3.Checked = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
        }

        //SERIAL CONNECTION AND SETUP
        private void Connect_Click(object sender, EventArgs e)
        {
            Connect.Enabled = false;
            ClosePort.Enabled = true;
            Send.Enabled = true;
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
            
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 115200;
                serialPort1.Open();

                //serialPort1.WriteLine("a");

                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();

                serialPort1.WriteLine("s100\n");
            }
            catch(Exception ex)
            {
            }

            if (serialPort1.IsOpen)
            {
                serialPort1.DataReceived += port_DataReceived;
            }
        }

        private void ClosePort_Click(object sender, EventArgs e)
        {
            Connect.Enabled = true;
            ClosePort.Enabled = false;
            try
            {
                serialPort1.Close();
            }
            catch (Exception ex)
            {
            }

            if (serialPort1.IsOpen)
            {
                serialPort1.DataReceived -= port_DataReceived;
            }

        }


        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // read all bytes in input buffer
            string data = serialPort1.ReadExisting();

            
                // assign to textbox by marshalling to UI thread
                this.Invoke(new MethodInvoker(delegate ()
            {
                int value;
                                
                if (data.Contains("g"))
                {
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = false;
                    if(modeTracker == 2)
                    {
                        button8.Enabled = true;
                    }
                    textBox3.Text ="Command Recieved Successfully";
                } else if (data.Contains("ok"))
                {
                    textBox3.Text = "Move Finished";
                    if (modeTracker == 1)
                    {
                        groupBox3.Enabled = true;
                    } else
                    {
                        groupBox4.Enabled = true;
                    }
                } else if (data.Contains("z")){
                    string[] tokens = data.Split('/');
                    
                    
                    try
                    {
                        String substring = tokens[0];
                        tokens = substring.Split('z');
                        value = Convert.ToInt32(tokens[1]);
                        value = value / -11;
                        imageRotate(value);
                    } catch { }
                    
                    
                    


                }
            }));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serialPort1.Close();
            }
            catch (Exception ex)
            {
            }

            if (serialPort1.IsOpen)
            {
                serialPort1.DataReceived -= port_DataReceived;
            }
        }

        private void moveDevice(int variable)
        {
            variable = variable * 10;
            string toSend = "a" + Convert.ToString(variable) + "\n";
            serialPort1.WriteLine(toSend);
            Console.WriteLine(toSend);
        }

        //initialise
        private void Send_Click(object sender, EventArgs e)
        {
            
            if (radioButton1.Checked)
            {
                groupBox3.Enabled = true;
                groupBox4.Enabled = false;
                modeTracker = 1;
            }
            else if (radioButton2.Checked)
            {
                groupBox3.Enabled = false;
                groupBox4.Enabled = true;
                modeTracker = 2;
            }
            else if(radioButton3.Checked)
            {
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
                serialPort1.WriteLine(stringIndex);

                csvRead();
            }

            if (radioButton1.Checked || radioButton2.Checked || radioButton3.Checked)
            {
                try
                {
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.WriteLine(stringIndex);

                    }
                }
                catch (Exception ex)
                {
                }
            } else
            {
                MessageBox.Show("please select a mode");
            }
                

        }


        //Position sending
        //0 degrees
        private void button1_Click(object sender, EventArgs e)
        {
            moveDevice(0);

        }
        
        //90 degrees
        private void button2_Click(object sender, EventArgs e)
        {
            moveDevice(90);
        }

        //180 degrees
        private void button3_Click(object sender, EventArgs e)
        {
            moveDevice(180);

        }

        //270 degrees
        private void button4_Click(object sender, EventArgs e)
        {
            moveDevice(270);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt16(numericUpDown1.Value);
            moveDevice(val);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt16(numericUpDown4.Value);

            val = degressPerSec / val;

            string toSend = "s" + Convert.ToString(val) + "\n";
            serialPort1.WriteLine(toSend);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            button9.Enabled = false;
            int val = Convert.ToInt16(numericUpDown3.Value);

            val = degressPerSec / val;

            string toSend = "f" + Convert.ToString(val) + "\n";
            serialPort1.WriteLine(toSend);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            button7.Enabled = false;
            int val = Convert.ToInt16(numericUpDown3.Value);

            val = degressPerSec / val;

            string toSend = "r" + Convert.ToString(val) + "\n";
            serialPort1.WriteLine(toSend);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            button9.Enabled = true;
            button7.Enabled = true;
            string toSend = "x\n";
            serialPort1.WriteLine(toSend);
        }

        private void imageRotate(int angle)
        {
            int angleToMove = angle; //- lastAngle;

            Image orginal = TableTurner.Properties.Resources.Compass_Transparent;
            Bitmap image = (Bitmap)orginal;
            pictureBox1.Image = null;

            RotateBilinear ro = new RotateBilinear(angleToMove, true);


            ro.FillColor = Color.White;

            pictureBox1.BackColor = Color.White;

            Bitmap image2 = ro.Apply(image);
            pictureBox1.Image = image2;
            lastAngle = angle;
        }
        
        private void waitForPos()
        {
            bool atPos = false;
            while (!atPos)
            {
                string data = serialPort1.ReadExisting();
                if (data.Contains("ok"))
                {
                    Console.WriteLine(data);
                    atPos = true;
                }
            }
            
        }

        private void csvRead()
        {
            textBox3.Text = "Starting Automated Protocol";

            List<string> Position = new List<string>();
            List<string> waitTime = new List<string>();

            textBox3.AppendText("wait for indexing");


            waitForPos();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "Documents";
            openFileDialog1.Filter = "Gyro Template (*.csv)|*.csv";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    path = openFileDialog1.FileName;
                    textBox3.AppendText("File Found Successfully\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            
            using (var reader = new StreamReader(path))
            {
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    Position.Add(values[0]);
                    waitTime.Add(values[1]);
                }
            }

            Position.RemoveAt(0);
            waitTime.RemoveAt(0);

            textBox3.AppendText("File Read Successfully\n");
            int i = 0;
            while (i < waitTime.Count)
            {
                

                int toPosition = Convert.ToInt32(Position[i]);
                textBox3.AppendText("Moving to " + toPosition +"\n");
                moveDevice(toPosition);

                waitForPos();

                int interval = Convert.ToInt32(waitTime[i]);
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(interval));

                i =i+1;
            }



        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.WriteLine(textBox1.Text + "\n");
                }
                textBox1.Text = null;
            }
        }
    }
}
