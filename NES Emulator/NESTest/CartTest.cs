using System;
using System.IO;
using FileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NESTest
{
    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void ReadFile()
        {
            //Arrange
            string FileName = @"C:\Users\panda\Downloads\Super Mario Bros. (USA).nes";
            FileReader.CartridgeReader cartReader = new CartridgeReader(FileName);
            Exception e = new FileNotFoundException();
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
    }
}
