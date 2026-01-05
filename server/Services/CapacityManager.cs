using System;
namespace Conference.Functions.Services
{
    public static class CapacityManager
    {
        // Decide status when attempting to register one new person for an event
        // Returns: "confirmed", "waitlisted", or "rejected"
        public static string DecideStatus(int confirmedCount, int waitlistedCount, int capacity, int waitlistCapacity, bool enforceCapacity)
        {
            if (!enforceCapacity)
            {
                return "confirmed";
            }

            // capacity == 0 means unlimited
            if (capacity <= 0)
            {
                return "confirmed";
            }

            if (confirmedCount < capacity)
            {
                return "confirmed";
            }

            if (waitlistCapacity > 0 && waitlistedCount < waitlistCapacity)
            {
                return "waitlisted";
            }

            return "rejected";
        }
    }
}