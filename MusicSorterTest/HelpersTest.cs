using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicSorter2;

namespace MusicSorterTest
{
    [TestClass]
    public class HelpersTest
    {
        [TestMethod]
        public void TestRemoveDiacritics()
        {
            Assert.AreEqual("John Lennon", "Johņ Ļénnon".RemoveDiacritics());
            Assert.AreEqual("aeiucgklnsz", "āēīūčģķļņšž".RemoveDiacritics());
            Assert.AreEqual("Bob", "Bob".RemoveDiacritics());
        }

        [TestMethod]
        public void TestRemoveWildcards()
        {
            Assert.AreEqual("Why.mp3", "Why?.mp3".RemoveWildcards(false));
            Assert.AreEqual(@"C:\Music\How.mp3", @"C:\Music\How?.mp3".RemoveWildcards(true));
            Assert.AreEqual(@"CMusicWhy.mp3", @"C:\Music\W*<>|hy?.mp3".RemoveWildcards(false));

        }

        [TestMethod]
        public void TestMakeLegalPath()
        {
            Assert.AreEqual("aeiucgklnsz.mp3", "āēīūčģķļņšž.mp3".MakeLegalPath());
            Assert.AreEqual(@"CMusicWhy.mp3", @"C:\Music\W*<>|hy?.mp3".MakeLegalPath());
        }

        [TestMethod]
        public void TestGetRelativePath()
        {
            string base_path = @"C:\absolute\path";
            string relative_original = @"test\relative\dir\file.exe";
            string absolute_path_file = Path.Combine(base_path, relative_original);
            string rel = Helpers.GetRelativePath(base_path, absolute_path_file);
            Assert.AreEqual(relative_original, rel, "Calculated relative path does not match original");
        }
    }
}
