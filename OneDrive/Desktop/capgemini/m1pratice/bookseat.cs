using System;
using System.Collections.Generic;

public class Seat
{
    public int SeatNo { get; set; }
    public bool IsBooked { get; set; }
    public string BookedBy { get; set; }
}

public class SeatBookingService
{
    private readonly object _lock = new object();

    private readonly Dictionary<int, Seat> _seats =
        new Dictionary<int, Seat>();

    public SeatBookingService()
    {
        // Initialize 100 seats
        for (int i = 1; i <= 100; i++)
        {
            _seats[i] = new Seat
            {
                SeatNo = i,
                IsBooked = false
            };
        }
    }

    public bool BookSeat(int seatNo, string userId)
    {
        lock (_lock) // Thread synchronization
        {
            if (!_seats.TryGetValue(seatNo, out Seat seat))
                return false;

            if (seat.IsBooked)
                return false;

            seat.IsBooked = true;
            seat.BookedBy = userId;

            return true;
        }
    }
}

class Program
{
    static void Main()
    {
        var service = new SeatBookingService();

        bool user1 = service.BookSeat(10, "UserA");
        bool user2 = service.BookSeat(10, "UserB");

        Console.WriteLine(user1); // True
        Console.WriteLine(user2); // False
    }
}
