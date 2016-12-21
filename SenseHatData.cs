using System;
using Emmellsoft.IoT.Rpi.SenseHat;


namespace SerialSample
{
    public class SenseHatData
    {
        public double? Humidity { get; set; }
        public double? Pressure { get; set; }
        public double? Temperature { get; set; }
        public string Location { get; set; }
        public double temperature { get; set; }

    }
}