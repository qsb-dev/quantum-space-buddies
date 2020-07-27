using System;

namespace QSB
{
    [Flags]
    public enum State
    {
        Flashlight = 0,
        Suit = 1,
        ProbeLauncher = 2,
        SignalScope = 4
        //Increment these in binary to add more states
    }
}