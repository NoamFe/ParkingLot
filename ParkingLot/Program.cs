using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NFSystems.Extensions;

namespace ParkingLot
{
    class Program
    {
        private static Vehicle CreateRandomVehicle() => new Vehicle(EnumExtensions.Random<CarSize>(), Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 6));
        
        static void Main(string[] args)
        {
            var vacantParkingSpaces = BuilSpots();

            var parkingLot = new ParkingLot(vacantParkingSpaces.ToList());

            var totalVehicleCounter = 0;
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();                            

            while (true)
            {
                var vehicle = CreateRandomVehicle();

                Thread removeVehicleThread = new Thread(() => RemoveVehicle(stopWatch, parkingLot));
                removeVehicleThread.Start();
                 
                WriteMessage($"{vehicle.License} - {vehicle.CarSize} wanting to get in and park", ConsoleColor.Green);

                if (parkingLot.Insert(vehicle) == false)
                {
                    WriteMessage($"{ vehicle.License} - { vehicle.CarSize} Cant find a spot! going somewhere else!", ConsoleColor.Gray);
                }
                else
                {
                    lock (_vehiclesLock)
                    {
                        _vehiclesIn.Add(vehicle);
                    }
                    totalVehicleCounter++;
                }

                WriteMessage($"Total vehicles so far: { totalVehicleCounter}", ConsoleColor.White);
                WriteMessage(parkingLot.TotalVehiclesString, ConsoleColor.White);
            }
            
            Console.ReadLine();
        }

        private static void RemoveVehicle(Stopwatch stopWatch, ParkingLot parkingLot)
        {
            lock (_vehiclesLock)
            {
                if (_vehiclesIn.Any())
                {
                    TimeSpan ts = stopWatch.Elapsed;
                    if (ts.Milliseconds > 200)
                    {
                        var index = GetRandomNumber(0, _vehiclesIn.Count - 1);

                        var vehicleToRemove = _vehiclesIn[index];

                        WriteMessage($"Vehicle {vehicleToRemove.License} - {vehicleToRemove.CarSize} is leaving now....", ConsoleColor.Red);

                        parkingLot.Remove(vehicleToRemove);

                        stopWatch.Stop();

                        _vehiclesIn.Remove(vehicleToRemove);
                    }
                    stopWatch.Start();

                }
            }
        }


        private static IEnumerable<ParkingSpot> BuilSpots()
        {
            var vacantParkingSpaces = new List<ParkingSpot>();
            for (int i = 0; i < 20; i++)
            {
                vacantParkingSpaces.Add(new ParkingSpot() { Id = i.ToString(), Floor = 0, ParkingSize = ParkingSize.Large });
            }

            for (int i = 0; i < 50; i++)
            {
                vacantParkingSpaces.Add(new ParkingSpot() { Id = (i + 5).ToString(), Floor = 0, ParkingSize = ParkingSize.Medium });
            }

            for (int i = 0; i < 50; i++)
            {
                vacantParkingSpaces.Add(new ParkingSpot() { Id = (i + 10).ToString(), Floor = 0, ParkingSize = ParkingSize.Small });
            }

            for (int i = 0; i < 5; i++)
            {
                vacantParkingSpaces.Add(new ParkingSpot()
                {
                    Id = (i + 15).ToString(),
                    Floor = 0,
                    ParkingSize = ParkingSize.Motorcycle
                });
            }

            return vacantParkingSpaces;
        }


        private static readonly Random random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (random)
            {
                return random.Next(min, max);
            }
        }

        private static object _messageLock = new object();


        public static void WriteMessage(string message, ConsoleColor color)
        {
            lock (_messageLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{DateTime.Now} - {message}");
                Console.ResetColor();
            }
        }

        private static readonly object _vehiclesLock = new object();

        private static List<Vehicle> _vehiclesIn = new List<Vehicle>();       
    }
}
