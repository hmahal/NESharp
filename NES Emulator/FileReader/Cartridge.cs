namespace FileReader
{
    public class Cartridge
    {
        private const int _prgromConst = 16384;
        private const int _chrromConst = 8192;
        private const int _trainerConst = 512;

        public byte[] Header { get; set; }
        public byte[] Prgrom { get; set; }
        public byte[] Chrrom { get; set; }
        public byte[] Trainer { get; set; }
        public byte[] Title { get; set; }
    }
}