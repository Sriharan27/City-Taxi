using City_Taxi.Models;
using Mailjet.Client.Resources;

namespace City_Taxi.Interfaces
{
    public interface ISystemUserRepository
    {
        Task<IEnumerable<SystemUser>> GetAll();
        Task<SystemUser> GetByIdAsync(int id);
        bool Add(SystemUser user);
        bool Update(SystemUser user);
        bool Delete(SystemUser user);
        bool Save();
    }
}
