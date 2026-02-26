using NESEmu;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace NESTest
{
    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void ReadFileThrowFileNotFoundException()
        {
            //Arrange
            string FileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".nes");
            NESEmu.CartridgeReader cartReader = new CartridgeReader(FileName);

            //Act + Assert
            Assert.ThrowsException<FileNotFoundException>(() => cartReader.readCart());
        }

        [TestMethod]
        public void ReadFile_CheckThreeBytes()
        {
            //Arrange
            string FileName = Path.GetTempFileName();
            File.WriteAllBytes(FileName, new byte[] { (byte)'N', (byte)'E', (byte)'S', 0x1A, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
                .Concat(new byte[16384]).Concat(new byte[8192]).ToArray());
            try
            {
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
            finally
            {
                File.Delete(FileName);
            }
        }
    }
}
