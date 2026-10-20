using System;
using KK.App.Timing;

namespace KK.Presentation.Tests.Fakes
{
    public class FakeClock : IClock
    {
        public TimeSpan Now { get; set; }

        public void Advance(double seconds)
        {
            Now += TimeSpan.FromSeconds(seconds);
        }
    }
}
