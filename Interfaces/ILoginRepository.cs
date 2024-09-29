using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface ILoginRepository
    {
        bool AddLogin(Login login);
        bool UpdateLogin(Login login);
        bool DeleteLogin(Login login);
        Task<Login> GetByUsernameAsync(string username);
        Task<Login> GetByUserIdAsync(int userid);
        bool Save();
    }
}
