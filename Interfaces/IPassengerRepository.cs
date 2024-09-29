using City_Taxi.Models;
using City_Taxi.ViewModel;

namespace City_Taxi.Interfaces
{
    public interface IPassengerRepository
    {
        Task<IEnumerable<Passenger>> GetAll();
        Task<Passenger> GetByIdAsync(int id);
        Task<int> GetPassengerCountAsync();
        bool AddPassenger(Passenger passenger);
        bool Update(Passenger passenger);
        bool Delete(Passenger passenger);
        bool Save();
        Task UpdatePassengerLocationAsync(int passengerId, string latitude, string longitude);
        Task<IEnumerable<DriverInfo>> GetNearbyDriversByTypeAsync(string latitude, string longitude, string vehicleType, double radiusKm);

    }
}
