using OSEmulator;

namespace OSEmulatorTests
{
    [TestClass]
    public class ArchiveTests
    {
        public static string OSDir = "C:\\Users\\Policarp\\Desktop\\VirtualOS.zip";
        [TestMethod]
        public void TestPathNotContainSlash()
        {
            ArchiveAccess archive = new ArchiveAccess(OSDir);
            foreach(var el in archive.GetArchivesFileNames())
            {
                Assert.IsFalse(el.StartsWith("VirtualOS"));
                Assert.IsFalse(el.EndsWith('/'));
            }
        }
    }
}
