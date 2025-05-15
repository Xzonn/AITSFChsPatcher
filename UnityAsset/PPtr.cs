namespace UnityAsset
{
    public sealed class PPtr<T> where T : Object
    {
        public int m_FileID;
        public long m_PathID;

        public PPtr(BinaryReaderExtended reader)
        {
            m_FileID = reader.ReadInt32();
            m_PathID = reader.ReadInt64();
        }
    }
}
