using static UnityAsset.Assets;

namespace UnityAsset
{
    public class MonoBehaviour : Object
    {
        public PPtr<Object> m_GameObject;
        public byte m_Enabled;
        public PPtr<Object> m_Script;
        public string m_Name;

        public MonoBehaviour(ObjectInfo objectInfo, BinaryReaderExtended reader) : base(objectInfo, reader)
        {
            m_GameObject = new PPtr<Object>(reader);
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
            m_Script = new PPtr<Object>(reader);
            m_Name = reader.ReadAlignedString();
        }
    }
}
