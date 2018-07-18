using System;

namespace ParkingLot
{
    public class Vehicle
    {
        public string License;
        public CarSize CarSize { get; set; }         
        public Vehicle(CarSize carSize)
        {
            CarSize = carSize;
        }

        public Vehicle(CarSize carSize, string license)
        {
            CarSize = carSize;
            License = license;
        }
    }
}
