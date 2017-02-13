using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
{
    class Program
    {

        static void Main(string[] args)
        {
            const string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. 3 (USA).nes";
            CartridgeReader cartReader = new CartridgeReader(FileName);
        }


    }
}
