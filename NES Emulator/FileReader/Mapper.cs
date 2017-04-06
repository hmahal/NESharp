using NESEmu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    abstract class Mapper
    {
        public Mapper(Cartridge cart) { }
        public abstract byte read(ushort addr);
        public abstract void write(ushort addr, byte value);
    }
}
