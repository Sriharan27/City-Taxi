using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class SystemUserRepository : ISystemUserRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(SystemUser user)
        {
            _context.Add(user);
            return Save();
        }
        public bool Update(SystemUser user)
        {
            _context.Update(user);
            return Save();
        }
        public bool Delete(SystemUser user)
        {
            _context.Remove(user);
            return Save();
        }

        public async Task<IEnumerable<SystemUser>> GetAll()
        {
            return await _context.SystemUsers.Include(u => u.Login).ToListAsync();
        }

        public async Task<SystemUser> GetByIdAsync(int id)
        {
            return await _context.SystemUsers.Include(u => u.Login).FirstOrDefaultAsync(i => i.SystemUserID == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}

