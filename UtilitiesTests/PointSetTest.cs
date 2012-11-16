using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesTests
{
    /// <summary>
    /// Summary description for PointSetTest
    /// </summary>
    [TestClass]
    public class PointSetTest
    {
        public PointSetTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void HashSetTest()
        {
            //This is all about testing intended behavior with C#'s hashset class.
            //Maybe I just don't trust hashsets like I'm supposed to.

            HashSet<P> set = new HashSet<P>();

            for (int x = -80; x < 80; x++)
            {
                for (int y = -80; y < 80; y++)
                {
                    Assert.IsFalse(set.Contains(new P(x, y)));
                    set.Add(new P(x, y));
                    Assert.IsTrue(set.Contains(new P(x, y)));
                }
            }
        }

        [TestMethod]
        public void DictionaryTest()
        {
            //dictionary is acting funny too?

            Dictionary<P, int> pDict = new Dictionary<P, int>();



            for (int x = -80; x < 80; x++)
            {
                for (int y = -80; y < 80; y++)
                {
                    Assert.IsFalse(pDict.Keys.Contains(new P(x, y)));
                    pDict[new P(x, y)] = x & y;
                    Assert.IsTrue(pDict.Keys.Contains(new P(x, y)));
                    Assert.AreEqual(pDict[new P(x, y)], x & y);
                }
            }
        }

        struct P
        {
            int x;
            int y;

            public P(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}
