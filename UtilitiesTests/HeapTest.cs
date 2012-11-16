using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CowMouse.Utilities;
using CowMouse.Utilities.DataStructures;

namespace UtilitiesTests
{
    /// <summary>
    /// Summary description for HeapTest
    /// </summary>
    [TestClass]
    public class HeapTest
    {
        public HeapTest()
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
        public void IntMaxHeapTest()
        {
            IntMaxHeap imh = new IntMaxHeap();

            for (int lengthOfTest = 10; lengthOfTest < 1000; lengthOfTest *= 10)
            {
                Assert.AreEqual(imh.Count, 0);

                try
                {
                    imh.Peek();
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                }

                for (int i = 0; i < lengthOfTest; i++)
                {
                    Assert.AreEqual(imh.Count, i);
                    imh.Add(i);
                    Assert.AreEqual(imh.Peek(), i);

                    Assert.IsTrue(SatisfiesPrecondition(imh));
                }

                Assert.IsTrue(imh.Count == lengthOfTest);
                Assert.IsTrue(SatisfiesPrecondition(imh));

                for (int i = 0; i < lengthOfTest; i++)
                {
                    Assert.AreEqual(imh.Pop(), lengthOfTest - 1 - i);
                    Assert.IsTrue(SatisfiesPrecondition(imh));
                    Assert.AreEqual(imh.Count, lengthOfTest - 1 - i);
                }
            }
        }

        private bool SatisfiesPrecondition(IntMaxHeap imh)
        {
            for (int n = 0; n < imh.Count; n++)
            {
                int top = imh.GetElementAt(n);

                if (2 * n + 1 < imh.Count)
                {
                    int left = imh.GetElementAt(2 * n + 1);
                    if (top < left)
                        return false;
                }

                if (2 * n + 2 < imh.Count)
                {
                    int right = imh.GetElementAt(2 * n + 2);
                    if (top < right)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Incredibly straight-forward heap example class.
        /// </summary>
        class IntMaxHeap : Heap<int>
        {
            public IntMaxHeap(int capacity = 50)
                : base(capacity)
            {
            }

            /// <summary>
            /// Max heap
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public override bool isBetter(int a, int b)
            {
                return a > b;
            }

            /// <summary>
            /// Exposed list in order to check the precondition a lot.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public int GetElementAt(int index)
            {
                return baseList[index];
            }
        }
    }
}
