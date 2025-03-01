using BackupCore;
using Common;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BackupTests
{
    internal static class FileHelpers
    {
        public static string TestFilesDirectory = "TestFiles";
        public static void ClearDirectories(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists($"{TestFilesDirectory}/{path}"))
                    Directory.Delete($"{TestFilesDirectory}/{path}", true);
            }
        }

        public static void ClearFiles(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists($"{TestFilesDirectory}/{path}"))
                    File.Delete($"{TestFilesDirectory}/{path}");
            }
        }
        /*
         * {path}
         *  - fileA
         *  - directoryA
         *      - fileB
         *      -directoryB
         *          - fileC 
         */
        public static void CreateTestDirectory(string path, string postfix = "", Dictionary<string, string> content = null)
        {
            var originalPath = path;
            ClearDirectories(path);
            if (!string.IsNullOrEmpty(postfix))
                path = $"{path}/{postfix}";
            content = content ?? new Dictionary<string, string>();
            CreateDirectory(path);
            CreateDirectory($"{path}/directoryA");
            CreateDirectory($"{path}/directoryA/directoryB");
            CreateFileWithText($"{path}/fileA", content);
            CreateFileWithText($"{path}/directoryA/fileB", content);
            CreateFileWithText($"{path}/directoryA/directoryB/fileC", content);
            if (!string.IsNullOrEmpty(postfix))
            {
                RefreshGuardFile(originalPath);
            }

        }
        /*
         * desc
         * src
         *  - srcA/
         *  - srcB
         *  - srcC
         *  - srcD
         */
        public static void CreateDirecotriesForIgnoreTests(string src, string srcA, string srcB, string srcC, string srcD, string firstFileContent, string secondFileContent)
        {
            ClearDirectories(src);
            CreateDirectory(src);
            CreateDirectory($"{src}/{srcA}");
            CreateDirectory($"{src}/{srcB}");
            CreateFile($"{src}/{srcC}", firstFileContent);
            CreateFile($"{src}/{srcD}", secondFileContent);
        }

        public static void CreateFile(string path, string content)
        {
            ClearFiles(path);
            File.Create($"{TestFilesDirectory}/{path}").Dispose();
            File.WriteAllText($"{TestFilesDirectory}/{path}", content);
        }
        public static void HideFile(string path)
        {
            File.SetAttributes($"{TestFilesDirectory}/{path}", FileAttributes.Hidden);
        }
        public static void AssertFileIsHidden(string path)
        {
            var attributes = File.GetAttributes($"{TestFilesDirectory}/{path}");
            (attributes & FileAttributes.Hidden).ShouldBe(FileAttributes.Hidden);
        }
        public static void AssertFileIsVisible(string path)
        {
            var attributes = File.GetAttributes($"{TestFilesDirectory}/{path}");
            ((int)(attributes & FileAttributes.Hidden)).ShouldBe(0);
        }
        public static void CreateDirectory(string path)
        {
            ClearDirectories(path);
            Directory.CreateDirectory($"{TestFilesDirectory}/{path}");
        }
        public static void AssertDirectoryExists(string path)
        {
            Directory.Exists($"{TestFilesDirectory}/{path}").ShouldBeTrue();
        }
        public static void AssertFileExists(string path, string content)
        {
            File.Exists($"{TestFilesDirectory}/{path}").ShouldBeTrue();
            File.ReadAllText($"{TestFilesDirectory}/{path}").ShouldBe(content);
        }

        public static void AssertDirectoryNotExists(string path)
        {
            Directory.Exists($"{TestFilesDirectory}/{path}").ShouldBeFalse();
        }
        public static void AssertFileNotExists(string path)
        {
            File.Exists($"{TestFilesDirectory}/{path}").ShouldBeFalse();
        }

        public static void Assert(string path, Dictionary<string, string> content = null)
        {
            content = content ?? new Dictionary<string, string>();
            AssertDirectoryExists(path);
            AssertDirectoryExists($"{path}/directoryA");
            AssertDirectoryExists($"{path}/directoryA/directoryB");
            AssertFileExists($"{path}/fileA", content);
            AssertFileExists($"{path}/directoryA/fileB", content);
            AssertFileExists($"{path}/directoryA/directoryB/fileC", content);
        }

        public static void RefreshGuardFile(string path, params string[] ignoreFilesRegex)
        {
            FilesystemModel.Directory.PREFIX_GROSS = "|   ";
            var fileFactory = new FilesystemModel.FileFactory(ignoreFilesRegex.Select(x => new Regex(x)).ToList());
            BackupDestination.CreateBackupDirectoryGuardFile(new FilesystemModel.Directory(fileFactory, $"{TestFilesDirectory}/{path}", false));
        }

        private static void CreateFileWithText(string path, Dictionary<string, string> content)
        {
            string name = path.Split('/').Last();
            if (!content.TryGetValue(name, out string text))
                text = GetFileContent(name);

            CreateFile(path, text);
        }

        private static void AssertFileExists(string path, Dictionary<string, string> content)
        {
            string name = path.Split('/').Last();
            if (!content.TryGetValue(name, out string text))
                text = GetFileContent(name);
            AssertFileExists(path, text);
        }

        public static void AssertGuardFile(string path, bool exists)
        {
            File.Exists($"{TestFilesDirectory}/{path}/{Consts.BackupDirectoryGuardFilePath}").ShouldBe(exists);
        }

        private static string GetFileContent(string name) => $"Test 123 test ABC test abc - {name}";

        public static DateTime GetModificationDate(string path) => File.GetCreationTimeUtc($"{TestFilesDirectory}/{path}");
    }
}
