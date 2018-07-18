using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingLot
{
    public class ParkingLot
    {
        Queue<ParkingSpot> _vacantMotorcycleParkingSpaces;
        Queue<ParkingSpot> _vacantLargeParkingSpaces;
        Queue<ParkingSpot> _vacantMediumParkingSpaces;
        Queue<ParkingSpot> _vacantSmallParkingSpaces;

        Dictionary<ParkingSize, Queue<ParkingSpot>> _spotsQueues;
        Dictionary<Vehicle, ParkingSpot> _occupiedParkingSpaces;

        private int TotalAvailableSpots => _vacantMotorcycleParkingSpaces.Count +
            _vacantLargeParkingSpaces.Count +
            _vacantMediumParkingSpaces.Count +
            _vacantSmallParkingSpaces.Count;

        public string TotalVehiclesString => $"{TotalAvailableSpots} / {MaxCapacity} spot are currently available in the lot.";
        public bool IsFull => TotalAvailableSpots == 0;

        public bool IsEmpty => !_occupiedParkingSpaces.Any();

        public ParkingLot()
        {
            Init();
        }

        public void Init()
        {
            _vacantSmallParkingSpaces = new Queue<ParkingSpot>();
            _vacantMediumParkingSpaces = new Queue<ParkingSpot>();
            _vacantMotorcycleParkingSpaces = new Queue<ParkingSpot>();
            _vacantLargeParkingSpaces = new Queue<ParkingSpot>();
            _spotsQueues = new Dictionary<ParkingSize, Queue<ParkingSpot>>();

            _spotsQueues.Add(ParkingSize.Large, _vacantLargeParkingSpaces);
            _spotsQueues.Add(ParkingSize.Medium, _vacantMediumParkingSpaces);
            _spotsQueues.Add(ParkingSize.Small, _vacantSmallParkingSpaces);
            _spotsQueues.Add(ParkingSize.Motorcycle, _vacantMotorcycleParkingSpaces);

            _occupiedParkingSpaces = new Dictionary<Vehicle, ParkingSpot>();
        }

        public readonly int MaxCapacity;

        public ParkingLot(IList<ParkingSpot> vacantParkingSpaces)
        {
            Init();
            foreach (var item in vacantParkingSpaces)
            {
                if (item.ParkingSize == ParkingSize.Large)
                    _vacantLargeParkingSpaces.Enqueue(item);
                if (item.ParkingSize == ParkingSize.Medium)
                    _vacantMediumParkingSpaces.Enqueue(item);
                if (item.ParkingSize == ParkingSize.Small)
                    _vacantSmallParkingSpaces.Enqueue(item);
                if (item.ParkingSize == ParkingSize.Motorcycle)
                    _vacantMotorcycleParkingSpaces.Enqueue(item);
            }

            MaxCapacity = vacantParkingSpaces.Count;
        }

        
        private ParkingSpot GetSpot(CarSize carSize)
        {
            lock (_queueLock)
            {
                if (carSize == CarSize.Bus)
                {
                    if (_vacantLargeParkingSpaces.Count > 0)
                        return _vacantLargeParkingSpaces.Dequeue();
                }

                if (carSize == CarSize.Car)
                {
                    if (_vacantMediumParkingSpaces.Count > 0)
                        return _vacantMediumParkingSpaces.Dequeue();

                    if (_vacantLargeParkingSpaces.Count > 0)
                        return _vacantLargeParkingSpaces.Dequeue();

                    return null;
                }
                if (carSize == CarSize.SmallCar)
                {
                    if (_vacantSmallParkingSpaces.Count > 0)
                        return _vacantSmallParkingSpaces.Dequeue();

                    if (_vacantMediumParkingSpaces.Count > 0)
                        return _vacantMediumParkingSpaces.Dequeue();

                    if (_vacantLargeParkingSpaces.Count > 0)
                        return _vacantLargeParkingSpaces.Dequeue();

                    return null;
                }
                if (carSize == CarSize.Motorcycle)
                {
                    if (_vacantMotorcycleParkingSpaces.Count > 0)
                        return _vacantMotorcycleParkingSpaces.Dequeue();

                    if (_vacantSmallParkingSpaces.Count > 0)
                        return _vacantSmallParkingSpaces.Dequeue();

                    if (_vacantMediumParkingSpaces.Count > 0)
                        return _vacantMediumParkingSpaces.Dequeue();

                    if (_vacantLargeParkingSpaces.Count > 0)
                        return _vacantLargeParkingSpaces.Dequeue();

                    return null;
                }
                return null;
            }
        }

        public bool Insert(Vehicle vehicle)
        {
            ParkingSpot spot = GetSpot(vehicle.CarSize);
            if (spot == null)
                return false;

            lock (_occupiedParkingSpacesLock)
            {
                _occupiedParkingSpaces.Add(vehicle, spot);
            }

            return true;
        }
        public ParkingSpot Remove(Vehicle vehicle)
        {
            lock (_occupiedParkingSpacesLock)
            {
                var spot = _occupiedParkingSpaces[vehicle];
                lock (_queueLock)
                {
                    _occupiedParkingSpaces[vehicle] = null;

                    _spotsQueues[spot.ParkingSize].Enqueue(spot);

                    return spot;
                }
            }
        }

        private static readonly object _occupiedParkingSpacesLock = new object();
        private static readonly object _queueLock = new object();
    }
}
