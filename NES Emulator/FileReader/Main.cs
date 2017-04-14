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
        /// <summary>
        /// Entry point for the headless instance. Mainly used for debugging.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
           
            NES nes = new NES();
            nes.reset();
            nes.Start();
            
        }
    }
}
