using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UnityAsset
{
    public partial class Assets
    {
        private readonly BinaryReaderExtended reader;
        public Assets(BinaryReaderExtended reader, string replacePath, string assetsName)
        {
            this.reader = reader;
            this.replacePath = replacePath;
            this.assetsName = assetsName;
            reader.Position = 0;
            // Read Header
            MetadataSize = reader.ReadUInt32BE();
            FileSize = reader.ReadUInt32BE();
            Version = reader.ReadUInt32BE();
            Debug.Assert(Version == 17);
            DataOffset = reader.ReadUInt32BE();

            Endianess = reader.ReadByte();
            Debug.Assert(Endianess == 0);
            Reserved = reader.ReadBytes(3);

            UnityVersion = reader.ReadStringToNull();
            TargetPlatform = reader.ReadUInt32();

            // Read Types
            EnableTypeTree = reader.ReadBoolean();
            TypeCount = reader.ReadInt32();
            Types = new List<SerializedType>(TypeCount);
            for (int i = 0; i < TypeCount; i++)
            {
                Types.Add(ReadSerializedType(false));
            }

            // Read Objects
            ObjectCount = reader.ReadInt32();
            Objects = new List<ObjectInfo>(ObjectCount);
            reader.AlignStream();
            for (int i = 0; i < ObjectCount; i++)
            {
                var objectInfo = new ObjectInfo
                {
                    m_PathID = reader.ReadInt64(),
                    byteStart = reader.ReadUInt32() + DataOffset,
                    byteSize = reader.ReadUInt32(),
                    typeID = reader.ReadInt32()
                };
                var type = Types[objectInfo.typeID];
                objectInfo.serializedType = type;
                objectInfo.classID = type.classID;

                Objects.Add(objectInfo);
            }

            // Read Scripts
            ScriptCount = reader.ReadInt32();
            ScriptTypes = new List<LocalSerializedObjectIdentifier>(ScriptCount);
            for (int i = 0; i < ScriptCount; i++)
            {
                var m_ScriptType = new LocalSerializedObjectIdentifier
                {
                    localSerializedFileIndex = reader.ReadInt32(),
                    localIdentifierInFile = reader.ReadInt64()
                };
                ScriptTypes.Add(m_ScriptType);
            }

            // Read Externals
            ExternalsCount = reader.ReadInt32();
            Externals = new List<FileIdentifier>(ExternalsCount);
            for (int i = 0; i < ExternalsCount; i++)
            {
                var m_External = new FileIdentifier
                {
                    tempEmpty = reader.ReadStringToNull(),
                    guid = new Guid(reader.ReadBytes(16)),
                    type = reader.ReadInt32(),
                    pathName = reader.ReadStringToNull()
                };
                m_External.fileName = Path.GetFileName(m_External.pathName);
                Externals.Add(m_External);
            }

            UserInformation = reader.ReadStringToNull();

            reader.AlignStream(16);

            MonoBehaviourTree = new TypeTree
            {
                m_Nodes = new List<TypeTreeNode>()
            };
            FileStream fs = File.OpenRead(Path.Combine(replacePath, "MonoBehaviour.bin"));
            TypeTreeBlobRead(MonoBehaviourTree, new BinaryReaderExtended(fs));
            fs.Close();
            fs = File.OpenRead(Path.Combine(replacePath, "Texture2D.bin"));
            Texture2DTree = new TypeTree
            {
                m_Nodes = new List<TypeTreeNode>()
            };
            TypeTreeBlobRead(Texture2DTree, new BinaryReaderExtended(fs));
            fs.Close();
        }

        private SerializedType ReadSerializedType(bool isRefType)
        {
            var type = new SerializedType
            {
                classID = reader.ReadInt32(),
                m_IsStrippedType = reader.ReadBoolean(),
                m_ScriptTypeIndex = reader.ReadInt16()
            };
            if (isRefType && type.m_ScriptTypeIndex >= 0)
            {
                type.m_ScriptID = reader.ReadBytes(16);
            }
            else if (type.classID == 114)
            {
                type.m_ScriptID = reader.ReadBytes(16);
            }
            type.m_OldTypeHash = reader.ReadBytes(16);

            if (EnableTypeTree)
            {
                type.m_Type = new TypeTree
                {
                    m_Nodes = new List<TypeTreeNode>()
                };
                TypeTreeBlobRead(type.m_Type);
            }

            return type;
        }

        private void TypeTreeBlobRead(TypeTree m_Type) { TypeTreeBlobRead(m_Type, reader); }

        private void TypeTreeBlobRead(TypeTree m_Type, BinaryReaderExtended reader)
        {
            m_Type.NumberOfNodes = reader.ReadInt32();
            m_Type.StringBufferSize = reader.ReadInt32();
            for (int i = 0; i < m_Type.NumberOfNodes; i++)
            {
                var typeTreeNode = new TypeTreeNode();
                m_Type.m_Nodes.Add(typeTreeNode);
                typeTreeNode.m_Version = reader.ReadUInt16();
                typeTreeNode.m_Level = reader.ReadByte();
                typeTreeNode.m_TypeFlags = reader.ReadByte();
                typeTreeNode.m_TypeStrOffset = reader.ReadUInt32();
                typeTreeNode.m_NameStrOffset = reader.ReadUInt32();
                typeTreeNode.m_ByteSize = reader.ReadInt32();
                typeTreeNode.m_Index = reader.ReadInt32();
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }
            m_Type.m_StringBuffer = reader.ReadBytes(m_Type.StringBufferSize);

            using (var stringBufferReader = new BinaryReaderExtended(new MemoryStream(m_Type.m_StringBuffer)))
            {
                for (int i = 0; i < m_Type.NumberOfNodes; i++)
                {
                    var m_Node = m_Type.m_Nodes[i];
                    m_Node.m_Type = ReadString(stringBufferReader, m_Node.m_TypeStrOffset);
                    m_Node.m_Name = ReadString(stringBufferReader, m_Node.m_NameStrOffset);
                }
            }

            string ReadString(BinaryReaderExtended stringBufferReader, uint value)
            {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset)
                {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                var offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str))
                {
                    return str;
                }
                return offset.ToString();
            }
        }
    }
}
