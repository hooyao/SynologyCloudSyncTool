using System.IO;
using com.hy.synology.filemanager.core.util;
using NUnit.Framework;
using Org.BouncyCastle.Bcpg;

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
        
        [Test]
        public void Size_Returns_The_Correct_Size()
        {
            CircularArrayQueue c5 = new CircularArrayQueue(5);
            c5.EnQueue(new byte[]{1,2,3,4});
            c5.EnQueue(new byte[]{5,6,7,8});
            Assert.That(c5.Size, Is.EqualTo(8));
            byte[] dq1 = c5.DeQueue(3);
            Assert.That(BytesUtils.ByteArrayCompare(dq1,new byte[]{1,2,3}),Is.True);
            
            c5.EnQueue(new byte[]{9,10,11,12,13});
            Assert.That(c5.Size, Is.EqualTo(10));
            byte[] dq2 = c5.DeQueue(6);
            Assert.That(BytesUtils.ByteArrayCompare(dq2,new byte[]{4,5,6,7,8,9}),Is.True);
            byte[] dq3 = c5.DeQueue(3);
            Assert.That(BytesUtils.ByteArrayCompare(dq3,new byte[]{10,11,12}),Is.True);
            Assert.That(c5.Size, Is.EqualTo(1));
            byte[] dq4 = c5.DeQueue(1);
            Assert.That(BytesUtils.ByteArrayCompare(dq4,new byte[]{13}),Is.True);
        }
    }
}