namespace NESEmu
{
    public class Cartridge
    {
        private const int _prgromConst = 16384;
        private const int _chrromConst = 8192;
        private const int _trainerConst = 512;

        public bool VerticalMirroring { get; set; }
        public bool Save_RAM { get; set; }
        public bool Trainer_Present { get; set; }
        public bool Four_Screen_Mirroring { get; set; }

        public int mapper { get; set; }

        public byte[] Sram { get; set; }
        public byte[] Header { get; set; }
        public byte[] Prgrom { get; set; }
        public byte[] Chrrom { get; set; }
        public byte[] Trainer { get; set; }
        public byte[] Title { get; set; }
    }
}