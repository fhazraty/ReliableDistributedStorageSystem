namespace FullNode
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// This method is used to split files in an efficient way
        /// </summary>
        /// <param name="value">Binary which is needed to split</param>
        /// <param name="bufferLength">The split buffer size</param>
        /// <returns>Splitted file as byte array</returns>
        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;
            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }
    }
}
