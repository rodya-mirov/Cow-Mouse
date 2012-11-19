using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace UtilitiesTests
{
    /// <summary>
    /// Summary description for ThreadingTest
    /// </summary>
    [TestClass]
    public class ThreadingTest
    {
        public ThreadingTest()
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
        public void GeneralThreadTest()
        {
            //This is just me checking that threading works the way I think it does
            Thread testThread = new Thread(new ThreadStart(ThreadingTest.Derp));

            Assert.IsFalse(testThread.IsAlive);

            testThread.Start();

            while (testThread.IsAlive)
            {
                Thread.Sleep(1);
            }

            Assert.IsFalse(testThread.IsAlive);
        }

        private static void Derp()
        {
            for (int reps = 0; reps < 100; reps++)
            {
                for (int i = 0; i < 100000; i++)
                {
                }
                Console.WriteLine(reps);
            }
        }
    }
}
