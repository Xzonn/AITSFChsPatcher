using static UnityAsset.Assets;

namespace UnityAsset
{
    public class Object
    {
        public ObjectInfo objectInfo;
        private readonly BinaryReaderExtended reader;
        public Object(ObjectInfo objectInfo, BinaryReaderExtended reader)
        {
            this.objectInfo = objectInfo;
            this.reader = reader;
        }
    }
}
