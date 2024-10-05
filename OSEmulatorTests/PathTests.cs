using OSEmulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSEmulatorTests
{
    [TestClass]
    public class PathTests
    {
        public static string OSDir = "C:\\Users\\Policarp\\Desktop\\VirtualOS.zip";
        [TestMethod]
        public void TestCurrentPathOnCreation()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            Assert.AreEqual("", path.CurrentPath);
        }
        [TestMethod]
        public void TestPathDoesNotGoOutside()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/Policarp/work");
            path.SetPosition("..");
            path.SetPosition("..");
            path.SetPosition("..");
            path.SetPosition("..");
            path.SetPosition("..");
            Assert.AreEqual("", path.CurrentPath);
        }
        [TestMethod]
        public void TestPathWalkthroughWithRoot()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("..");
            path.SetPosition("/bin");
            Assert.AreEqual("/bin", path.CurrentPath);
        }
        [TestMethod]
        public void TestPathWalkthroughWithBackFolder()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/Policarp");
            path.SetPosition("../root");
            Assert.AreEqual("/home/root", path.CurrentPath);
        }
        [TestMethod]
        public void TestPathWalkthroughWithCurrentFolder()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/Policarp");
            path.SetPosition("./work");
            Assert.AreEqual("/home/Policarp/work", path.CurrentPath);
        }
        [TestMethod]
        public void TestPathNotChangedWhenWrong()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/root/passwords/bin");
            Assert.AreEqual("", path.CurrentPath);
        }

    }
}
