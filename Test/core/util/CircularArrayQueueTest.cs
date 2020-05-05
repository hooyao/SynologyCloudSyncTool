using System.IO;
using com.hy.synology.filemanager.core.util;
using NUnit.Framework;

namespace com.hy.synology.filemanager.test.core.util
{
    public class CircularArrayQueueTest
    {
        [Test]
        public void GetCapacity_Returns_Log_Of_BaseCapacity()
        {
            CircularArrayQueue c3 = new CircularArrayQueue(3);
            Assert.That(c3.GetCapacity(1), Is.EqualTo(3));
            Assert.That(c3.GetCapacity(20), Is.EqualTo(24));
            CircularArrayQueue c1 = new CircularArrayQueue(1);
            Assert.That(c1.GetCapacity(1), Is.EqualTo(1));
            Assert.That(c1.GetCapacity(4), Is.EqualTo(4));
        }

        [Test]
        public void Constructor_Throws_Exception_When_Init_Capacity_Zero()
        {
            Assert.Throws<InvalidDataException>(code: () => new CircularArrayQueue(0));
        }
    }
}