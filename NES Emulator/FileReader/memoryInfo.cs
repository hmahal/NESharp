using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    struct MemoryInfo
    {
        public ushort Address { get; set; }
        public ushort PC_register { get; set; }
        public int Addr_mode { get; set; }

        public MemoryInfo(ushort addr, ushort pCounter, int mode)
        {
            Address = addr;
            PC_register = pCounter;
            Addr_mode = mode;
        }
    }
}
