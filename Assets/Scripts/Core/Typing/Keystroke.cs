using System;

namespace KK.Core.Typing
{
    public readonly struct Keystroke
    {
        public readonly int Index;
        public readonly char Expected;
        public readonly char Typed;
        public readonly TimeSpan Time;

        public bool IsCorrect => Expected == Typed;

        public Keystroke(int index, char expected, char typed, TimeSpan time)
        {
            Index = index;
            Expected = expected;
            Typed = typed;
            Time = time;
        }
    }
}
