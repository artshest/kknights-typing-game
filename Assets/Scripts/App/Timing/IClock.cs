using System;

namespace KK.App.Timing
{
    public interface IClock
    {
        // Реальное время: не должно зависеть от паузы и Time.timeScale
        TimeSpan Now { get; }
    }
}
