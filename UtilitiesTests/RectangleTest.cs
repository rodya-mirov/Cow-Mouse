using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace UtilitiesTests
{
    [TestClass]
    public class RectangleTest
    {
        [TestMethod]
        public void RectangleTestMethod()
        {
            Rectangle a;


            for (int xmin = -5; xmin <= 5; xmin++)
            {
                for (int ymin = -5; ymin <= 5; ymin++)
                {
                    for (int width = 0; width <= 10; width++)
                    {
                        for (int height = 0; height <= 10; height++)
                        {
                            a = new Rectangle(xmin, ymin, width, height);

                            Assert.AreEqual(xmin, a.X);
                            Assert.AreEqual(ymin, a.Y);
                            Assert.AreEqual(width, a.Width);
                            Assert.AreEqual(height, a.Height);

                            Assert.AreEqual(xmin, a.Left);
                            Assert.AreEqual(ymin, a.Top);
                            Assert.AreEqual(xmin + width, a.Right);
                            Assert.AreEqual(ymin + height, a.Bottom);

                            for (int x = a.Left; x < a.Right; x++)
                            {
                                for (int y = a.Top; y < a.Bottom; y++)
                                {
                                    Assert.IsTrue(a.Contains(x, y));
                                }
                            }

                            for (int x = a.Left; x <= a.Right; x++)
                                Assert.IsFalse(a.Contains(x, a.Bottom));

                            for (int y = a.Top; y <= a.Bottom; y++)
                                Assert.IsFalse(a.Contains(a.Right, y));
                        }
                    }
                }
            }
        }
    }
}
