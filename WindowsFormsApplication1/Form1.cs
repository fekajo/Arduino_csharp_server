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
using OpenHardwareMonitor.Hardware;
using System.Net.NetworkInformation;

namespace Arduino_Controll
{
    public partial class Form1 : Form
    {
        static string data, temphum, temp , humi;
        Computer c = new Computer()
        {
            GPUEnabled = true,
            CPUEnabled = true
        };

        float value1, value2, value3;

        private SerialPort port = new SerialPort();
        public Form1()
        {
            InitializeComponent();
            Init();
        }
        public static double PingTimeAverage(string host, int echoNum)
        {
            long totalTime = 0;
            int timeout = 120;
            Ping pingSender = new Ping();

            for (int i = 0; i < echoNum; i++)
            {
                PingReply reply = pingSender.Send(host, timeout);
                if (reply.Status == IPStatus.Success)
                {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / echoNum;
        }
        private void Init()
        {
            try
            {
                notifyIcon1.Visible = false;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    comboBox1.Items.Add(port);
                }
                port.BaudRate = 9600;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            c.Open();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                try
                {
                    notifyIcon1.ShowBalloonTip(500, "Arduino", toolStripStatusLabel1.Text, ToolTipIcon.Info);

                }
                catch (Exception ex)
                {

                }
                this.Hide();
            }


        }


        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {

            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
           {
               port.Write("D");
               port.Close();
           }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label3.Text = "Disconnected";
            timer1.Enabled = false;
            toolStripStatusLabel1.Text = "Disconnected to Arduino...";
            data = "";
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.PortName = comboBox1.Text;
                    port.Open();
                    timer1.Interval = Convert.ToInt32(comboBox2.Text);
                    timer1.Enabled = true;
                    toolStripStatusLabel1.Text = "Sending data...";
                    label3.Text = "Connected";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            Status();
            data = port.ReadExisting();
            if (data.Length >= 12)
            {
                humi = data.Substring(1, 2);
                temp = data.Substring(7, 2);
            }
            temphum = data;
            label5.Text = temphum;
            label6.Text = humi;
            label7.Text = temp;
        }

        private void Status()
        {
            foreach (var hardware in c.Hardware)
            {

                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                        if (sensor.SensorType == SensorType.Temperature)
                        {

                            value1 = sensor.Value.GetValueOrDefault();
                        }

                }

                if (hardware.HardwareType == HardwareType.CPU)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            value2 = sensor.Value.GetValueOrDefault();

                        }

                }
            }
            value3 = Convert.ToInt32(PingTimeAverage((comboBox3.Text), 4));
            if (value3 > 99)
            {
                value3 = 99;
            }
            try
            {
                port.Write(value1 + "*" + value2 + "#" + value3 + "&" + humi + "H" + temp + "T");
                label9.Text = Convert.ToString(value1 + "*" + value2 + "#" + value3 + "&" + humi + "H" + temp + "T");
            }
            catch (Exception ex)
            {
                timer1.Stop();
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = "Arduino's not responding...";
            }
        }

    }
}