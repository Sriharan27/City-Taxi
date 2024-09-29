using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(Vehicle vehicle)
        {
            _context.Add(vehicle);
            return Save();
        }
        public bool Update(Vehicle vehicle)
        {
            _context.Update(vehicle);
            return Save();
        }
        public bool Delete(Vehicle vehicle)
        {
            _context.Remove(vehicle);
            return Save();
        }
        public bool BulkDelete(IEnumerable<Vehicle> vehicle)
        {
            _context.RemoveRange(vehicle);
            return Save();
        }
        public async Task<IEnumerable<Vehicle>> GetAll()
        {
            return await _context.Vehicles.Include(v => v.Driver).Include(v => v.VehicleType).ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehicleByDrivedId(int id)
        {
            return await _context.Vehicles.Where(d => d.DriverID == id).ToListAsync();
        }

        public async Task<Vehicle> GetByVehicleIdAsync(int id)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(i => i.VehicleID == id);
        }
        public async Task<List<Vehicle>> GetAllByDriverIdAsync(int id)
        {
            return await _context.Vehicles.Where(v => v.DriverID == id).Include(v => v.Driver).Include(v => v.VehicleType).ToListAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
