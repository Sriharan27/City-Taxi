using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface IDriverStatusRepository
    {
        bool AddStatus(DriverStatus driverStatus);
        bool UpdateStatus(DriverStatus driverStatus);
        bool DeleteStatus(DriverStatus driverStatus);
        bool Save();
        Task<DriverStatus> GetDriverStatusAsync(int driverID);
        Task<DriverStatus> GetByDriverStatusIdAsync(int driverStatusId);
        Task<IEnumerable<DriverStatus>> GetNearbyDriversAsync(string latitude, string longitude, double radius);

    }
}
