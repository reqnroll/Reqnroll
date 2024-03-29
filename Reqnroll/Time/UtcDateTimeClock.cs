using System;

namespace Reqnroll.Time
{
    public class UtcDateTimeClock : IClock
    {
        public DateTime GetToday()
        {
            return DateTime.UtcNow.Date;
        }

        public DateTime GetNowDateAndTime()
        {
            return DateTime.UtcNow;
        }
    }
}
