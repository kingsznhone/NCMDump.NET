using NCMDump.Core;

namespace NCMDump.Test
{
    [TestClass]
    public sealed class NcmRC4Tests
    {
        [TestMethod]
        public void Transform_ShouldReturnCorrectLength()
        {
            // Arrange
            byte[] key = [0x01, 0x02, 0x03, 0x04];
            var rc4 = new NcmRC4(key);
            Span<byte> data = stackalloc byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            byte originalFirstByte = data[0];

            // Act
            int result = rc4.Transform(data);

            // Assert
            Assert.AreEqual(5, result, "Should return the length of transformed data");
            Assert.AreNotEqual(originalFirstByte, data[0], "Data should be transformed in-place");
        }

        [TestMethod]
        public void Transform_IsReversible()
        {
            // Arrange
            byte[] key = [0x01, 0x02, 0x03, 0x04];
            byte[] originalData = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            byte[] testData = new byte[originalData.Length];
            originalData.CopyTo(testData, 0);

            // Act - First transformation
            var rc4_1 = new NcmRC4(key);
            rc4_1.Transform(testData);

            // Act - Second transformation with same key (should restore original)
            var rc4_2 = new NcmRC4(key);
            rc4_2.Transform(testData);

            // Assert
            CollectionAssert.AreEqual(originalData, testData, "Double transformation should restore original data");
        }

        [TestMethod]
        public void Transform_EmptyData_ShouldReturnZero()
        {
            // Arrange
            byte[] key = [0x01, 0x02, 0x03, 0x04];
            var rc4 = new NcmRC4(key);
            Span<byte> emptyData = Span<byte>.Empty;

            // Act
            int result = rc4.Transform(emptyData);

            // Assert
            Assert.AreEqual(0, result, "Transform of empty data should return 0");
        }

        [TestMethod]
        public void Transform_DifferentKeys_ProduceDifferentResults()
        {
            // Arrange
            byte[] key1 = [0x01, 0x02, 0x03, 0x04];
            byte[] key2 = [0x05, 0x06, 0x07, 0x08];
            byte[] data1 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            byte[] data2 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];

            var rc4_1 = new NcmRC4(key1);
            var rc4_2 = new NcmRC4(key2);

            // Act
            rc4_1.Transform(data1);
            rc4_2.Transform(data2);

            // Assert
            CollectionAssert.AreNotEqual(data1, data2, "Different keys should produce different results");
        }

        [TestMethod]
        public void Transform_ConsistentResults_SameKeyAndData()
        {
            // Arrange
            byte[] key = [0x01, 0x02, 0x03, 0x04];
            byte[] data1 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            byte[] data2 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];

            var rc4_1 = new NcmRC4(key);
            var rc4_2 = new NcmRC4(key);

            // Act
            rc4_1.Transform(data1);
            rc4_2.Transform(data2);

            // Assert
            CollectionAssert.AreEqual(data1, data2, "Same key and data should produce identical results");
        }

        [TestMethod]
        public void Transform_TestNative()
        {
            byte[] key = [0x01, 0x02, 0x03, 0x04];
            byte[] data1 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            byte[] data2 = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
            var rc4_1 = new NcmRC4(key);
            var rc4_2 = new NcmRC4Native(key);

            rc4_1.Transform(data1);
            rc4_2.Transform(data2);

            // Assert
            CollectionAssert.AreEqual(data1, data2, "Same key and data should produce identical results");
        }
    }
}