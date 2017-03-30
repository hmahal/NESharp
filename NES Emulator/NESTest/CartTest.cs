using NESEmu;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace NESTest
{
    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void ReadFileThrowFileNotFoundException()
        {
            //Arrange
            string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. (USA).nes";
            NESEmu.CartridgeReader cartReader = new CartridgeReader(FileName);
            //Act
            try
            {
                NESEmu.Cartridge cart = cartReader.readCart();
            }
            catch (FileNotFoundException exception)
            {
                //Assert
                StringAssert.Equals(exception, new FileNotFoundException());
            }
        }

        [TestMethod]
        public void ReadFile_CheckThreeBytes()
        {
            //Arrange
            string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. 3 (USA).nes";
            NESEmu.CartridgeReader cartReader = new CartridgeReader(FileName);
            byte firstByte = (byte)'N';
            byte secondByte = (byte)'E';
            byte thirdByte = (byte)'S';
            //Act
            NESEmu.Cartridge cart = cartReader.readCart();

            //Assert
            Assert.AreEqual(cart.Header[0], firstByte);
            Assert.AreEqual(cart.Header[1], secondByte);
            Assert.AreEqual(cart.Header[2], thirdByte);
        }
    }
}