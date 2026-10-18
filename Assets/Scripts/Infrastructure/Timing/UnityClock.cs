using KK.App.Timing;
using System;
using UnityEngine;

namespace KK.Infrastructure.Timing
{
    public class UnityClock : IClock
    {
        public TimeSpan Now => TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble);
    }
}
