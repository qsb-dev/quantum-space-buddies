using System;

namespace QSB
{
    [Flags]
    public enum State
    {
        None = 0,
        Flashlight = 1,
        Suit = 2,
        ProbeLauncher = 4,
        Signalscope = 8,
        Translator = 16,
        ProbeActive = 32
        //Increment these in binary to add more states
    }
}