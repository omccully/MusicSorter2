using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicSorter2;

namespace MusicSorterTest
{
    [TestClass]
    public class SorterTest
    {
        #region Test Step 1 (Unpack)
        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataUnpackFullPath\TestFolder2\subfolder")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataUnpackFullPath\TestFolder")]
        public void TestUnpackFullPath()
        {
            TestUnpack(Path.GetFullPath("TestDataUnpackFullPath"));
        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataUnpackRelativePath\TestFolder2\subfolder")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataUnpackRelativePath\TestFolder")]
        public void TestUnpackRelativePath()
        {
            TestUnpack("TestDataUnpackRelativePath");
        }

        /// <summary>
        /// Tests Sorter.UnpackAll()
        /// </summary>
        /// <param name="RootPath">RootPath must contain TestFile1.mp3 and TestFile2.mp3</param>
        void TestUnpack(string RootPath)
        {
            Sorter sorter = new Sorter(RootPath, "{#}. {T}");

            string File1SourcePath = Path.Combine(RootPath, @"TestFolder2\subfolder\TestFile1.mp3");
            string File2SourcePath = Path.Combine(RootPath, @"TestFolder\TestFile2.mp3");
            string File1DestinationPath = Path.Combine(RootPath, "TestFile1.mp3");
            string File2DestinationPath = Path.Combine(RootPath, "TestFile2.mp3");
            
            EventTracker<FileChangedEventArgs> tracker = new EventTracker<FileChangedEventArgs>(
                new FileChangedEventArgs[] {
                    new FileChangedEventArgs(File1SourcePath, File1DestinationPath),
                    new FileChangedEventArgs(File2SourcePath, File2DestinationPath)
            });

            sorter.FileUnpacked += delegate (object sender, FileChangedEventArgs e)
            {
                tracker.TrackEvent(e);
            };

            sorter.UnpackAll();

            Assert.AreEqual(0, tracker.ObjectsNotSeen().Length, "Not all expected events occurred");

            bool File1Found = false, File2Found = false;

            var files = Directory.EnumerateFileSystemEntries(RootPath);
            foreach (string file in files)
            {
                // make sure there are no directories in TestDataUnpack
                Assert.IsFalse(Directory.Exists(file), "Directory exists in result folder");
                switch(Path.GetFileName(file))
                {
                    case "TestFile1.mp3":
                        File1Found = true;
                        break;
                    case "TestFile2.mp3":
                        File2Found = true;
                        break;
                    default:
                        Assert.Fail("Unknown file in result folder: " + file);
                        break;
                }
            }

            Assert.IsTrue(File1Found);
            Assert.IsTrue(File2Found);
        }
        #endregion

        #region Test Step 2 (Pack) and in combination with 3 (Rename)
        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", "TestDataPackFullPathDontRename")]
        [DeploymentItem(@"TestData\TestFile2.mp3", "TestDataPackFullPathDontRename")]
        public void TestPackFullPathDontRename()
        {
            TestPack(Path.GetFullPath("TestDataPackFullPathDontRename"));
        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", "TestDataPackRelativePathDontRename")]
        [DeploymentItem(@"TestData\TestFile2.mp3", "TestDataPackRelativePathDontRename")]
        public void TestPackRelativePathDontRename()
        {
            TestPack("TestDataPackRelativePathDontRename");
        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", "TestDataPackFullPathAndRename")]
        [DeploymentItem(@"TestData\TestFile2.mp3", "TestDataPackFullPathAndRename")]
        public void TestPackFullPathAndRename()
        {
            TestPack(Path.GetFullPath("TestDataPackFullPathAndRename"), true);
        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", "TestDataPackRelativePathAndRename")]
        [DeploymentItem(@"TestData\TestFile2.mp3", "TestDataPackRelativePathAndRename")]
        public void TestPackRelativePathAndRename()
        {
            TestPack("TestDataPackRelativePathAndRename", true);
        }

        void TestPack(string RootPath, bool Rename=false)
        {
            Sorter sorter = new Sorter(RootPath, "{#}. {T}");

            string File1SourcePath = Path.Combine(RootPath, "TestFile1.mp3");
            string File1DestinationFolder1 = Path.Combine(RootPath, "omccully");
            string File1DestinationFolder2 = Path.Combine(File1DestinationFolder1, "Not Applicable");
            string File1DestinationPath = Path.Combine(File1DestinationFolder2, 
                (Rename ? "1. Silence.mp3" : "TestFile1.mp3"));
            
            string File2SourcePath = Path.Combine(RootPath, "TestFile2.mp3");
            string File2DestinationFolder1 = Path.Combine(RootPath, "mccullo");
            string File2DestinationFolder2 = Path.Combine(File2DestinationFolder1, "Not Applicable");
            string File2DestinationPath = Path.Combine(File2DestinationFolder2,
                (Rename ? "2. Silence (Remix).mp3" : "TestFile2.mp3"));

            EventTracker<FolderCreatedEventArgs> create_tracker = new EventTracker<FolderCreatedEventArgs>(
                new FolderCreatedEventArgs[] {
                    new FolderCreatedEventArgs(File1DestinationFolder1),
                    new FolderCreatedEventArgs(File1DestinationFolder2),
                    new FolderCreatedEventArgs(File2DestinationFolder1),
                    new FolderCreatedEventArgs(File2DestinationFolder2)
            });


            sorter.FolderCreated += delegate (object sender, FolderCreatedEventArgs e)
            {
                create_tracker.TrackEvent(e);
            };

            EventTracker<FileChangedEventArgs> move_tracker = new EventTracker<FileChangedEventArgs>(
                new FileChangedEventArgs[] {
                    new FileChangedEventArgs(File1SourcePath, File1DestinationPath),
                    new FileChangedEventArgs(File2SourcePath, File2DestinationPath)
            });

            sorter.FileMoved += delegate (object sender, FileChangedEventArgs e)
            {
                move_tracker.TrackEvent(e);
            };

            sorter.PackAll(Rename); 

            Assert.IsTrue(create_tracker.WasSuccessful(), "create_tracker failed: " + create_tracker.Debug());
            Assert.IsTrue(move_tracker.WasSuccessful(), "move_tracker failed: " + move_tracker.Debug());

            Assert.IsTrue(File.Exists(File1DestinationPath), "File1 not moved properly");
            Assert.IsTrue(File.Exists(File2DestinationPath), "File2 not moved properly");
        }
        #endregion

        #region Test Step 3 (Rename)
        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataRenameRelativePath\TestFolder1")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataRenameRelativePath\TestFolder2\subfolder")]
        public void TestRenameRelativePath()
        {
            TestRename("TestDataRenameRelativePath");

        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataRenameFullPath\TestFolder1")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataRenameFullPath\TestFolder2\subfolder")]
        public void TestRenameFullPath()
        {
            TestRename(Path.GetFullPath("TestDataRenameFullPath"));

        }

        public void TestRename(string RootPath)
        {
            Sorter sorter = new Sorter(RootPath, "{#}. {T}");

            string File1Before = Path.Combine(RootPath, @"TestFolder1\TestFile1.mp3");
            string File1After = Path.Combine(RootPath, @"TestFolder1\1. Silence.mp3");
            string File2Before = Path.Combine(RootPath, @"TestFolder2\subfolder\TestFile2.mp3");
            string File2After = Path.Combine(RootPath,@"TestFolder2\subfolder\2. Silence (Remix).mp3");

            EventTracker<FileChangedEventArgs> name_change_tracker = new EventTracker<FileChangedEventArgs>(
                new FileChangedEventArgs[] {
                    new FileChangedEventArgs(File1Before, File1After),
                    new FileChangedEventArgs(File2Before, File2After)
            });

            sorter.FileRenamed += delegate (object sender, FileChangedEventArgs e)
            {
                name_change_tracker.TrackEvent(e);
            };

            sorter.NameChange();

            Assert.IsTrue(name_change_tracker.WasSuccessful(), "name_change_tracker failed: " + name_change_tracker.Debug());
            Assert.IsTrue(File.Exists(File1After), "File1 not changed properly");
            Assert.IsTrue(File.Exists(File2After), "File2 not changed properly");
        }
        #endregion
    }
}
