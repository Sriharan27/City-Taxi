using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class VehicleTypeRepository : IVehicleTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(VehicleTypes vehicleTypes)
        {
            _context.Add(vehicleTypes);
            return Save();
        }
        public bool Update(VehicleTypes vehicleTypes)
        {
            _context.Update(vehicleTypes);
            return Save();
        }
        public bool Delete(VehicleTypes vehicleTypes)
        {
            _context.Remove(vehicleTypes);
            return Save();
        }

        public async Task<IEnumerable<VehicleTypes>> GetAll()
        {
            return await _context.VehicleTypes.ToListAsync();
        }

        public async Task<VehicleTypes> GetByVehicleTypeIdAsync(int id)
        {
            return await _context.VehicleTypes.FirstOrDefaultAsync(i => i.VehicleTypeID == id);
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
