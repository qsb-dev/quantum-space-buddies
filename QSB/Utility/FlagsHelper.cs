using System;
using System.Collections.Generic;

namespace QSB.Utility
{
    // Stolen from here : https://stackoverflow.com/questions/3261451/using-a-bitmask-in-c-sharp

    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }

        public static List<string> FlagsToListSet<T>(T flags) where T : struct
        {
            var temp = new List<string>();
            var array = (T[])Enum.GetValues(flags.GetType());
            Array.ForEach(array, x => temp.Add(Convert.ToString(IsSet(flags, x) ? 1 : 0)));
            return temp;
        }
    }
}
