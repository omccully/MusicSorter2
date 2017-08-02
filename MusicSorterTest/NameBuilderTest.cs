using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicSorter2;

namespace MusicSorterTest
{
    [TestClass]
    public class NameBuilderTest
    {
        [TestMethod]
        public void BuildNameConstructAndToStringTest()
        {
            string[] testformats = {
                "{#}. {T}",
                "{AR} {AL} {#} {T}",
                "{#}}}} !@#$%^&*(){}",
                "{}\\:\"<;\'>?,./",
                "AR AL # T",
                "{#}}. {T}"
            };

            foreach(string s in testformats)
            {
                Assert.AreEqual(s, new NameBuilder(s).ToString());
            }
        }

        [TestMethod]
        public void BuildNameTest()
        {
            NameBuilder nb_a = new NameBuilder("{#}. {T}");
            Assert.AreEqual("6. A Day In The Life", nb_a.Build("6", "A Day In The Life", "Imagine", "John Lennon"));
            Assert.AreEqual(". A Day In The Life", nb_a.Build(null, "A Day In The Life", "Imagine", "John Lennon"));
            Assert.AreEqual(". A Day In The Life", nb_a.Build("", "A Day In The Life", "Imagine", "John Lennon"));

            NameBuilder nb_b = new NameBuilder("{AR} {AL} {#} {T}");
            Assert.AreEqual("John Lennon Imagine 6 A Day In The Life", nb_b.Build("6", "A Day In The Life", "Imagine", "John Lennon"));
            Assert.AreEqual("John LénnøN {}|[]:;\'\"\\<>,.?/ æåø -=!@#$%^&*()", nb_b.Build("æåø", "-=!@#$%^&*()", "{}|[]:;\'\"\\<>,.?/", "John LénnøN"));

            NameBuilder nb_c = new NameBuilder("{#}}. {T}");
            Assert.AreEqual("6}. A Day In The Life", nb_c.Build("6", "A Day In The Life", "Imagine", "John Lennon"));
        }


    }
}
