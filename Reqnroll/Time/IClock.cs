using System;

namespace Reqnroll.Time
{
    public interface IClock
    {
        DateTime GetToday();

        DateTime GetNowDateAndTime();
    }
}
