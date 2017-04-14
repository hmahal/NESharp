namespace NESEmu
{
    /// <summary>
    /// NES class written to run quick tests and run without the WPF overhead.
    /// </summary>
    class NES
    {
        public Cartridge Cart { get; }
        public CPU6502 CPU { get; }
        public Memory RAM { get; }
        public PPU ppu { get; }
        public Mapper Mapper { get; }

        /// <summary>
        /// Contructor for the NES console object. Uses Dependency Injection to create components.
        /// Mainly used for testing in this form.
        /// </summary>
        public NES()
        {
            Cart = new Cartridge();
            const string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. 3 (USA).nes";            
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

        /// <summary>
        /// Test run for the NES console used for debugging.
        /// </summary>
        public void Start()
        {
            //Runs an arbitrary number of ticks for the CPU
            for (int j = 0; j < 10000000; j++)
            {
                uint cycles = CPU.Tick();
                uint ppuCycles = cycles * 3;
                for (int i = 0; i < ppuCycles; i++)
                {
                    ppu.Step();
                    Mapper.Tick();
                }                
            }
        }

    }
}
