using City_Taxi.Models;

namespace City_Taxi.Interfaces
{
    public interface IReservationRepository
    {
        bool Add(Reservation reservation);
        bool Update(Reservation reservation);
        bool Delete(Reservation reservation);
        bool Save();
        Task<Reservation> GetWaitingReservationByDriverIDAsync(int driverId);
        Task<Reservation> GetReservationByReservationIDAsync(int reservationID);
        Task<string> GetReservationStatusByReservationIDAsync(int reservationID);
        Task<IEnumerable<Reservation>> GetReservationByDriverIDAsync(int driverId);
        Task<IEnumerable<Reservation>> GetReservationByPassengerIDAsync(int passengerID);
        Task<IEnumerable<Reservation>> GetAll();
        Task<int> GetTripCountAsync();
    }
}
