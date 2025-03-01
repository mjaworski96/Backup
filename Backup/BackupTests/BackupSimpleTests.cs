using BackupCore;
using Common;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BackupTests
{
    public class BackupSimpleTests
    {
        [Fact]
        public async Task BackupShouldCreateBackup()
        {
            var src = $"{nameof(BackupShouldCreateBackup)}Src";
            var desc = $"{nameof(BackupShouldCreateBackup)}Desc";
            FileHelpers.CreateTestDirectory(src);
            FileHelpers.ClearDirectories(desc);

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldNotModifyBackupWhenNoChangesWasMade()
        {
            var src = $"{nameof(BackupShouldNotModifyBackupWhenNoChangesWasMade)}Src";
            var desc = $"{nameof(BackupShouldNotModifyBackupWhenNoChangesWasMade)}Desc";
            FileHelpers.CreateTestDirectory(src);
            FileHelpers.CreateTestDirectory(desc, src);

            var srcFileAModificationTime = FileHelpers.GetModificationDate($"{src}/fileA");
            var srcFileBModificationTime = FileHelpers.GetModificationDate($"{src}/directoryA/fileB");
            var srcFileCModificationTime = FileHelpers.GetModificationDate($"{src}/directoryA/directoryB/fileC");
            var descFileAModificationTime = FileHelpers.GetModificationDate($"{desc}/{src}/fileA");
            var descFileBModificationTime = FileHelpers.GetModificationDate($"{desc}/{src}/directoryA/fileB");
            var descFileCModificationTime = FileHelpers.GetModificationDate($"{desc}/{src}/directoryA/directoryB/fileC");

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);

            srcFileAModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{src}/fileA"));
            srcFileBModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{src}/directoryA/fileB"));
            srcFileCModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{src}/directoryA/directoryB/fileC"));
            descFileAModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{desc}/{src}/fileA"));
            descFileBModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{desc}/{src}/directoryA/fileB"));
            descFileCModificationTime.ShouldBe(FileHelpers.GetModificationDate($"{desc}/{src}/directoryA/directoryB/fileC"));

            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldModifyBackupWhenChangesWasMade()
        {
            var src = $"{nameof(BackupShouldModifyBackupWhenChangesWasMade)}Src";
            var desc = $"{nameof(BackupShouldModifyBackupWhenChangesWasMade)}Desc";

            var filesContent = new Dictionary<string, string>
            {
                {"fileA", "Test" },
                {"fileB", "Test Test Test" },
                {"fileC", "Test Test" }
            };

            FileHelpers.CreateTestDirectory(src, content: filesContent);
            FileHelpers.CreateTestDirectory(desc, src);

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);


            FileHelpers.Assert(src, filesContent);
            FileHelpers.Assert($"{desc}/{src}", filesContent);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldCreateFileAndDirectoryWhenNewFileAndDirectoryWasAdded()
        {
            var src = $"{nameof(BackupShouldCreateFileAndDirectoryWhenNewFileAndDirectoryWasAdded)}Src";
            var desc = $"{nameof(BackupShouldCreateFileAndDirectoryWhenNewFileAndDirectoryWasAdded)}Desc";
            var fileContent = "Test ABC Test 123 abc";
            FileHelpers.CreateTestDirectory(src);
            FileHelpers.CreateFile($"{src}/directoryA/newFile", fileContent);
            FileHelpers.CreateDirectory($"{src}/directoryNew");
            FileHelpers.CreateTestDirectory(desc, src);

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.AssertFileExists($"{src}/directoryA/newFile", fileContent);
            FileHelpers.AssertFileExists($"{desc}/{src}/directoryA/newFile", fileContent);
            FileHelpers.AssertDirectoryExists($"{src}/directoryNew");
            FileHelpers.AssertDirectoryExists($"{desc}/{src}/directoryNew");

            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldDeleteFileAndDirectoryWhenNewFileAndDirectoryWasDeleted()
        {
            var src = $"{nameof(BackupShouldDeleteFileAndDirectoryWhenNewFileAndDirectoryWasDeleted)}Src";
            var desc = $"{nameof(BackupShouldDeleteFileAndDirectoryWhenNewFileAndDirectoryWasDeleted)}Desc";

            FileHelpers.CreateTestDirectory(src);
            FileHelpers.CreateTestDirectory(desc, src);
            FileHelpers.CreateFile($"{desc}/{src}/directoryA/oldFile", "File to be deleted");
            FileHelpers.CreateDirectory($"{desc}/{src}/directoryOld");
            FileHelpers.RefreshGuardFile(desc);

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.Assert(src);
            FileHelpers.Assert($"{desc}/{src}");
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.AssertFileNotExists($"{src}/directoryA/oldFile");
            FileHelpers.AssertDirectoryNotExists($"{src}/directoryOld");
            FileHelpers.AssertFileNotExists($"{desc}/{src}/directoryA/oldFile");
            FileHelpers.AssertDirectoryNotExists($"{desc}/{src}/directoryOld");

            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldDetectIfDirectoryWasNotUsedAsDestinationDirectory()
        {
            var src = $"{nameof(BackupShouldDetectIfDirectoryWasNotUsedAsDestinationDirectory)}Src";
            var desc = $"{nameof(BackupShouldDetectIfDirectoryWasNotUsedAsDestinationDirectory)}Desc";

            var filesContent = new Dictionary<string, string>
            {
                {"fileA", "Test" },
                {"fileB", "Test Test Test" },
                {"fileC", "Test Test" }
            };

            FileHelpers.CreateTestDirectory(src, content: filesContent);
            FileHelpers.CreateTestDirectory(desc, src);
            FileHelpers.CreateFile($"{desc}/{src}/newFile", "");

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryChanged();
            backup.AssertErrorsCount(1);

            FileHelpers.Assert(src, filesContent);
            FileHelpers.Assert($"{desc}/{src}", filesContent);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.AssertDirectoryNotExists($"{desc}/{src}/newFile");
            FileHelpers.AssertDirectoryNotExists($"{src}/newFile");
            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldDetectIfDestinationDirectoryWasModified()
        {
            var src = $"{nameof(BackupShouldDetectIfDestinationDirectoryWasModified)}Src";
            var desc = $"{nameof(BackupShouldDetectIfDestinationDirectoryWasModified)}Desc";

            var filesContent = new Dictionary<string, string>
            {
                {"fileA", "Test" },
                {"fileB", "Test Test Test" },
                {"fileC", "Test Test" }
            };

            FileHelpers.CreateTestDirectory(src, content: filesContent);
            FileHelpers.CreateTestDirectory(desc, src);
            FileHelpers.ClearFiles($"{desc}/{Consts.BackupDirectoryGuardFilePath}");

            var backup = BackupHelper.Default;
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryChanged();
            backup.AssertErrorsCount(1);

            FileHelpers.Assert(src, filesContent);
            FileHelpers.Assert($"{desc}/{src}", filesContent);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.ClearDirectories(desc, src);
        }

        [Fact]
        public async Task BackupShouldNotCopyIgnoredFiles()
        {
            var srcRoot = $"{nameof(BackupShouldNotCopyIgnoredFiles)}Src";
            var srcA = $"{nameof(BackupShouldNotCopyIgnoredFiles)}SrcA";
            var srcB = $"{nameof(BackupShouldNotCopyIgnoredFiles)}SrcB";
            var srcC = $"{nameof(BackupShouldNotCopyIgnoredFiles)}SrcC";
            var srcD = $"{nameof(BackupShouldNotCopyIgnoredFiles)}SrcD";
            var desc = $"{nameof(BackupShouldNotCopyIgnoredFiles)}Desc";

            var firstContent = "Test content";
            var secondContent = "Test content 2";

            FileHelpers.CreateDirecotriesForIgnoreTests(srcRoot, srcA, srcB, srcC, srcD, firstContent, secondContent);
            FileHelpers.ClearDirectories(desc);

            var backup = BackupHelper.Default;
            backup.SourceIgnorePatterns = new[] { $"/{srcB}", $"/{srcD}" };

            await backup.CreateBackup(desc, srcRoot);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{srcRoot}/{srcC}", firstContent);
            FileHelpers.AssertFileExists($"{srcRoot}/{srcD}", secondContent);
            FileHelpers.AssertDirectoryExists(desc);
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryNotExists($"{desc}/{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcC}", firstContent);
            FileHelpers.AssertFileNotExists($"{desc}/{srcRoot}/{srcD}");

            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcRoot, false);
            FileHelpers.ClearDirectories(srcRoot, desc);
            FileHelpers.ClearFiles(srcC, srcD);
        }

        [Fact]
        public async Task BackupShouldDeleteFilesIfWasIgnoredBySource()
        {
            var srcRoot = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}Src";
            var srcA = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}SrcA";
            var srcB = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}SrcB";
            var srcC = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}SrcC";
            var srcD = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}SrcD";
            var desc = $"{nameof(BackupShouldDeleteFilesIfWasIgnoredBySource)}Desc";

            var firstContentSrc = "Test content modify";
            var secondContentSrc = "Test content 2 modify";
            var firstContentDesc = "Test content";
            var secondContentDesc = "Test content 2";

            FileHelpers.CreateDirecotriesForIgnoreTests(srcRoot, srcA, srcB, srcC, srcD, firstContentSrc, secondContentSrc);
            FileHelpers.CreateDirecotriesForIgnoreTests($"{desc}/{srcRoot}", srcA, srcB, srcC, srcD, firstContentDesc, secondContentDesc);
            

            var backup = BackupHelper.Default;
            backup.SourceIgnorePatterns = new[] { $"/{srcB}", $"/{srcD}" };
            FileHelpers.RefreshGuardFile(desc);

            await backup.CreateBackup(desc, srcRoot);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{srcRoot}/{srcC}", firstContentSrc);
            FileHelpers.AssertFileExists($"{srcRoot}/{srcD}", secondContentSrc);
            FileHelpers.AssertDirectoryExists(desc);
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryNotExists($"{desc}/{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcC}", firstContentSrc);
            FileHelpers.AssertFileNotExists($"{desc}/{srcRoot}/{srcD}");

            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcRoot, false);

            FileHelpers.ClearDirectories(srcRoot, desc);
            FileHelpers.ClearFiles(srcC, srcD);
        }

        [Fact]
        public async Task BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination()
        {
            var srcRoot = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}Src";
            var srcA = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}SrcA";
            var srcB = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}SrcB";
            var srcC = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}SrcC";
            var srcD = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}SrcD";
            var desc = $"{nameof(BackupShouldCopyFilesEvenIfTheyAreAddedInIgnoreInDestination)}Desc";

            var firstContent = "Test content";
            var secondContent = "Test content 2";

            FileHelpers.CreateDirecotriesForIgnoreTests(srcRoot, srcA, srcB, srcC, srcD, firstContent, secondContent);
            FileHelpers.ClearDirectories(desc);

            var backup = BackupHelper.Default;
            backup.DestinationIgnorePatterns = new[] { $"/{srcB}", $"/{srcD}" };

            await backup.CreateBackup(desc, srcRoot);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{srcRoot}/{srcC}", firstContent);
            FileHelpers.AssertFileExists($"{srcRoot}/{srcD}", secondContent);
            FileHelpers.AssertDirectoryExists(desc);
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcC}", firstContent);
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcD}", secondContent);

            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcRoot, false);

            FileHelpers.ClearDirectories(srcRoot, desc);
            FileHelpers.ClearFiles(srcC, srcD);
        }

        [Fact]
        public async Task BackupShouldNotDeleteFilesIfWasIgnoredByDestination()
        {
            var srcRoot = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}Src";
            var srcA = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}SrcA";
            var srcB = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}SrcB";
            var srcC = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}SrcC";
            var srcD = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}SrcD";
            var desc = $"{nameof(BackupShouldNotDeleteFilesIfWasIgnoredByDestination)}Desc";

            var firstContentSrc = "Test content modify";
            var secondContentSrc = "Test content 2 modify";
            var firstContentDesc = "Test content";
            var secondContentDesc = "Test content 2";

            FileHelpers.CreateDirecotriesForIgnoreTests(srcRoot, srcA, srcB, srcC, srcD, firstContentSrc, secondContentSrc);
            FileHelpers.CreateDirecotriesForIgnoreTests($"{desc}/{srcRoot}", srcA, srcB, srcC, srcD, firstContentDesc, secondContentDesc);
            FileHelpers.ClearDirectories($"{srcRoot}/{srcB}");
            FileHelpers.ClearFiles($"{srcRoot}/{srcD}");

            var backup = BackupHelper.Default;
            backup.DestinationIgnorePatterns = new[] { $"/{srcB}", $"/{srcD}" };
            FileHelpers.RefreshGuardFile(desc, backup.DestinationIgnorePatterns.ToArray());

            await backup.CreateBackup(desc, srcRoot);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.AssertDirectoryExists($"{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryNotExists($"{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{srcRoot}/{srcC}", firstContentSrc);
            FileHelpers.AssertFileNotExists($"{srcRoot}/{srcD}");
            FileHelpers.AssertDirectoryExists(desc);
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcA}");
            FileHelpers.AssertDirectoryExists($"{desc}/{srcRoot}/{srcB}");
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcC}", firstContentSrc);
            FileHelpers.AssertFileExists($"{desc}/{srcRoot}/{srcD}", secondContentDesc);

            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(srcRoot, false);

            FileHelpers.ClearDirectories(srcRoot, desc);
            FileHelpers.ClearFiles(srcC, srcD);
        }

        [Fact]
        public async Task BackupShouldHandleCompareLargerFileBySizeParameter()
        {
            var src = $"{nameof(BackupShouldHandleCompareLargerFileBySizeParameter)}Src";
            var desc = $"{nameof(BackupShouldHandleCompareLargerFileBySizeParameter)}Desc";

            var srcFilesContent = new Dictionary<string, string>
            {
                { "fileA", new string('a', 20 * 1024 * 1024) },
                { "fileB", new string('b', 30 * 1024 * 1024) },
                { "fileC", new string('c', 25 * 1024 * 1024) },
                { "fileD", new string('d', 30 * 1024 * 1024) },
                { "fileE", new string('e', 35 * 1024 * 1024) },
            };

            var descFilesContent = new Dictionary<string, string>
            {
                { "fileA", new string('f', 20 * 1024 * 1024) },
                { "fileB", new string('g', 30 * 1024 * 1024) },
                { "fileC", new string('h', 25 * 1024 * 1024) },
                { "fileD", new string('i', 35 * 1024 * 1024) },
                { "fileE", new string('j', 30 * 1024 * 1024) },
            };

            var expectedFilesContent = new Dictionary<string, string>
            {
                { "fileA", new string('a', 20 * 1024 * 1024) },
                { "fileB", new string('g', 30 * 1024 * 1024) },
                { "fileC", new string('c', 25 * 1024 * 1024) },
                { "fileD", new string('d', 30 * 1024 * 1024) },
                { "fileE", new string('e', 35 * 1024 * 1024) },
            };

            FileHelpers.CreateTestDirectory(src, content: srcFilesContent);
            FileHelpers.CreateFile($"{src}/fileD", srcFilesContent["fileD"]);
            FileHelpers.CreateFile($"{src}/fileE", srcFilesContent["fileE"]);
            FileHelpers.CreateTestDirectory(desc, src, content: descFilesContent);
            FileHelpers.CreateFile($"{desc}/{src}/fileD", descFilesContent["fileD"]);
            FileHelpers.CreateFile($"{desc}/{src}/fileE", descFilesContent["fileE"]);
            FileHelpers.RefreshGuardFile(desc);

            var backup = BackupHelper.Default;
            backup.CompareLargerFilesBySize = "25M";
            await backup.CreateBackup(desc, src);
            backup.AssertDirectoryNotChanged();
            backup.AssertErrorsCount(0);

            FileHelpers.Assert(src, srcFilesContent);
            FileHelpers.AssertFileExists($"{src}/fileD", srcFilesContent["fileD"]);
            FileHelpers.AssertFileExists($"{src}/fileE", srcFilesContent["fileE"]);
            FileHelpers.Assert($"{desc}/{src}", expectedFilesContent);
            FileHelpers.AssertFileExists($"{desc}/{src}/fileD", expectedFilesContent["fileD"]);
            FileHelpers.AssertFileExists($"{desc}/{src}/fileE", expectedFilesContent["fileE"]);
            FileHelpers.AssertGuardFile(desc, true);
            FileHelpers.AssertGuardFile(src, false);
            FileHelpers.ClearDirectories(desc, src);
        }
    }
}
