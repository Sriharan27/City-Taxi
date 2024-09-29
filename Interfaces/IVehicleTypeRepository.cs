using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface IVehicleTypeRepository
    {
        Task<IEnumerable<VehicleTypes>> GetAll();
        Task<VehicleTypes> GetByVehicleTypeIdAsync(int id);
        bool Add(VehicleTypes vehicleTypes);
        bool Update(VehicleTypes vehicleTypes);
        bool Delete(VehicleTypes vehicleTypes);
        bool Save();
    }
}
