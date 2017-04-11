using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    class Start
    {
        private static void Main(string[] args)
        {
           
            NES nes = new NES();
            nes.reset();
            nes.Start();
            
        }
    }
}
