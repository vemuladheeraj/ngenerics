/*  
  Copyright 2007-2017 The NGenerics Team
 (https://github.com/ngenerics/ngenerics/wiki/Team)

 This program is licensed under the MIT License.  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at https://opensource.org/licenses/MIT.
*/

using System;
using System.Collections.Generic;
using NGenerics.DataStructures.Trees;
using NUnit.Framework;

namespace NGenerics.Tests.DataStructures.Trees.BinarySearchTreeTests
{
    [TestFixture]
    public class Maximum : BinarySearchTreeTest
    {

        [Test]
        public void Simple()
        {
            var tree = GetTestTree();
            var i = tree.Maximum;

            Assert.AreEqual(i.Key, 19);
            Assert.AreEqual(i.Value, "19");
        }

        [Test]
        public void ExceptionInvalidMax()
        {
            var tree = new BinarySearchTree<int, string>();
            KeyValuePair<int, string> i;
            Assert.Throws<InvalidOperationException>(() => i = tree.Maximum);
        }

    }
}