using NESEmu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    public abstract class Mapper
    {
        public Cartridge cart { get; set; }

        /// <summary>
        /// Constructor for the Mapper class.  MMVC3 is used.  
        /// </summary>
        /// <param name="cart"></param>
        public Mapper(Cartridge cart)
        {
            this.cart = cart;
        }

        public abstract byte read(ushort addr);
        public abstract void Tick();
        public abstract void write(ushort addr, byte value);
    }
}