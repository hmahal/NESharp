namespace FileReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. 3 (USA).nes";
            CartridgeReader cartReader = new CartridgeReader(FileName);
        }
    }
}