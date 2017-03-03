using FileReader;
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
            FileReader.CartridgeReader cartReader = new CartridgeReader(FileName);
            //Act
            try
            {
                FileReader.Cartridge cart = cartReader.readCart();
            }
            catch (FileNotFoundException exception)
            {
                //Assert
                StringAssert.Equals(exception.Message, cartReader.FileNotFound.Message);
            }
        }

        [TestMethod]
        public void ReadFile_CheckThreeBytes()
        {
            //Arrange
            string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. 3 (USA).nes";
            FileReader.CartridgeReader cartReader = new CartridgeReader(FileName);
            byte firstByte = (byte)'N';
            byte secondByte = (byte)'E';
            byte thirdByte = (byte)'S';
            //Act
            FileReader.Cartridge cart = cartReader.readCart();

            //Assert
            Assert.AreEqual(cart.Header[0], firstByte);
            Assert.AreEqual(cart.Header[1], secondByte);
            Assert.AreEqual(cart.Header[2], thirdByte);
        }
    }
}