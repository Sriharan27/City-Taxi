using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface IDriverRatingsRepository
    {
        bool Add(DriverRatings driverRatings);
        bool Update(DriverRatings driverRatings);
        bool Delete(DriverRatings driverRatings);
        bool Save();
        Task<int> GetRatingsCountAsync();
        Task<int> GetTripStarsTotalAsync();
        Task<DriverRatings> GetRatingByReservationID(int reservationID);
    }
}
