
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Emmellsoft.IoT.Rpi.SenseHat;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts.SingleColor;
using Windows.UI;

namespace SerialSample
{    
    public sealed partial class MainPage : Page
    {
       
        private SerialDevice serialPort = null;
        //DataWriter dataWriteObject = null;
       // DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices;
   
      

        public MainPage()
        {
            this.InitializeComponent();            
            comPortInput.IsEnabled = false;
           // sendTextButton.IsEnabled = false;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            Loaded += MainPage_Loaded;
           
        }

   
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //get a reference to SenseHat
            senseHat = await SenseHatFactory.GetSenseHat();
            //initialize the timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

        }
        private enum TemperatureUnit
        {
            Celcius,
            Fahrenheit,
            Kelvin
        }

        public void Timer_Tick(object sender, object e)
        {
            var tinyFont = new TinyFont();
            senseHat.Sensors.HumiditySensor.Update();
            senseHat.Sensors.PressureSensor.Update();

            ISenseHatDisplay display = senseHat.Display;
            //gather data
            
            SenseHatData data = new SenseHatData();
            data.Temperature = senseHat.Sensors.Temperature-5;
            data.Humidity = senseHat.Sensors.Humidity-8;
            data.Pressure = senseHat.Sensors.Pressure;
           
            var tempvalue = data.Temperature;
           int temperature = (int)data.Temperature;
         
            gaugeTemp.Value = (float)data.Temperature;
            gaugePres.Value = (float)data.Pressure;
            gaugehumi.Value = (float)data.Humidity;
          
            display.Clear();
            if (temperature<=32)
            {
              
                var s = Convert.ToString(temperature);
                tinyFont.Write(display, s, Colors.Green);
               
            }
         
            else if ((temperature >=33) || (temperature <=34) )
            {
                
                var s = Convert.ToString(temperature);
                tinyFont.Write(display, s, Colors.Blue);
               
            }
            if(temperature >= 34)
            {
               
                var s = Convert.ToString(temperature);
                tinyFont.Write(display, s, Colors.Red);
               
            }
           
            display.Update();


        }


        ISenseHat senseHat;

   
    
        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                status.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;
                comPortInput.IsEnabled = true;
                ConnectDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

    
        private async void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            var selection = ConnectDevices.SelectedItems;

            if (selection.Count <= 0)
            {
                status.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];         

            try
            {                
                serialPort = await SerialDevice.FromIdAsync(entry.Id);

                // Disable the 'Connect' button 
                comPortInput.IsEnabled = false;

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);                
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
              
                // Display configured settings
                status.Text = "Serial port configured successfully: ";
                status.Text += serialPort.BaudRate + "-";
                status.Text += serialPort.DataBits + "-";
                status.Text += serialPort.Parity.ToString() + "-";
                status.Text += serialPort.StopBits;

                // Set the RcvdText field to invoke the TextChanged callback
                // The callback launches an async Read task to wait for data
                rcvdText.Text = "Waiting for data...";

               
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                comPortInput.IsEnabled = true;
                //sendTextButton.IsEnabled = false;
            }
        }

       
    
        private void CloseDevice()
        {            
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            comPortInput.IsEnabled = true;
            //sendTextButton.IsEnabled = false;            
            rcvdText.Text = "";
            listOfDevices.Clear();               
        }

    
        private void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                status.Text = "";
                
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }          
        }

  
        private void light_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleSwitch light = (ToggleSwitch)sender;
            light.Toggled += Light_Toggled;

        }

        private async void Light_Toggled(object sender, RoutedEventArgs e)
        {
            if (light.IsOn)
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("q");
                    await dataWriter.StoreAsync();
                    //await dataWriter.FlushAsync();
                    dataWriter.DetachStream();

                   
                }
            }
            else
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("w");
                    await dataWriter.StoreAsync();
                    //await dataWriter.FlushAsync();
                    dataWriter.DetachStream();

                   
                }

            }
        }

        private void fan_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleSwitch fan = (ToggleSwitch)sender;
            fan.Toggled += Fan_Toggled;
        }

        private async void Fan_Toggled(object sender, RoutedEventArgs e)
        {
            if (fan.IsOn)
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("a");
                    await dataWriter.StoreAsync();
                    //await dataWriter.FlushAsync();
                    dataWriter.DetachStream();

                }
            }
            else
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("s");
                    await dataWriter.StoreAsync();
                    //await dataWriter.FlushAsync();
                    dataWriter.DetachStream();

                   
                }
            }
        }

        private void secondLight_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleSwitch secondLight = (ToggleSwitch)sender;
            secondLight.Toggled += SecondLight_Toggled;
        }

        private async void SecondLight_Toggled(object sender, RoutedEventArgs e)
        {
            if (secondLight.IsOn)
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("z");
                    await dataWriter.StoreAsync();
                   // await dataWriter.FlushAsync();
                    dataWriter.DetachStream();


                }
            }
            else
            {
                using (var dataWriter = new DataWriter(serialPort.OutputStream))
                {
                    dataWriter.WriteString("x");
                    await dataWriter.StoreAsync();
                    //await dataWriter.FlushAsync();
                    dataWriter.DetachStream();


                }
            }
        }
    }
}
