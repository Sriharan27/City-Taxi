using City_Taxi.Models;
using City_Taxi.ViewModel;

namespace City_Taxi.Interfaces
{
    public interface IDriverRepository
    {
        Task<IEnumerable<Driver>> GetAll();
        IEnumerable<Banks> GetAllBanks();
        Task<Driver> GetByIdAsync(int id);
        Task<int> GetDriverCountAsync();
        IEnumerable<BankBranches> GetBranchesByBankId(int bankId);
        bool AddDriver(Driver driver);
        bool Update(Driver driver);
        bool Delete(Driver driver);
        bool Save();
    }
}
