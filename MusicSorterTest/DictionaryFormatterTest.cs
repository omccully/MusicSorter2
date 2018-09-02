using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicSorter2;

namespace MusicSorterTest
{
    [TestClass]
    public class DictionaryFormatterTest
    {
        [TestMethod]
        public void Format()
        {
            DictionaryFormatter df = new DictionaryFormatter("aaa {num}. {name} zzz");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["num"] = "5";
            dict["name"] = "Panic Station";

            Assert.AreEqual("aaa 5. Panic Station zzz", df.Format(dict));
        }

        [TestMethod]
        public void Format_MissingDictionaryItem()
        {
            DictionaryFormatter df = new DictionaryFormatter("{num}. {name} zzz");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["num"] = "5";

            Assert.ThrowsException<KeyNotFoundException>(delegate
            {
                df.Format(dict);
            });
        }

        [TestMethod]
        public void FormatPerformanceTest()
        {
            DictionaryFormatter df = new DictionaryFormatter("{num}. {name} zzz");
 
            for (int i = 0; i < 100000; i++)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["num"] = "5" + i;
                dict["name"] = "Panic Station" + i;
                df.Format(dict);
            }

            //Assert.AreEqual("5. Panic Station zzz", );
        }
    }
}
