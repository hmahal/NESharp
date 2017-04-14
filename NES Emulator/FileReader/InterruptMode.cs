using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    /// <summary>
    /// Enum definition for the three interrupt modes for 6502 CPU.
    /// </summary>
    enum InterruptMode
    {
        NoneInterrupt = 1,
        NMIInterrupt = 2,
        IRQInterrupt = 3
    }
}
