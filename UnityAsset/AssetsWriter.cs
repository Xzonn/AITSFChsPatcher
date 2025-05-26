using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnityAsset
{
    public partial class Assets
    {
        private BinaryWriterExtended writer;
        public void DumpRaw(BinaryWriterExtended writer, BinaryReader resourcesReader, BinaryWriter resourcesWriter)
        {
            this.writer = writer;
            writer.Position = 0;
            // Write Header
            writer.WriteBE(MetadataSize);
            writer.WriteBE(FileSize);
            writer.WriteBE(Version);
            writer.WriteBE(DataOffset);

            writer.Write(Endianess);
            writer.Write(Reserved, 0, 3);

            writer.WriteStringToNull(UnityVersion);
            writer.Write(TargetPlatform);

            // Write Types
            writer.Write(EnableTypeTree);
            writer.Write(TypeCount);
            foreach (var type in Types)
            {
                WriteSerializedType(type, false);
            }

            // Write Objects
            writer.Write(ObjectCount);
            writer.AlignStream();
            WriteObjects(resourcesReader, resourcesWriter);

            // Write Scripts
            writer.Write(ScriptCount);
            foreach (var m_ScriptType in ScriptTypes)
            {
                writer.Write(m_ScriptType.localSerializedFileIndex);
                writer.Write(m_ScriptType.localIdentifierInFile);
            }

            // Write Externals
            writer.Write(ExternalsCount);
            foreach (var m_External in Externals)
            {
                writer.WriteStringToNull(m_External.tempEmpty);
                writer.Write(m_External.guid.ToByteArray());
                writer.Write(m_External.type);
                writer.WriteStringToNull(m_External.pathName);
            }

            writer.WriteStringToNull(UserInformation);
            writer.Flush();

            writer.Position = 4;
            writer.WriteBE((uint)writer.BaseStream.Length);
        }

        private void WriteSerializedType(SerializedType type, bool isRefType)
        {
            writer.Write(type.classID);
            writer.Write(type.m_IsStrippedType);
            writer.Write(type.m_ScriptTypeIndex);
            if (isRefType && type.m_ScriptTypeIndex >= 0)
            {
                writer.Write(type.m_ScriptID, 0, 16);
            }
            else if (type.classID == 114)
            {
                writer.Write(type.m_ScriptID, 0, 16);
            }
            writer.Write(type.m_OldTypeHash, 0, 16);

            if (EnableTypeTree)
            {
                TypeTreeBlobWrite(type.m_Type);
            }
        }

        private void TypeTreeBlobWrite(TypeTree m_Type)
        {
            writer.Write(m_Type.NumberOfNodes);
            writer.Write(m_Type.StringBufferSize);
            for (int i = 0; i < m_Type.NumberOfNodes; i++)
            {
                var typeTreeNode = m_Type.m_Nodes[i];
                writer.Write(typeTreeNode.m_Version);
                writer.Write(typeTreeNode.m_Level);
                writer.Write(typeTreeNode.m_TypeFlags);
                writer.Write(typeTreeNode.m_TypeStrOffset);
                writer.Write(typeTreeNode.m_NameStrOffset);
                writer.Write(typeTreeNode.m_ByteSize);
                writer.Write(typeTreeNode.m_Index);
                writer.Write(typeTreeNode.m_MetaFlag);
            }
            writer.Write(m_Type.m_StringBuffer, 0, m_Type.StringBufferSize);
        }

        private class ObjectOrder
        {
            public ObjectInfo objectInfo;
            public string replacePath;
            public long position;
        }

        private class TextureOrder
        {
            public ObjectInfo objectInfo;
            public long position;
            public uint resourcePosition;
            public uint resourceSize;
            public OrderedDictionary dictionary;
        }

        private class TextureSize
        {
            public int width;
            public int height;
        }

        private void WriteObjects(BinaryReader resourcesReader, BinaryWriter resourcesWriter)
        {
            List<ObjectOrder> objectOrders = [];
            for (int i = 0; i < ObjectCount; i++)
            {
                var objectInfo = Objects[i];
                writer.Write(objectInfo.m_PathID);
                var objectOrder = new ObjectOrder
                {
                    objectInfo = objectInfo,
                    position = writer.Position
                };
                writer.Write((uint)0);
                string fullReplacePath = Path.Combine(replacePath, $"{assetsName}/{(ClassIDType)objectInfo.classID}/{objectInfo.m_PathID:x016}.asset");
                if (File.Exists(fullReplacePath))
                {
                    var fileInfo = new FileInfo(fullReplacePath);
                    if (fileInfo.Length > 0)
                    {
                        objectOrder.replacePath = fullReplacePath;
                        writer.Write((uint)fileInfo.Length);
                    }
                    else
                    {
                        writer.Write(objectInfo.byteSize);
                    }
                }
                else
                {
                    writer.Write(objectInfo.byteSize);
                }
                objectOrders.Add(objectOrder);
                writer.Write(objectInfo.typeID);
            }
            long writerPosition = writer.Position;

            objectOrders.Sort((x, y) => x.objectInfo.byteStart.CompareTo(y.objectInfo.byteStart));
            List<TextureOrder> textureOrders = [];
            Dictionary<long, TextureSize> textureSizes = [];

            uint dataPosition = 0;
            foreach (var objectOrder in objectOrders)
            {
                writer.Position = objectOrder.position;
                writer.Write(dataPosition);
                writer.Position = DataOffset + dataPosition;
                byte[] bytes;
                if (!string.IsNullOrEmpty(objectOrder.replacePath))
                {
                    bytes = File.ReadAllBytes(objectOrder.replacePath);
                }
                else
                {
                    reader.Position = objectOrder.objectInfo.byteStart;
                    bytes = reader.ReadBytes((int)objectOrder.objectInfo.byteSize);
                }

                BinaryReaderExtended binaryReader = new(new MemoryStream(bytes));

                int classID = Types[objectOrder.objectInfo.typeID].classID;
                if (classID == (int)ClassIDType.MonoBehaviour && !string.IsNullOrEmpty(objectOrder.replacePath)) // MonoBehaviour
                {
                    OrderedDictionary dictionary = TypeTreeHelper.ReadType(MonoBehaviourTree, binaryReader);
                    long atlasPathID = (long)((OrderedDictionary)dictionary["atlas"])["m_PathID"];
                    int atlasWidth = (int)(float)((OrderedDictionary)dictionary["m_fontInfo"])["AtlasWidth"];
                    int atlasHeight = (int)(float)((OrderedDictionary)dictionary["m_fontInfo"])["AtlasHeight"];
                    textureSizes[atlasPathID] = new TextureSize { height = atlasHeight, width = atlasWidth };
                }
                else if (classID == (int)ClassIDType.Texture2D) // Texture2D
                {
                    OrderedDictionary dictionary = TypeTreeHelper.ReadType(Texture2DTree, binaryReader);
                    string path = (string)((OrderedDictionary)dictionary["m_StreamData"])["path"];
                    if (string.IsNullOrEmpty(path))
                    {
                        long pathID = objectOrder.objectInfo.m_PathID;
                        string fullReplacePath = Path.Combine(replacePath, $"{assetsName}/Texture2D/{pathID:x016}.res");
                        if (File.Exists(fullReplacePath))
                        {
                            var newBytes = File.ReadAllBytes(fullReplacePath);
                            var originalSize = (int)dictionary["m_CompleteImageSize"];
                            if (originalSize == newBytes.Length)
                            {
                                dictionary["image data"] = newBytes;
                            }
                            else
                            {
                                Debug.Assert(false, $"Texture2D {pathID:x016} size mismatch: expected {dictionary["m_CompleteImageSize"]}, got {newBytes.Length}");
                                dictionary["image data"] = newBytes.Take(originalSize).Concat(((byte[])dictionary["image data"]).Skip(newBytes.Length)).ToArray();
                            }
                            using var newStream = new MemoryStream();
                            using var newWriter = new BinaryWriterExtended(newStream);
                            TypeTreeHelper.WriteType(dictionary, Texture2DTree, newWriter);
                            bytes = newStream.ToArray();
                        }
                    }
                    else
                    {
                        uint offset = (uint)((OrderedDictionary)dictionary["m_StreamData"])["offset"];
                        uint size = (uint)((OrderedDictionary)dictionary["m_StreamData"])["size"];
                        var textureOrder = new TextureOrder
                        {
                            objectInfo = objectOrder.objectInfo,
                            position = writer.Position,
                            resourcePosition = offset,
                            resourceSize = size,
                            dictionary = dictionary
                        };
                        textureOrders.Add(textureOrder);
                    }
                }

                writer.Write(bytes);
                writer.AlignStream(8);
                dataPosition = (uint)writer.Position - DataOffset;
            }

            textureOrders.Sort((x, y) => x.resourcePosition.CompareTo(y.resourcePosition));
            foreach (var textureOrder in textureOrders)
            {
                byte[] bytes;
                OrderedDictionary dictionary = textureOrder.dictionary;

                long pathID = textureOrder.objectInfo.m_PathID;
                string fullReplacePath = Path.Combine(replacePath, $"{assetsName}/Texture2D/{pathID:x016}.res");
                if (File.Exists(fullReplacePath))
                {
                    bytes = File.ReadAllBytes(fullReplacePath);
                    ((OrderedDictionary)dictionary["m_StreamData"])["size"] = (uint)bytes.Length;
                    if (textureSizes.ContainsKey(pathID))
                    {
                        Debug.Assert(bytes.Length == textureSizes[pathID].width * textureSizes[pathID].height);
                        dictionary["m_Width"] = textureSizes[pathID].width;
                        dictionary["m_Height"] = textureSizes[pathID].height;
                    }
                }
                else
                {
                    resourcesReader.BaseStream.Position = textureOrder.resourcePosition;
                    bytes = resourcesReader.ReadBytes((int)textureOrder.resourceSize);
                }
                ((OrderedDictionary)dictionary["m_StreamData"])["offset"] = (uint)resourcesWriter.BaseStream.Position;
                resourcesWriter.Write(bytes);
                writer.Position = textureOrder.position;
                TypeTreeHelper.WriteType(dictionary, Texture2DTree, writer);
            }

            writer.Position = writerPosition;
        }
    }
}
