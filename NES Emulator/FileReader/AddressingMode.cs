using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    /// <summary>
    /// Enum values for the 13 different addressing modes for the 6502 CPU.
    /// More information on the addressing modes can be found here:
    /// https://wiki.nesdev.com/w/index.php/CPU_addressing_modes
    /// </summary>
    enum AddressingMode
    {
        Absolute = 1,
        AbsoluteX = 2,
        AbsoluteY = 3,
        Accumulator = 4,
        Immediate= 5,
        Implied = 6,
        IndirectX = 7,
        Indirect = 8,
        IndirectY = 9,
        Relative = 10,
        ZeroPage = 11,
        ZeroPageX = 12,
        ZeroPageY = 13
    }
}
