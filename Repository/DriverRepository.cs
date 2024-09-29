using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.ViewModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class DriverRepository : IDriverRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public DriverRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public bool AddDriver(Driver driver)
        {
            _context.Add(driver);
            return Save();
        }
        public bool Update(Driver driver)
        {
            _context.Update(driver);
            return Save();
        }
        public bool Delete(Driver driver)
        {
            _context.Remove(driver);
            return Save();
        }

        public async Task<IEnumerable<Driver>> GetAll()
        {
            return await _context.Drivers.Include(d => d.BankBranches).Include(d => d.BankBranches.Bank).ToListAsync();
        }
        public IEnumerable<Banks> GetAllBanks()
        {
            return _context.Banks.ToList();
        }

        public async Task<Driver> GetByIdAsync(int id)
        {
            return await _context.Drivers.FirstOrDefaultAsync(i => i.DriverID == id);
        }
        public async Task<int> GetDriverCountAsync()
        {
            return await _context.Drivers.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public IEnumerable<BankBranches> GetBranchesByBankId(int bankId)
        {
            return _context.BankBranches.Where(b => b.BankCode == bankId.ToString()).ToList();
        }
    }
}
