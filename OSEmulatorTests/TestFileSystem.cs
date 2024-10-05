using OSEmulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSEmulatorTests
{
    [TestClass]
    public class TestFileSystem
    {
        public static string OSDir = "C:\\Users\\Policarp\\Desktop\\VirtualOS.zip";
        [TestMethod]
        public void TestListRoot()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/");
            string[] expected = ["bin", "dev", "etc", "home"];
            int i = 0;
            foreach(var item in path.GetDirectoryFiles())
            {
                Assert.AreEqual(expected[i++], item.Item1);
                Console.WriteLine(item);
            }
            Assert.AreEqual(i, expected.Length);
        }
        [TestMethod]
        public void TestPolicarp()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/Policarp");
            string[] expected = ["config.txt", "work", "YTActivation.txt"];
            int i = 0;
            foreach (var item in path.GetDirectoryFiles())
            {
                //Assert.AreEqual(expected[i++], item.Item1);
                Console.WriteLine(item);
            }
            //Assert.AreEqual(i, expected.Length);
        }
        [TestMethod]
        public void TestPolicarpWithOwners()
        {
            VirtualPath path = new VirtualPath(OSDir, "admin");
            path.SetPosition("/home/Policarp");
            string[] expected = ["config.txt", "work", "YTActivation.txt"];
            string[] ownership = ["Policarp", "admin", "Policarp"];
            path.ChangeOwner("/home/Policarp/config.txt", "Policarp");
            path.ChangeOwner("/home/Policarp/YTActivation.txt", "Policarp");
            int i = 0;
            foreach (var item in path.GetDirectoryFiles())
            {
                Assert.AreEqual(expected[i], item.Item1);
                Assert.AreEqual(ownership[i++], path.GetOwner(item.Item2));
                Console.WriteLine(item);
            }
            Assert.AreEqual(i, expected.Length);
        }
    }
}
