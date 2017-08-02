using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicSorter2;
using System.IO;
using System.Diagnostics;

namespace MusicSorterTest
{
    [TestClass]
    public class FilePropertiesTest
    {
        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataStatic")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataStatic\TestFolder")]
        public void ReadFilePropertiesFullPathTest()
        {
            ReadFilePropertiesTest(Path.GetFullPath("TestDataStatic"));
        }

        [TestMethod]
        [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
        [DeploymentItem(@"TestData\TestFile1.mp3", @"TestDataStatic")]
        [DeploymentItem(@"TestData\TestFile2.mp3", @"TestDataStatic\TestFolder")]
        public void ReadFilePropertiesRelativePathTest()
        {
            ReadFilePropertiesTest("TestDataStatic");
        }

        public void ReadFilePropertiesTest(string TestDataFolder)
        {
            string TestFile1Path = Path.Combine(TestDataFolder, "TestFile1.mp3");

            FilePropertiesReader fpr = new FilePropertiesReader(TestDataFolder);

            // test for file properties
            List<FileProperties> files = fpr.GetFiles();
            List<FileProperties> test_file_in_list =
                files.Where(file => file.Path == TestFile1Path).ToList();
            Assert.AreEqual(1, test_file_in_list.Count, $"Not found: {TestFile1Path}");

            FileProperties test_file = test_file_in_list.First();
            Assert.IsFalse(test_file.IsDirectory, "GetFiles returned a directory");
            Assert.AreEqual("Silence", test_file.Title, "Invalid title");
            Assert.AreEqual("omccully", test_file.AnyArtist, "Invalid artist");
            Assert.AreEqual("Not Applicable", test_file.Album, "Invalid album");
            Assert.AreEqual("1", test_file.TrackNumber, "Invalid track number");

            // test for GetFilesAndDirectories seeing the TestFolder
            string TestFolderFullPath = Path.Combine(TestDataFolder, "TestFolder");
            List<FileProperties> directories = fpr.GetFilesAndDirectories().Where(f => f.IsDirectory).ToList();
            Assert.AreEqual(1, directories.Count(d => d.Path == TestFolderFullPath), "Directory count incorrect. " +
                $"Expected: {TestFolderFullPath}\nDirectories found:\n" + String.Join("\n", directories));
        }
    }
}
