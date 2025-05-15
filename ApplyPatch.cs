using UnityAsset;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Linq;

namespace AITheSomniumFilesChsPatch
{
    internal static partial class Program
    {
        public static void ApplyPatch(string basePath, out string tempPath)
        {
            // 检查文件是否存在
            Debug.Assert(Directory.Exists(basePath));
            Debug.Assert(File.Exists(Path.Combine(basePath, "AI_TheSomniumFiles.exe")));
            var resourcesAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/resources.assets");
            Debug.Assert(File.Exists(resourcesAssetsPath));
            Debug.Assert(File.Exists($"{resourcesAssetsPath}.resS"));
            var fontsAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/StandaloneWindows64/fonts");
            Debug.Assert(File.Exists(fontsAssetsPath));
            var textsAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/StandaloneWindows64/luabytecode");
            Debug.Assert(File.Exists(textsAssetsPath));

            // 创建临时文件夹
            tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            while (Directory.Exists(tempPath) || File.Exists(tempPath))
            {
                tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            }
            DirectoryInfo di = Directory.CreateDirectory(tempPath);
            di.Attributes |= FileAttributes.Hidden;
            Directory.CreateDirectory($"{tempPath}/Output");

            // 提取嵌入的文件
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] names = asm.GetManifestResourceNames();
            foreach (var name in names)
            {
                Stream stream = asm.GetManifestResourceStream(name);
                string fileName = name.Split('.')[2];
                string fileExtension = name.Split('.').Last();
                if (fileExtension == "bin")
                {
                    BinaryReader br = new BinaryReader(stream);
                    File.WriteAllBytes($"{tempPath}/{fileName}.{fileExtension}", br.ReadBytes((int)br.BaseStream.Length));
                }
                else if (fileExtension == "zip")
                {
                    if (File.Exists("Patch.zip"))
                    {
                        stream.Close();
                        stream = File.OpenRead("Patch.zip");
                    }
                    ZipArchive archive = new ZipArchive(stream);
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string completeFileName = Path.Combine(tempPath, file.FullName);
                        if (file.Name == "")
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                            continue;
                        }
                        else
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(completeFileName)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                            }
                            file.ExtractToFile(completeFileName, true);
                        }
                    }
                }
                stream.Close();
            }

            // resources.assets
            ReplaceAssets(resourcesAssetsPath, tempPath, $"{tempPath}/Output/resources.assets");

            // fonts
            ExtractBundle(fontsAssetsPath, tempPath, $"{tempPath}/Output/fonts");

            // luabytecode
            ExtractBundle(textsAssetsPath, tempPath, $"{tempPath}/Output/luabytecode");

            // 备份并替换
            if (!File.Exists($"{resourcesAssetsPath}.bak"))
            {
                File.Move(resourcesAssetsPath, $"{resourcesAssetsPath}.bak");
            }
            else
            {
                File.Delete(resourcesAssetsPath);
            }
            File.Move($"{tempPath}/Output/resources.assets", resourcesAssetsPath);

            if (!File.Exists($"{resourcesAssetsPath}.resS.bak"))
            {
                File.Move($"{resourcesAssetsPath}.resS", $"{resourcesAssetsPath}.resS.bak");
            }
            else
            {
                File.Delete($"{resourcesAssetsPath}.resS");
            }
            File.Move($"{tempPath}/Output/resources.assets.resS", $"{resourcesAssetsPath}.resS");

            if (!File.Exists($"{fontsAssetsPath}.bak"))
            {
                File.Move(fontsAssetsPath, $"{fontsAssetsPath}.bak");
            }
            else
            {
                File.Delete(fontsAssetsPath);
            }
            File.Move($"{tempPath}/Output/fonts", fontsAssetsPath);

            if (!File.Exists($"{textsAssetsPath}.bak"))
            {
                File.Move(textsAssetsPath, $"{textsAssetsPath}.bak");
            }
            else
            {
                File.Delete(textsAssetsPath);
            }
            File.Move($"{tempPath}/Output/luabytecode", textsAssetsPath);
        }

        static void ReplaceAssets(string assetsPath, string replacePath, string outputPath)
        {
            string assetsName = Path.GetFileName(assetsPath);
            BinaryReaderExtended reader = new BinaryReaderExtended(File.OpenRead(assetsPath));
            Assets assetsData = new Assets(reader, replacePath, assetsName);

            BinaryWriterExtended writer = new BinaryWriterExtended(File.Create(outputPath));
            BinaryReaderExtended resourcesReader = new BinaryReaderExtended(File.OpenRead($"{assetsPath}.resS"));
            BinaryWriterExtended resourcesWriter = new BinaryWriterExtended(File.Create($"{outputPath}.resS"));

            assetsData.DumpRaw(writer, resourcesReader, resourcesWriter);

            reader.Close();
            writer.Close();
            resourcesReader.Close();
            resourcesWriter.Close();
        }

        static void ExtractBundle(string bundlePath, string replacePath, string outputPath)
        {
            BinaryReaderExtended reader = new BinaryReaderExtended(File.OpenRead(bundlePath));
            Bundle bundleData = new Bundle(reader);
            Debug.Assert(bundleData.FileList.Length == 1 || bundleData.FileList.Length == 2);

            Bundle.StreamFile assetsFile, resourcesFile;
            MemoryStream assetsStream = new MemoryStream(), resourcesStream = new MemoryStream();
            Stream[] replaceStreams;
            if (bundleData.FileList[0].fileName.Contains(".resS"))
            {
                resourcesFile = bundleData.FileList[0];
                assetsFile = bundleData.FileList[1];
                replaceStreams = new Stream[]
                {
                    resourcesStream,
                    assetsStream
                };
            }
            else
            {
                assetsFile = bundleData.FileList[0];
                if (bundleData.FileList.Length == 2)
                {
                    resourcesFile = bundleData.FileList[1];
                }
                else
                {
                    resourcesFile = new Bundle.StreamFile { stream = new MemoryStream() };
                }
                replaceStreams = new Stream[]
                {
                    assetsStream,
                    resourcesStream
                };
            }
            BinaryReaderExtended assetsReader = new BinaryReaderExtended(assetsFile.stream);
            Assets assetsData = new Assets(assetsReader, replacePath, assetsFile.fileName);
            BinaryWriterExtended assetsWriter = new BinaryWriterExtended(assetsStream);
            BinaryReaderExtended resourcesReader = new BinaryReaderExtended(resourcesFile.stream);
            BinaryWriterExtended resourcesWriter = new BinaryWriterExtended(resourcesStream);
            assetsData.DumpRaw(assetsWriter, resourcesReader, resourcesWriter);

            BinaryWriterExtended writer = new BinaryWriterExtended(File.Create(outputPath));
            bundleData.DumpRaw(writer, replaceStreams);

            reader.Close();
            writer.Close();
            assetsReader.Close();
            assetsWriter.Close();
            resourcesReader.Close();
            resourcesWriter.Close();
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            string[] names = asm.GetManifestResourceNames();
            byte[] bytes;
            foreach (var name in names)
            {
                if (name.Contains(dllName))
                {
                    Stream stream = asm.GetManifestResourceStream(name);
                    bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    stream.Close();
                    return Assembly.Load(bytes);
                }
            }
            return null;
        }
    }
}
