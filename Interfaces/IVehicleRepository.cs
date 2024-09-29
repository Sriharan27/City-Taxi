using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAll();
        Task<IEnumerable<Vehicle>> GetAllVehicleByDrivedId(int id);
        Task<Vehicle> GetByVehicleIdAsync(int id);
        Task<List<Vehicle>> GetAllByDriverIdAsync(int id);
        bool Add(Vehicle driver);
        bool Update(Vehicle driver);
        bool Delete(Vehicle driver);
        bool BulkDelete(IEnumerable<Vehicle> vehicle);
        bool Save();
    }
}
