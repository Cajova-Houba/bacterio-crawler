using BacterioCrawler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BacterioCrawlerTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestIsEmptyItem_emptyString()
        {
            Assert.IsTrue(Parser.IsEmptyItem(""));
        }

        [TestMethod]
        public void TestIsEmptyItem_spacePrefix()
        {
            Assert.IsTrue(Parser.IsEmptyItem("   s__"));
        }

        [TestMethod]
        public void TestIsEmptyItem_noSpacePrefix()
        {
            Assert.IsTrue(Parser.IsEmptyItem("s__"));
        }
        [TestMethod]
        public void TestIsEmptyItem_noDashPostfix()
        {
            Assert.IsTrue(Parser.IsEmptyItem("acnes"));
        }
    }
}
