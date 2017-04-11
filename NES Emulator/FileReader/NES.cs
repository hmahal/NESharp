using NESEmu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//TODO: Fix namespaces, they are all over the place right now.
namespace NESEmu
{
    class NES
    {
        public Cartridge Cart { get; }
        public CPU6502 CPU { get; }
        public Memory RAM { get; }
        public PPU ppu { get; }
        public Mapper Mapper { get; }


        public NES()
        {
            Cart = new Cartridge();
            const string FileName = @"C:\Users\panda\Downloads\Star Wars - The Empire Strikes Back (USA).nes";
            //TODO:Fix this
            Input input1 = new Input();
            Input input2 = new Input();
            ppu = PPU.Instance;
            Cart = Cart.getCart(FileName);
            Mapper = new MMC3(Cart);
            //Create and pass the mapper to the memory
            Memory.Create(2048, Mapper, input1, input2);
            RAM = Memory.Instance;
            //Pass the memory to the CPU & PPU
            CPU6502.Create(RAM);
            CPU = CPU6502.Instance;           
        }

        public void reset()
        {
            CPU.Reset();
        }

        public void Start()
        {
            for (int j = 0; j < 10000000; j++)
            {
                uint cycles = CPU.Tick();
                uint ppuCycles = cycles * 3;
                for (int i = 0; i < ppuCycles; i++)
                {
                    ppu.run();
                    Mapper.Tick();
                }                
            }
        }

    }
}
