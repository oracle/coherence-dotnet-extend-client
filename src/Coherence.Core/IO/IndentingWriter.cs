/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Text;

using Tangosol.Util;

namespace Tangosol.IO
{
    /// <summary>
    /// An IndentingWriter is used to indent line-based output to an
    /// underlying <b>TextWriter</b>.
    /// </summary>
    /// <author>Cameron Purdy  2000.10.17</author>
    /// <author>Ana Cikic  2009.09.09</author>
    public class IndentingWriter : TextWriter
    {
        #region Constructors

        /// <summary>
        /// Construct an IndentingWriter that indents a certain number of
        /// spaces.
        /// </summary>
        /// <param name="writer">
        /// The underlying <b>TextWriter</b> to write to.
        /// </param>
        /// <param name="spaces">
        /// The number of spaces to indent each line with.
        /// </param>
        public IndentingWriter(TextWriter writer, int spaces) : this(writer, StringUtils.Dup(' ', spaces))
        {}

        /// <summary>
        /// Construct an IndentingWriter that indents using an indention
        /// string.
        /// </summary>
        /// <param name="writer">
        /// The underlying <b>TextWriter</b> to write to.
        /// </param>
        /// <param name="indent">
        /// The string value to indent each line with.
        /// </param>
        public IndentingWriter(TextWriter writer, string indent)
        {
            m_writer = writer;

            if (indent.IndexOf('\n') >= 0)
            {
                throw new ArgumentException("indent contains new line char");
            }

            // if this and underlying writers are both indenting writers, combine
            // the indentation strings
            Type type = typeof(IndentingWriter);
            if (GetType() == type && writer.GetType() == type)
            {
                IndentingWriter that = (IndentingWriter) writer;
                indent   = new StringBuilder().Append(that.m_achIndent).Append(indent).ToString();
                m_writer = that.m_writer;
            }

            m_achIndent = indent.ToCharArray();
        }

        #endregion

        #region TextWriter members

        /// <summary>
        /// When overridden in a derived class, returns the <b>Encoding</b>
        /// in which the output is written.
        /// </summary>
        /// <returns>
        /// The Encoding in which the output is written.
        /// </returns>
        public override Encoding Encoding
        {
            get { return m_writer.Encoding; }
        }

        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">
        /// The character to write to the text stream.
        /// </param>
        public override void Write(char value)
        {
            lock (this)
            {
                if (value == '\n')
                {
                    m_isNewline = true;
                }
                else if (m_isNewline)
                {
                    m_isNewline = false;
                    if (!m_isSuspended)
                    {
                        m_writer.Write(m_achIndent);
                    }
                }

                m_writer.Write(value);
            }
        }

        /// <summary>
        /// Writes a character array to the text stream.
        /// </summary>
        /// <param name="buffer">
        /// The character array to write to the text stream.
        /// </param>
        public override void Write(char[] buffer)
        {
            lock (this)
            {
                if (buffer != null)
                {
                    Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Writes a subarray of characters to the text stream.
        /// </summary>
        /// <param name="buffer">
        /// The character array to write data from.
        /// </param>
        /// <param name="index">
        /// Starting index in the buffer.
        /// </param>
        /// <param name="count">
        /// The number of characters to write.
        /// </param>
        public override void Write(char[] buffer, int index, int count)
        {
            lock (this)
            {
                for (int i = 0; i < count; ++i)
                {
                    Write(buffer[index++]);
                }
            }
        }

        /// <summary>
        /// Writes a string to the text stream.
        /// </summary>
        /// <param name="value">
        /// The string to write.
        /// </param>
        public override void Write(string value)
        {
            lock (this)
            {
                Write(value.ToCharArray());
            }
        }

        /// <summary>
        /// Writes a line terminator to the text stream.
        /// </summary>
        public override void WriteLine()
        {
            m_writer.WriteLine();
            m_isNewline = true;
        }

        #endregion

        #region IndentingWriter methods

        /// <summary>
        /// Suspends indentation.
        /// </summary>
        public virtual void Suspend()
        {
            m_isSuspended = true;
        }

        /// <summary>
        /// Resumes indentation.
        /// </summary>
        public virtual void Resume()
        {
            m_isSuspended = false;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying TextWriter to write to.
        /// </summary>
        private TextWriter m_writer;

        /// <summary>
        /// The characters to use to indent each line.
        /// </summary>
        private char[] m_achIndent;

        /// <summary>
        /// True if the IndentingWriter is on a new line.
        /// </summary>
        private bool m_isNewline = true;

        /// <summary>
        /// True if the indentation feature of the IndentingWriter is
        /// suspended.
        /// </summary>
        private bool m_isSuspended = false;

        #endregion
    }
}
