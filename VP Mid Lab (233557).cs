using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RideSharingSystem
{
    // Abstract User class
    public abstract class User
    {
        protected string UserId;
        protected string name;
        protected string phoneNumber;

        public User(string userId, string name, string phoneNumber)
        {
            this.UserId = userId;
            this.name = name;
            SetPhoneNumber(phoneNumber);
        }

        public virtual void DisplayProfile()
        {
            Console.WriteLine($"User ID: {UserId}, Name: {name}, Phone Number: {phoneNumber}");
        }

        public string Name => name;

        public void SetPhoneNumber(string phoneNumber)
        {
            if (Regex.IsMatch(phoneNumber, @"^\d{10}$"))
            {
                this.phoneNumber = phoneNumber;
            }
            else
            {
                throw new ArgumentException("Invalid phone number!");
            }
        }
    }

    // Rider class
    public class Rider : User
    {
        private List<Trip> rideHistory;

        public Rider(string userId, string name, string phoneNumber) : base(userId, name, phoneNumber)
        {
            rideHistory = new List<Trip>();
        }

        public void RequestRide(RideSharingSystem system)
        {
            system.RequestRide(this);
        }

        public void ViewRideHistory()
        {
            Console.WriteLine($"{name}'s Ride History:");
            foreach (var trip in rideHistory)
            {
                trip.DisplayTripDetails();
            }
        }

        public void AddToRideHistory(Trip trip)
        {
            rideHistory.Add(trip);
        }
    }

    // Driver class
    public class Driver : User
    {
        private string driverId;
        private string vehicleDetails;
        private bool isAvailable;
        private List<Trip> tripHistory;

        public Driver(string userId, string name, string phoneNumber, string driverId, string vehicleDetails) : base(userId, name, phoneNumber)
        {
            this.driverId = driverId;
            this.vehicleDetails = vehicleDetails;
            isAvailable = true;
            tripHistory = new List<Trip>();
        }

        public void AcceptRide(Trip trip)
        {
            if (isAvailable)
            {
                trip.StartTrip();
                tripHistory.Add(trip);
                Console.WriteLine($"{name} accepted the ride.");
                isAvailable = false;
            }
            else
            {
                Console.WriteLine($"{name} is currently unavailable.");
            }
        }

        public void ViewTripHistory()
        {
            Console.WriteLine($"{name}'s Trip History:");
            foreach (var trip in tripHistory)
            {
                trip.DisplayTripDetails();
            }
        }

        public void ToggleAvailability()
        {
            isAvailable = !isAvailable;
            Console.WriteLine($"{name} is now " + (isAvailable ? "available" : "unavailable"));
        }

        public bool IsAvailable => isAvailable;
    }

    // Trip class
    public class Trip
    {
        public string TripId { get; private set; }
        public string RiderName { get; private set; }
        public string DriverName { get; private set; }
        public string StartLocation { get; set; }
        public string Destination { get; set; }
        public decimal Fare { get; private set; }
        public string Status { get; private set; }

        private static int idCounter = 1;

        public Trip(string riderName, string driverName, string startLocation, string destination)
        {
            TripId = "T" + (idCounter++).ToString();
            RiderName = riderName;
            DriverName = driverName;
            StartLocation = startLocation;
            Destination = destination;
            Fare = CalculateFare();
            Status = "Pending";
        }

        public decimal CalculateFare()
        {

            var random = new Random();
            return random.Next(10, 50);
        }

        public void StartTrip()
        {
            Status = "In Progress";
            Console.WriteLine($"Trip {TripId} started from {StartLocation} to {Destination}. Fare: {Fare}");
        }

        public void EndTrip()
        {
            Status = "Completed";
            Console.WriteLine($"Trip {TripId} completed. Final fare: {Fare}");
        }

        public void DisplayTripDetails()
        {
            Console.WriteLine($"Trip ID: {TripId}, Rider: {RiderName}, Driver: {DriverName}, Start: {StartLocation}, Destination: {Destination}, Fare: {Fare}, Status: {Status}");
        }
    }

    // RideSharingSystem class
    public class RideSharingSystem
    {
        private List<Rider> registeredRiders;
        private List<Driver> registeredDrivers;
        private List<Trip> availableTrips;

        public RideSharingSystem()
        {
            registeredRiders = new List<Rider>();
            registeredDrivers = new List<Driver>();
            availableTrips = new List<Trip>();
        }

        public void RegisterUser()
        {
            Console.WriteLine("Registering user as Rider or Driver? (r/d)");
            string userType = Console.ReadLine()?.ToLower();
            Console.WriteLine("Enter Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Enter Phone Number (10 digits):");
            string phoneNumber = Console.ReadLine();
            string userId = Guid.NewGuid().ToString();

            if (userType == "r")
            {
                Rider rider = new Rider(userId, name, phoneNumber);
                registeredRiders.Add(rider);
                Console.WriteLine("Rider registered successfully.");
            }
            else if (userType == "d")
            {
                Console.WriteLine("Enter Driver ID:");
                string driverId = Console.ReadLine();
                Console.WriteLine("Enter Vehicle Details:");
                string vehicleDetails = Console.ReadLine();
                Driver driver = new Driver(userId, name, phoneNumber, driverId, vehicleDetails);
                registeredDrivers.Add(driver);
                Console.WriteLine("Driver registered successfully.");
            }
            else
            {
                Console.WriteLine("Invalid user type.");
            }
        }

        public void RequestRide(Rider rider)
        {
            Console.WriteLine("Enter Start Location:");
            string startLocation = Console.ReadLine();
            Console.WriteLine("Enter Destination:");
            string destination = Console.ReadLine();

            Driver availableDriver = FindAvailableDriver();
            if (availableDriver != null)
            {
                Trip trip = new Trip(rider.Name, availableDriver.Name, startLocation, destination);
                availableTrips.Add(trip);
                availableDriver.AcceptRide(trip);
                rider.AddToRideHistory(trip);
            }
            else
            {
                Console.WriteLine("No available drivers at the moment.");
            }
        }

        public Driver FindAvailableDriver()
        {
            foreach (var driver in registeredDrivers)
            {
                if (driver.IsAvailable)
                {
                    return driver;
                }
            }
            return null;
        }

        public void CompleteTrip(Driver driver)
        {
            Console.WriteLine("Enter Trip ID to complete:");
            string tripId = Console.ReadLine();
            Trip trip = availableTrips.Find(t => t.TripId == tripId);

            if (trip != null)
            {
                trip.EndTrip();
                driver.ToggleAvailability();
                availableTrips.Remove(trip);
            }
            else
            {
                Console.WriteLine("Invalid Trip ID.");
            }
        }

        public void DisplayAllTrips()
        {
            Console.WriteLine("Available Trips:");
            foreach (var trip in availableTrips)
            {
                trip.DisplayTripDetails();
            }
        }

        public List<Rider> RegisteredRiders => registeredRiders;

        public List<Driver> RegisteredDrivers => registeredDrivers;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            RideSharingSystem rideSharingSystem = new RideSharingSystem();
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\nWelcome to the Ride-Sharing System:");
                Console.WriteLine("1..Register as Rider**");
                Console.WriteLine("2.Register as Driver**");
                Console.WriteLine("3.Request a Ride (Rider)**");
                Console.WriteLine("4.Accept a Ride (Driver)**");
                Console.WriteLine("5. Complete a Trip (Driver)**");
                Console.WriteLine("6. View Ride History (Rider)**");
                Console.WriteLine("7. View Trip History (Driver)**");
                Console.WriteLine("8. Display All Trips**");
                Console.WriteLine("9. Exit**");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        rideSharingSystem.RegisterUser();
                        break;
                    case "2":
                        rideSharingSystem.RegisterUser();
                        break;
                    case "3":
                        Console.WriteLine("Enter Rider Name:");
                        string riderName = Console.ReadLine();
                        Rider rider = rideSharingSystem.RegisteredRiders.Find(r => r.Name.Equals(riderName, StringComparison.OrdinalIgnoreCase));
                        if (rider != null)
                        {
                            rider.RequestRide(rideSharingSystem);
                        }
                        else
                        {
                            Console.WriteLine("Rider not found.");
                        }
                        break;
                    case "4":
                        Console.WriteLine("Enter Driver Name:");
                        string driverName = Console.ReadLine();
                        Driver driver = rideSharingSystem.RegisteredDrivers.Find(d => d.Name.Equals(driverName, StringComparison.OrdinalIgnoreCase));

                        break;
                    case "5":
                        Console.WriteLine("Enter Driver Name:");
                        driverName = Console.ReadLine();
                        driver = rideSharingSystem.RegisteredDrivers.Find(d => d.Name.Equals(driverName, StringComparison.OrdinalIgnoreCase));
                        if (driver != null)
                        {
                            driver.ViewTripHistory();
                        }
                        else
                        {
                            Console.WriteLine("Driver not found.");
                        }
                        break;
                    case "6":
                        Console.WriteLine("Enter Rider Name:");
                        riderName = Console.ReadLine();
                        rider = rideSharingSystem.RegisteredRiders.Find(r => r.Name.Equals(riderName, StringComparison.OrdinalIgnoreCase));
                        if (rider != null)
                        {
                            rider.ViewRideHistory();
                        }
                        else
                        {
                            Console.WriteLine("Rider not found.");
                        }
                        break;
                    case "7":
                        Console.WriteLine("Enter Driver Name:");
                        driverName = Console.ReadLine();
                        driver = rideSharingSystem.RegisteredDrivers.Find(d => d.Name.Equals(driverName, StringComparison.OrdinalIgnoreCase));
                        if (driver != null)
                        {
                            driver.ViewTripHistory();
                        }
                        else
                        {
                            Console.WriteLine("Driver not found.");
                        }
                        break;
                    case "8":
                        rideSharingSystem.DisplayAllTrips();
                        break;
                    case "9":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
    }
}

