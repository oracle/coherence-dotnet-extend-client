/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using NUnit.Framework;

namespace Tangosol.IO
{
    [TestFixture]
    public class IndentingWriterTests
    {
        [Test]
        public void TestIndentingWriter()
        {
            char[] line1 = new char[] {'l', 'i', 'n', 'e', '1'};
            char nl = '\n';
            char[] line2 = new char[] {'z', 'l', 'i', 'n', 'e', '2'};
            string line3 = "\nline3";

            StringWriter strWriter = new StringWriter();
            IndentingWriter indWriter = new IndentingWriter(strWriter, 4);
            Assert.AreEqual(indWriter.Encoding, strWriter.Encoding);
            indWriter.Write(line1);
            indWriter.Write(nl);
            indWriter.Write(line2, 1, 5);
            indWriter.Write(line3);
            Assert.AreEqual("    line1\n    line2\n    line3", strWriter.ToString());
            strWriter = new StringWriter();
            indWriter = new IndentingWriter(strWriter, 4);
            indWriter.Suspend();
            indWriter.Write(line1);
            indWriter.Write(nl);
            indWriter.Write(line2, 1, 5);
            indWriter.Write(line3);
            Assert.AreEqual("line1\nline2\nline3", strWriter.ToString());
            indWriter.Resume();
            indWriter.WriteLine();
            Assert.AreEqual("line1\nline2\nline3\r\n", strWriter.ToString());
            indWriter.Write(line1);
            Assert.AreEqual("line1\nline2\nline3\r\n    line1", strWriter.ToString());

            strWriter = new StringWriter();
            indWriter = new IndentingWriter(strWriter, "ZZZ");
            indWriter.Write(line1);
            indWriter.Write(nl);
            indWriter.Write(line2, 1, 5);
            indWriter.Write(line3);
            Assert.AreEqual("ZZZline1\nZZZline2\nZZZline3", strWriter.ToString());
            strWriter = new StringWriter();
            indWriter = new IndentingWriter(strWriter, "ZZZ");
            indWriter.Suspend();
            indWriter.Write(line1);
            indWriter.Write(nl);
            indWriter.Write(line2, 1, 5);
            indWriter.Write(line3);
            Assert.AreEqual("line1\nline2\nline3", strWriter.ToString());

            strWriter = new StringWriter();
            indWriter = new IndentingWriter(strWriter, "ZZZ");
            IndentingWriter indWriter2 = new IndentingWriter(indWriter, "XXX");
            indWriter2.Write(line1);
            indWriter2.Write(nl);
            indWriter2.Write(line2, 1, 5);
            indWriter2.Write(line3);
            Assert.AreEqual("ZZZXXXline1\nZZZXXXline2\nZZZXXXline3", strWriter.ToString());
        }

        [Test]
        public void TestIndentingWriterWithException()
        {
            StringWriter strWriter = new StringWriter();
            Assert.That(() => new IndentingWriter(strWriter, "\r\n"), Throws.ArgumentException);
        }
    }
}
