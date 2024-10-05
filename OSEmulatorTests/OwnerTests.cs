using OSEmulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSEmulatorTests
{
    [TestClass]
    public class OwnerTests
    {
        public static string OSDir = "C:\\Users\\Policarp\\Desktop\\VirtualOS.zip";
        [TestMethod]
        public void TestEmptyFileOwner()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            Assert.AreEqual("Fail", path.GetOwner("/bin/policarp"));
        }
        [TestMethod]
        public void TestDefaultOwner()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            Assert.AreEqual("admin", path.GetOwner("/home/Policarp"));
        }
        [TestMethod]
        public void TestOwnerChanged()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.ChangeOwner("/home/Policarp", "Policarp");
            Assert.AreEqual("Policarp", path.GetOwner("/home/Policarp"));
        }
    }
}
