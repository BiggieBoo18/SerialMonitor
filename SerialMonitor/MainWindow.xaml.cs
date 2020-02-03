using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort = null;
        private ObservableCollection<RecievedData> dataList = new ObservableCollection<RecievedData>();

        public MainWindow()
        {
            InitializeComponent();
            EnumerateSerialPorts();
            EnumerateBaudrates();
        }
        public class COMPortInfo
        {
            public string DeviceID { get; set; }
            public string Description { get; set; }
        }

        public class BaudrateInfo
        {
            public int Baudrate { get; set; }
        }

        public class RecievedData
        {
            public string Data { get; set; }
        }

        /*
         * Enumerate serial ports
         */
        private void EnumerateSerialPorts()
        {
            // Get serial-port name
            string[] PortList = SerialPort.GetPortNames();
            var ItemList = new ObservableCollection<COMPortInfo>();
            foreach (string p in PortList)
            {
                ItemList.Add(new COMPortInfo { DeviceID = p, Description = p });
            }
            cmbCOM.ItemsSource = ItemList;
            cmbCOM.SelectedValuePath = "DeviceID";
            cmbCOM.DisplayMemberPath = "Description";
        }

        /*
         * Enumerate baudrate
         */
        private void EnumerateBaudrates()
        {
            int[] baudrates = new int[] { 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 250000, 500000, 1000000, 2000000 };
            var BaudrateList = new ObservableCollection<BaudrateInfo>();
            foreach (int baudrate in baudrates)
            {
                BaudrateList.Add(new BaudrateInfo { Baudrate = baudrate });
            }
            cmbBaud.ItemsSource = BaudrateList;
            cmbBaud.SelectedValuePath = "Baudrate";
            cmbBaud.DisplayMemberPath = "Baudrate";
        }

        /*
         * Connect serial port
         */
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // When selected serial-port
            if (cmbCOM.SelectedValue != null && cmbBaud.SelectedValue != null)
            {
                // Get serial-port name and baudrate
                var port     = cmbCOM.SelectedValue.ToString();
                var baudrate = Convert.ToInt32(cmbBaud.SelectedValue.ToString());
                // When disconnected
                if (serialPort == null)
                {
                    // Setting serial-port
                    serialPort = new SerialPort
                    {
                        PortName = port,
                        BaudRate = baudrate,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        Encoding = Encoding.UTF8,
                        WriteTimeout = 100000
                    };

                    // When received data
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);

                    // Try to connect
                    try
                    {
                        // Open serial-port
                        serialPort.Open();
                        txtbStatus.Text = "接続中";
                        txtbStatus.Background = Brushes.LimeGreen;
                        btnConnect.IsEnabled = false;
                        btnDisconnect.IsEnabled = true;
                        Console.WriteLine("COM port:" + serialPort.PortName + " Connected.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        /*
         * Disconnect serial port
         */
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            // Check connected or disconnected
            if (serialPort != null && serialPort.IsOpen == true)
            {
                serialPort.Close();
                txtbStatus.Text = "切断済み";
                txtbStatus.Background = Brushes.LightGray;
                btnDisconnect.IsEnabled = false;
                btnConnect.IsEnabled = true;
                serialPort = null;
            }
        }

        /*
         * Send text to serial port
         */
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            // Check connected or disconnected
            if (serialPort == null) return;
            if (serialPort.IsOpen == false) return;

            // Get text from textbox
            String data = txtbSend.Text + "\r\n";

            try
            {
                // Send text
                serialPort.Write(data);
                txtbSend.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SerialPortDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Check connected or disconnected
            if (serialPort == null) return;
            if (serialPort.IsOpen == false) return;

            // Show in the textbox
            try
            {
                dgData.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        dataList.Add(new RecievedData { Data = serialPort.ReadLine() });
                        dgData.ItemsSource = dataList;
                    })
                );
            }
            catch
            {
                dgData.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        dataList.Add(new RecievedData { Data = "!Error! cannot connect" + serialPort.PortName });
                        dgData.ItemsSource = dataList;
                    })
                );
            }
        }
    }
}
