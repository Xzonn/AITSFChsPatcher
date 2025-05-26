using UnityAsset;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Linq;

namespace AITheSomniumFilesChsPatch
{
    public static class PatchHelper
    {
        private static readonly string[] BUNDLE_FILE_NAMES =
        [
            "fonts",
            "image_name_zh_tw",
            "luabytecode",
            "scene_autosaveguide",
            "scene_dance",
            "scene_fiction",
            "scene_file",
            "scene_flowchart",
            "scene_investigation",
            "scene_languageselect",
            "scene_options",
            "scene_somnium",
        ];

        public static void ApplyPatch(string basePath, out string tempPath)
        {
            // 检查文件是否存在
            Debug.Assert(Directory.Exists(basePath));
            Debug.Assert(File.Exists(Path.Combine(basePath, "AI_TheSomniumFiles.exe")));
            var resourcesAssetsPath = Path.Combine(basePath, "AI_TheSomniumFiles_Data/resources.assets");
            Debug.Assert(File.Exists(resourcesAssetsPath));
            Debug.Assert(File.Exists($"{resourcesAssetsPath}.resS"));
            foreach (var fileName in BUNDLE_FILE_NAMES)
            {
                var bundlePath = Path.Combine(basePath, $"AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/StandaloneWindows64/{fileName}");
                Debug.Assert(File.Exists(bundlePath), $"缺少所需的文件：{bundlePath}");
            }

            // 创建临时文件夹
            tempPath = $"{Path.GetTempPath()}/temp_{DateTime.Now.GetHashCode():X8}";
            while (Directory.Exists(tempPath) || File.Exists(tempPath))
            {
                tempPath = $"{Path.GetTempPath()}/temp_{DateTime.Now.GetHashCode():X8}";
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
                    BinaryReader br = new(stream);
                    File.WriteAllBytes($"{tempPath}/{fileName}.{fileExtension}", br.ReadBytes((int)br.BaseStream.Length));
                }
                else if (fileExtension == "zip")
                {
                    if (File.Exists("Patch.zip"))
                    {
                        stream.Close();
                        stream = File.OpenRead("Patch.zip");
                    }
                    ZipArchive archive = new(stream);
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

            // AssetBundles
            foreach (var fileName in BUNDLE_FILE_NAMES)
            {
                string bundlePath = Path.Combine(basePath, $"AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/StandaloneWindows64/{fileName}");
                string outputPath = $"{tempPath}/Output/{fileName}";
                ExtractBundle(bundlePath, tempPath, outputPath);

                if (!File.Exists($"{bundlePath}.bak"))
                {
                    File.Move(bundlePath, $"{bundlePath}.bak");
                }
                else
                {
                    File.Delete(bundlePath);
                }
                File.Move($"{tempPath}/Output/{fileName}", bundlePath);
            }

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
        }

        static void ReplaceAssets(string assetsPath, string replacePath, string outputPath)
        {
            string assetsName = Path.GetFileName(assetsPath);
            using BinaryReaderExtended reader = new(File.OpenRead(assetsPath));
            Assets assetsData = new(reader, replacePath, assetsName);

            using BinaryWriterExtended writer = new(File.Create(outputPath));
            using BinaryReaderExtended resourcesReader = new(File.OpenRead($"{assetsPath}.resS"));
            using BinaryWriterExtended resourcesWriter = new(File.Create($"{outputPath}.resS"));

            assetsData.DumpRaw(writer, resourcesReader, resourcesWriter);
        }

        static void ExtractBundle(string bundlePath, string replacePath, string outputPath)
        {
            using BinaryReaderExtended reader = new(File.OpenRead(bundlePath));
            Bundle bundleData = new(reader);
            Debug.Assert(bundleData.FileList.Length == 1 || bundleData.FileList.Length == 2);

            Bundle.StreamFile assetsFile, resourcesFile;
            MemoryStream assetsStream = new(), resourcesStream = new();
            Stream[] replaceStreams;
            if (bundleData.FileList[0].fileName.Contains(".resS"))
            {
                resourcesFile = bundleData.FileList[0];
                assetsFile = bundleData.FileList[1];
                replaceStreams =
                [
                    resourcesStream,
                    assetsStream,
                ];
            }
            else
            {
                assetsFile = bundleData.FileList[0];
                if (bundleData.FileList.Length == 2 && bundleData.FileList[1].fileName.Contains(".resS"))
                {
                    resourcesFile = bundleData.FileList[1];
                    replaceStreams =
                    [
                        assetsStream,
                        resourcesStream,
                    ];
                }
                else
                {
                    resourcesFile = new Bundle.StreamFile { stream = new MemoryStream() };
                    replaceStreams =
                    [
                        assetsStream,
                    ];
                }
            }
            using BinaryReaderExtended assetsReader = new(assetsFile.stream);
            Assets assetsData = new(assetsReader, replacePath, assetsFile.fileName);
            using BinaryWriterExtended assetsWriter = new(assetsStream);
            using BinaryReaderExtended resourcesReader = new(resourcesFile.stream);
            using BinaryWriterExtended resourcesWriter = new(resourcesStream);
            assetsData.DumpRaw(assetsWriter, resourcesReader, resourcesWriter);

            using BinaryWriterExtended writer = new(File.Create(outputPath));
            bundleData.DumpRaw(writer, replaceStreams);
        }

        public static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
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
