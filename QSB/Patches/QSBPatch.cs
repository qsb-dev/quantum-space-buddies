using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB
{
    public abstract class QSBPatch
    {
        public abstract QSBPatchTypes Type { get; }

        public abstract void DoPatches();
    }
}
