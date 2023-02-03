using Backup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BackupTests
{
    public class BackupAdvancedTests
    {
        [Fact]
        public async Task BackupShouldWorkWithMultipleInputFiles()
        {
            var src = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}Src";
            var srcDirA = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcDirA";
            var srcDirB = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcDirB";
            var srcDirC = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcDirC";
            var desc = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}Desc";
            var srcFileA = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcFileA";
            var srcFileB = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcFileB";
            var srcFileC = $"{nameof(BackupShouldWorkWithMultipleInputFiles)}SrcFileC";

            FileHelpers.ClearDirectories(desc);
            FileHelpers.CreateTestDirectory(srcDirA);
            FileHelpers.CreateTestDirectory(srcDirB);
            FileHelpers.CreateTestDirectory(srcDirC);

            FileHelpers.CreateFile(srcFileA, "TestA");
            FileHelpers.CreateFile(srcFileB, "TestB");
            FileHelpers.CreateFile(srcFileC, "TestC");

            var backup = BackupHelper.Standard;
            await backup.CreateBackup(desc, srcDirA, srcDirB, srcFileA, srcFileB);
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcDirA);
            FileHelpers.Assert($"{desc}/{srcDirA}");
            FileHelpers.Assert(srcDirB);
            FileHelpers.Assert($"{desc}/{srcDirB}");
            FileHelpers.Assert(srcDirC);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcDirA, false);
            FileHelpers.AssertGuardFile(srcDirB, false);
            FileHelpers.AssertGuardFile(srcDirC, false);

            FileHelpers.AssertDirectoryNotExists($"{desc}/{srcDirC}");
            FileHelpers.AssertFileExists(srcFileA, "TestA");
            FileHelpers.AssertFileExists(srcFileB, "TestB");
            FileHelpers.AssertFileExists(srcFileC, "TestC");

            FileHelpers.AssertFileExists($"{desc}/{srcFileA}", "TestA");
            FileHelpers.AssertFileExists($"{desc}/{srcFileB}", "TestB");
            FileHelpers.AssertFileNotExists($"{desc}/{srcFileC}");

            FileHelpers.CreateFile(srcFileA, "TestA - modification");
            await backup.CreateBackup(desc, srcDirA, srcDirC, srcFileA, srcFileC);
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcDirA);
            FileHelpers.Assert($"{desc}/{srcDirA}");
            FileHelpers.Assert(srcDirB);
            FileHelpers.Assert($"{desc}/{srcDirC}");
            FileHelpers.Assert(srcDirC);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcDirA, false);
            FileHelpers.AssertGuardFile(srcDirB, false);
            FileHelpers.AssertGuardFile(srcDirC, false);

            FileHelpers.AssertDirectoryNotExists($"{desc}/{srcDirB}");
            FileHelpers.AssertFileExists(srcFileA, "TestA - modification");
            FileHelpers.AssertFileExists(srcFileB, "TestB");
            FileHelpers.AssertFileExists(srcFileC, "TestC");

            FileHelpers.AssertFileExists($"{desc}/{srcFileA}", "TestA - modification");
            FileHelpers.AssertFileNotExists($"{desc}/{srcFileB}");
            FileHelpers.AssertFileExists($"{desc}/{srcFileC}", "TestC");

            FileHelpers.ClearDirectories(desc, srcDirA, srcDirB, srcDirC);
            FileHelpers.ClearFiles(srcFileA, srcFileB, srcFileC);
        }

        [Fact]
        public async Task BackupShouldWorkWithEmptyFilesAndDirectories()
        {
            var srcA = $"{nameof(BackupShouldWorkWithEmptyFilesAndDirectories)}SrcA";
            var srcB = $"{nameof(BackupShouldWorkWithEmptyFilesAndDirectories)}SrcB";
            var desc = $"{nameof(BackupShouldWorkWithEmptyFilesAndDirectories)}Desc";

            FileHelpers.ClearDirectories(desc);
            FileHelpers.CreateFile(srcA, "");
            FileHelpers.CreateDirectory(srcB);

            var backup = BackupHelper.Standard;
            await backup.CreateBackup(desc, srcA, srcB);
            backup.AssertDirectoryNotChanged();

            FileHelpers.AssertFileExists(srcA, "");
            FileHelpers.AssertDirectoryExists(srcB);
            FileHelpers.AssertFileExists($"{desc}/{srcA}", "");
            FileHelpers.AssertDirectoryExists($"{desc}/{srcB}");
            FileHelpers.AssertGuardFile(desc, true);

            FileHelpers.CreateFile($"{desc}/{srcA}", "Content to be deleted");

            await backup.CreateBackup(desc, srcA, srcB);
            backup.AssertDirectoryNotChanged();

            FileHelpers.AssertFileExists(srcA, "");
            FileHelpers.AssertDirectoryExists(srcB);
            FileHelpers.AssertFileExists($"{desc}/{srcA}", "");
            FileHelpers.AssertDirectoryExists($"{desc}/{srcB}");
            FileHelpers.AssertGuardFile(desc, true);

            FileHelpers.ClearDirectories(srcB, desc);
            FileHelpers.ClearFiles(srcA);

        }
        [Theory]
        [InlineData("10M")]
        [InlineData("10k")]
        [InlineData("10m")]
        [InlineData("512M")]
        [InlineData("1g")]
        public async Task BackupShouldWorkWithLargeFiles(string bufferSize)
        {
            var srcA = $"{nameof(BackupShouldWorkWithLargeFiles)}SrcA_{bufferSize}";
            var srcB = $"{nameof(BackupShouldWorkWithLargeFiles)}SrcB_{bufferSize}";
            var desc = $"{nameof(BackupShouldWorkWithLargeFiles)}Desc_{bufferSize}";

            var content = new Dictionary<string, string>
            {
                { "fileA", new string('a', 20 * 1024 * 1024) },
                { "fileB", new string('b', 30 * 1024 * 1024) },
                { "fileC", new string('c', 25 * 1024 * 1024) },
                { "fileD", new string('d', 10 * 1024 * 1024) },
            };

            FileHelpers.ClearDirectories(desc);
            FileHelpers.CreateTestDirectory(srcA, content: content);
            FileHelpers.CreateFile(srcB, content["fileD"]);

            var backup = BackupHelper.Standard;
            backup.BufferSize = bufferSize;
            await backup.CreateBackup(desc, srcA, srcB);
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcA, content);
            FileHelpers.Assert($"{desc}/{srcA}", content);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcA, false);

            FileHelpers.AssertFileExists(srcB, content["fileD"]);
            FileHelpers.AssertFileExists($"{desc}/{srcB}", content["fileD"]);

            content = new Dictionary<string, string>
            {
                { "fileA", new string('e', 20 * 1024 * 1024) },
                { "fileB", new string('f', 30 * 1024 * 1024) },
                { "fileC", new string('g', 25 * 1024 * 1024) },
                { "fileD", new string('h', 10 * 1024 * 1024) },
            };

            FileHelpers.CreateTestDirectory(srcA, content: content);
            FileHelpers.CreateFile(srcB, content["fileD"]);

            await backup.CreateBackup(desc, srcA, srcB);
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcA, content);
            FileHelpers.Assert($"{desc}/{srcA}", content);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcA, false);

            FileHelpers.AssertFileExists(srcB, content["fileD"]);
            FileHelpers.AssertFileExists($"{desc}/{srcB}", content["fileD"]);

            FileHelpers.ClearDirectories(srcA, desc);
            FileHelpers.ClearFiles(srcB);
        }
        [Fact]
        public async Task BackupShouldWorkWithDirectoryPathEndedWithSlash()
        {
            var src = $"{nameof(BackupShouldWorkWithDirectoryPathEndedWithSlash)}Src";
            var desc = $"{nameof(BackupShouldWorkWithDirectoryPathEndedWithSlash)}Desc";

            FileHelpers.ClearDirectories(desc);
            FileHelpers.CreateTestDirectory(src);

            var backup = BackupHelper.Standard;
            await backup.CreateBackup($"{desc}/", $"{src}/");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}/");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            var content = new Dictionary<string, string>
            {
                { "fileA", "File A content" },
                { "fileB", "File B content" },
                { "fileC", "File C content" },
            };

            FileHelpers.CreateTestDirectory(src, content: content);

            await backup.CreateBackup($"{desc}/", $"{src}/");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(src, content);
            FileHelpers.Assert($"{desc}/{src}", content);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            FileHelpers.ClearDirectories(src, desc);
        }

        [Fact]
        public async Task BackupShouldWorkWithDnsAddresses()
        {
            var src = $"{nameof(BackupShouldWorkWithDnsAddresses)}Src";
            var desc = $"{nameof(BackupShouldWorkWithDnsAddresses)}Desc";

            FileHelpers.ClearDirectories(desc);
            FileHelpers.CreateTestDirectory(src);

            var backup = BackupHelper.Standard;
            backup.Address = "localhost";
            await backup.CreateBackup(desc, $"{src}");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            var content = new Dictionary<string, string>
            {
                { "fileA", "File A content" },
                { "fileB", "File B content" },
                { "fileC", "File C content" },
            };

            FileHelpers.CreateTestDirectory(src, content: content);

            await backup.CreateBackup(desc, $"{src}");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(src, content);
            FileHelpers.Assert($"{desc}/{src}", content);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            FileHelpers.ClearDirectories(src, desc);
        }

        [Fact]
        public async Task BackupShouldWorkWithAliases()
        {
            var srcA = $"{nameof(BackupShouldWorkWithAliases)}SrcA";
            var srcB = $"{nameof(BackupShouldWorkWithAliases)}SrcB";
            var srcAliasA = $"{nameof(BackupShouldWorkWithAliases)}AliasA";
            var srcAliasB = $"{nameof(BackupShouldWorkWithAliases)}AliasB";
            var desc = $"{nameof(BackupShouldWorkWithAliases)}Desc";

            FileHelpers.ClearDirectories(srcAliasB, desc);
            FileHelpers.ClearFiles(srcAliasB);

            FileHelpers.CreateTestDirectory(srcA);
            FileHelpers.CreateFile(srcB, "First test");

            var backup = BackupHelper.Standard;
            await backup.CreateBackup(desc, $"{srcA}*{srcAliasA}", $"{srcB}*{srcAliasB}");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcA);
            FileHelpers.Assert($"{desc}/{srcAliasA}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcAliasA, false);

            FileHelpers.AssertFileExists(srcB, "First test");
            FileHelpers.AssertFileExists($"{desc}/{srcAliasB}", "First test");

            var content = new Dictionary<string, string>
            {
                { "fileA", "File A content" },
                { "fileB", "File B content" },
                { "fileC", "File C content" },
            };

            FileHelpers.CreateTestDirectory(srcA, content: content);
            FileHelpers.CreateFile(srcB, "Second test");

            await backup.CreateBackup(desc, $"{srcA}*{srcAliasA}", $"{srcB}*{srcAliasB}");
            backup.AssertDirectoryNotChanged();

            FileHelpers.Assert(srcA, content);
            FileHelpers.Assert($"{desc}/{srcAliasA}", content);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcAliasA, false);
            FileHelpers.AssertFileExists(srcB, "Second test");
            FileHelpers.AssertFileExists($"{desc}/{srcAliasB}", "Second test");

            FileHelpers.ClearDirectories(srcA, desc);
            FileHelpers.ClearFiles(srcB);
            FileHelpers.ClearFiles(srcAliasA);
        }
    }
}
