using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Mailjet.Client.Resources;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddLogin(Login login)
        {
            _context.Add(login);
            return Save();
        }
        public async Task<Login> GetByUsernameAsync(string username)
        {
            return await _context.Logins.FirstOrDefaultAsync(i => i.Username == username);
        }
        public async Task<Login> GetByUserIdAsync(int userid)
        {
            return await _context.Logins.FirstOrDefaultAsync(i => i.UserID == userid);
        }
        public bool UpdateLogin(Login login)
        {
            _context.Update(login);
            return Save();
        }
        public bool DeleteLogin(Login login)
        {
            _context.Remove(login);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
