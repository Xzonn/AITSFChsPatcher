using System;
using System.Collections.Generic;

namespace UnityAsset
{
    public partial class Assets
    {
        public uint MetadataSize;
        public uint FileSize;
        public uint Version;
        public uint DataOffset;
        public byte Endianess;
        public byte[] Reserved;

        public string UnityVersion;
        public uint TargetPlatform;

        public bool EnableTypeTree;
        public int TypeCount;
        public List<SerializedType> Types;

        public int ObjectCount;
        public List<ObjectInfo> Objects;

        public int ScriptCount;
        public List<LocalSerializedObjectIdentifier> ScriptTypes;

        public int ExternalsCount;
        public List<FileIdentifier> Externals;

        public string UserInformation;

        public string replacePath;
        public string assetsName;
        public readonly TypeTree MonoBehaviourTree;
        public readonly TypeTree Texture2DTree;

        public class SerializedType
        {
            public int classID;
            public bool m_IsStrippedType;
            public short m_ScriptTypeIndex = -1;
            public TypeTree m_Type;
            public byte[] m_ScriptID; //Hash128
            public byte[] m_OldTypeHash; //Hash128
            public int[] m_TypeDependencies;
            public string m_KlassName;
            public string m_NameSpace;
            public string m_AsmName;
        }

        public class TypeTree
        {
            public List<TypeTreeNode> m_Nodes;
            public byte[] m_StringBuffer;

            // For Dump
            public int NumberOfNodes;
            public int StringBufferSize;
        }

        public class TypeTreeNode
        {
            public string m_Type;
            public string m_Name;
            public int m_ByteSize;
            public int m_Index;
            public byte m_TypeFlags; //m_IsArray
            public ushort m_Version;
            public int m_MetaFlag;
            public byte m_Level;
            public uint m_TypeStrOffset;
            public uint m_NameStrOffset;
            public ulong m_RefTypeHash;

            public TypeTreeNode() { }

            public TypeTreeNode(string type, string name, byte level, bool align)
            {
                m_Type = type;
                m_Name = name;
                m_Level = level;
                m_MetaFlag = align ? 0x4000 : 0;
            }
        }

        public class ObjectInfo
        {
            public long byteStart;
            public uint byteSize;
            public int typeID;
            public int classID;
            public ushort isDestroyed;
            public byte stripped;

            public long m_PathID;
            public SerializedType serializedType;
        }

        public class LocalSerializedObjectIdentifier
        {
            public int localSerializedFileIndex;
            public long localIdentifierInFile;
        }

        public class FileIdentifier
        {
            public Guid guid;
            public int type; //enum { kNonAssetType = 0, kDeprecatedCachedAssetType = 1, kSerializedAssetType = 2, kMetaAssetType = 3 };
            public string pathName;

            //custom
            public string fileName;

            // For Dump
            public string tempEmpty;
        }
    }
}
