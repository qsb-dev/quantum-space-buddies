using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Animation
{
    public class QSBTool : PlayerTool
    {
        // This class is actually needed. PlayerTool is abstract, but I don't want to change
        // any of the functionality of PlayerTool. Yes, I hate this too.

        public ToolType Type;
    }
}
