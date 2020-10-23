/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Processor;

namespace Tangosol.Internal.Util.Processor
{
    /// <summary>
    /// Contains factory methods and entry processor classes that are used to implement
    /// functionality exposed via different variants of <see cref="INamedCache"/> API.
    /// </summary>
    /// <author>as 2015.01.17</author>
    /// <author>lh 2015.04.08</author>
    /// <since>Coherence 12.2.1</since>
    public class CacheProcessors
    {
        #region Factory methods

        /// <summary>
        /// Return a <b>Null</b> entry processor.
        /// </summary>
        /// <returns>
        /// A <b>Null</b> processor.
        /// </returns>
        public static IEntryProcessor Nop()
        {
            return new NullProcessor();
        }

        /// <summary>
        /// Return a <b>Get</b> entry processor.
        /// </summary>
        /// <returns>
        /// A <b>Get</b> processor.
        /// </returns>
        public static IEntryProcessor Get()
        {
            return new GetProcessor();
        }

        /// <summary>
        /// Return a <b>GetOrDefault</b> entry processor.
        /// </summary>
        /// <returns>
        /// A <b>GetOrDefault</b> processor.
        /// </returns>
        public static IEntryProcessor GetOrDefault()
        {
            return new GetOrDefaultProcessor();
        }

        /// <summary>
        /// Return a <b>Insert</b> entry processor.
        /// </summary>
        /// <param name="value">
        /// The value to insert.
        /// </param>
        /// <param name="cMillis">
        /// The number of milliseconds until the cache entry will expire.
        /// </param>
        /// <returns>
        /// A <b>Insert</b> processor.
        /// </returns>
        public static IEntryProcessor Insert(object value, long cMillis)
        {
            return new InsertProcessor(value, cMillis);
        }

        /// <summary>
        /// Return a <b>InsertAll</b> entry processor.
        /// </summary>
        /// <param name="map">
        /// The map of entries to insert.
        /// </param>
        /// <returns>
        /// A <b>InsertAll</b> processor.
        /// </returns>
        public static IEntryProcessor InsertAll(IDictionary map)
        {
            return new InsertAllProcessor(map);
        }

        /// <summary>
        /// Return a <b>InsertIfAbsent</b> entry processor.
        /// </summary>
        /// <param name="value">
        /// The value to insert.
        /// </param>
        /// <returns>
        /// A <b>InsertIfAbsent</b> processor.
        /// </returns>
        public static IEntryProcessor InsertIfAbsent(object value)
        {
            return new InsertIfAbsentProcessor(value);
        }

        /// <summary>
        /// Return a <b>Remove</b> entry processor.
        /// </summary>
        /// <returns>
        /// A <b>Remove</b> processor.
        /// </returns>
        public static IEntryProcessor Remove()
        {
            return new RemoveProcessor();
        }

        /// <summary>
        /// Return a <b>RemoveBlind</b> entry processor.
        /// </summary>
        /// <returns>
        /// A <b>RemoveBlind</b> processor.
        /// </returns>
        public static IEntryProcessor RemoveBlind()
        {
            return new RemoveBlindProcessor();
        }

        /// <summary>
        /// Return a <b>Remove</b> entry processor.
        /// </summary>
        /// <param name="value">
        /// The value to remove.
        /// </param>
        /// <returns>
        /// A <b>Remove</b> processor.
        /// </returns>
        public static IEntryProcessor Remove(object value)
        {
            return new RemoveValueProcessor(value);
        }

        /// <summary>
        /// Return a <b>Replace</b> entry processor.
        /// </summary>
        /// <param name="value">
        /// The value to replce.
        /// </param>
        /// <returns>
        /// A <b>Replace</b> processor.
        /// </returns>
        public static IEntryProcessor Replace(object value)
        {
            return new ReplaceProcessor(value);
        }

        /// <summary>
        /// Return a <b>Replace</b> entry processor.
        /// </summary>
        /// <param name="oldValue">
        /// The old value to be replaced.
        /// </param>
        /// <param name="newValue">
        /// The new value to replace the old value.
        /// </param>
        /// <returns>
        /// A <b>Replace</b> processor.
        /// </returns>
        public static IEntryProcessor Replace(object oldValue, object newValue)
        {
            return new ReplaceValueProcessor(oldValue, newValue);
        }

        #endregion

        #region Entry Processors

        ///<summary>
        /// Abstract base class for entry processors.
        ///</summary>
        public abstract class BaseProcessor : AbstractProcessor, IPortableObject 
        {
            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public virtual void ReadExternal(IPofReader reader)
            {
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public virtual void WriteExternal(IPofWriter writer)
            {
            }

            #endregion
        }

        ///<summary>
        /// Null entry processor.
        ///</summary>
        public class NullProcessor : BaseProcessor
        {
            #region IEntryProcessor methods

            /// <summary>
            /// Process an <see cref="IInvocableCacheEntry"/>.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// Null.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                return null;
            }

            #endregion
        }

        /// <summary>
        /// Get entry processor.
        /// </summary>
        public class GetProcessor : BaseProcessor
        {
            #region IEntryProcessor methods

            /// <summary>
            /// Process an Get entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                return entry.Value;
            }

            #endregion
        }

        /// <summary>
        /// GetOrDefault entry processor.
        /// </summary>
        public class GetOrDefaultProcessor : BaseProcessor
        {
            #region IEntryProcessor methods

            /// <summary>
            /// Process an GetOrDefault entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                return entry == null ? Optional.Empty() : Optional.OfNullable(entry.Value);
            }

            #endregion
        }

        /// <summary>
        /// Insert entry processor.
        /// </summary>
        public class InsertProcessor : BaseProcessor
        {
            #region Constructors

            /// <summary>
            /// Default constructor
            /// </summary>
            public InsertProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="InsertProcessor"/> entry processor.
            /// </summary>
            /// <param name="value">
            /// The value to insert.
            /// </param>
            /// <param name="cMillis">
            /// The number of milliseconds until the cache entry will expire.
            /// </param>
            public InsertProcessor(object value, long cMillis)
            {
                m_value = value;
                m_cMillis = cMillis;
            }

            #endregion

            #region IEntryProcessor methods

            /// <summary>
            /// Process an GetOrDefault entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing.
            /// </returns>
            public override Object Process(IInvocableCacheEntry entry)
            {
                entry.Value = m_value;
                return m_value;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)
            {
                m_value = reader.ReadObject(0);
                m_cMillis = reader.ReadInt64(1);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(0, m_value);
                writer.WriteInt64(1, m_cMillis);
            }

            #endregion

            #region data members

            /// <summary>
            /// The value to insert.
            /// </summary>
            protected object m_value;

            /// <summary>
            /// The number of milliseconds until the cache entry will expire.
            /// </summary>
            protected long m_cMillis;

            #endregion
        }

        /// <summary>
        /// InsertAll entry processor.
        /// </summary>
        public class InsertAllProcessor : BaseProcessor
        {
            #region Constructors

            /// <summary>
            /// The default constructor.
            /// </summary>
            public InsertAllProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="InsertAllProcessor"/> entry processor.
            /// </summary>
            /// <param name="map">
            /// The map of entries to insert.
            /// </param>
            public InsertAllProcessor(IDictionary map)
            {
                m_map = map;
            }

            #endregion

            #region IEntryProcessor methods

            /// <summary>
            /// Process an InsertAll entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// Null.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                entry.Value = m_map[entry.Key];
                return null;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)
            {
                reader.ReadDictionary(0, m_map);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteDictionary(0, m_map);
            }

            #endregion

            #region data members

            /// <summary>
            /// The map of entries to insert in.
            /// </summary>
            protected IDictionary m_map = new Hashtable();

            #endregion
        }

        /// <summary>
        /// InsertIfAbsent entry processor
        /// </summary>
        public class InsertIfAbsentProcessor : BaseProcessor
        {
            #region Constructors

            /// <summary>
            /// Default constructor
            /// </summary>
            public InsertIfAbsentProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="InsertIfAbsentProcessor"/> entry processor.
            /// </summary>
            /// <param name="value">
            /// The value to insert.
            /// </param>
            public InsertIfAbsentProcessor(object value)
            {
                m_value = value;
            }

            #endregion

            #region IEntryProcessor methods

            /// <summary>
            /// Process an InsertIfAbsent entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// If value is present, return the value; otherwise, return null.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                if (entry.IsPresent && entry.Value != null)
                {
                    return entry.Value;
                }
                entry.SetValue(m_value, false);
                return null;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)
            {
                m_value = reader.ReadObject(0);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(0, m_value);
            }

            #endregion

            #region data members

            /// <summary>
            /// The value to insert.
            /// </summary>
            protected Object m_value;

            #endregion
        }

        /// <summary>
        /// Remove entry processor.
        /// </summary>
        public class RemoveProcessor : BaseProcessor
        {
            #region IEntryProcessor methods

            /// <summary>
            /// Process an Remove entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The value removed.
            /// </returns>
            public override Object Process(IInvocableCacheEntry entry)
            {
                Object value = entry.Value;
                entry.Remove(false);
                return value;
            }

            #endregion
        }

        /// <summary>
        /// Remove entry processor.
        /// </summary>
        public class RemoveBlindProcessor : BaseProcessor
        {
            #region IEntryProcessor methods

            /// <summary>
            /// Process an RemoveBlind entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                entry.Remove(false);
                return null;
            }

            #endregion
        }

        /// <summary>
        /// RemoveValue entry processor.
        /// </summary>
        public class RemoveValueProcessor : BaseProcessor
        {
            #region Constructors

            /// <summary>
            /// The default constructor.
            /// </summary>
            public RemoveValueProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="RemoveValueProcessor"/> entry processor.
            /// </summary>
            /// <param name="value">
            /// The value to remove.
            /// </param>
            public RemoveValueProcessor(Object value)
            {
                m_value = value;
            }

            #endregion

            #region IEntryProcessor methods

            /// <summary>
            /// Process an RemoveValue entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing: true, if the entry is found and
            /// removed; false otherwise.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                if (entry.IsPresent)
                {
                    Object valueCurrent = entry.Value;
                    if (Equals(valueCurrent, m_value))
                    {
                        entry.Remove(false);
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)
            {
                m_value = reader.ReadObject(0);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(0, m_value);
            }

            #endregion

            // ---- data members ------------------------------------------------

            /// <summary>
            /// The value to remove.
            /// </summary>
            protected Object m_value;
        }

        /// <summary>
        /// Replace entry processor.
        /// </summary>
        public class ReplaceProcessor : BaseProcessor
        {
            /// <summary>
            /// The default constructor.
            /// </summary>
            public ReplaceProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="ReplaceProcessor"/> entry processor.
            /// </summary>
            /// <param name="value">
            /// The value to replace.
            /// </param>
            public ReplaceProcessor(object value)
            {
                m_value = value;
            }

            #region IEntryProcessor

            /// <summary>
            /// Process an Replace entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                object oldValue = entry.Value;
                entry.Value = m_value;
                return oldValue;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)
            {
                m_value = reader.ReadObject(0);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)
            {
                writer.WriteObject(0, m_value);
            }

            #endregion

            #region data members

            /// <summary>
            /// The value to replace with.
            /// </summary>
            protected object m_value;

            #endregion
        }

        /// <summary>
        /// ReplaceValue entry processor.
        /// </summary>
        public class ReplaceValueProcessor : BaseProcessor
        {
            /// <summary>
            /// The default constructor.
            /// </summary>
            public ReplaceValueProcessor()
            {
            }

            /// <summary>
            /// Creates an instance of a <see cref="ReplaceValueProcessor"/> entry processor.
            /// </summary>
            /// <param name="oldValue">
            /// The old value to be replaced.
            /// </param>
            /// <param name="newValue">
            /// The new value to replace the old value.
            /// </param>
            public ReplaceValueProcessor(object oldValue, object newValue)
            {
                m_oldValue = oldValue;
                m_newValue = newValue;
            }

            #region IEntryProcessor

            /// <summary>
            /// Process an <b>ReplaceValue</b> entry processor.
            /// </summary>
            /// <param name="entry">
            /// The <b>IInvocableCacheEntry</b> to process.
            /// </param>
            /// <returns>
            /// The result of the processing: true, if the entry is found and
            /// value is replaced; false, otherwise.
            /// </returns>
            public override object Process(IInvocableCacheEntry entry)
            {
                if (entry.IsPresent)
                {
                    object valueCurrent = entry.Value;
                    if (Equals(valueCurrent, m_oldValue))
                    {
                        entry.Value = m_newValue;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region IPortableObject methods

            /// <summary>
            /// Restore the contents of a user type instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void ReadExternal(IPofReader reader)

            {
                m_oldValue = reader.ReadObject(0);
                m_newValue = reader.ReadObject(1);
            }

            /// <summary>
            /// Save the contents of a POF user type instance by writing its
            /// state using the specified <see cref="IPofWriter"/> object.
            /// </summary>
            /// <param name="writer">
            /// The <b>IPofWriter</b> to which to write the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override void WriteExternal(IPofWriter writer)

            {
                writer.WriteObject(0, m_oldValue);
                writer.WriteObject(1, m_newValue);
            }

            #endregion

            #region data members

            /// <summary>
            /// The old value to be replaced.
            /// </summary>
            protected object m_oldValue;
            
            /// <summary>
            /// The new value to replace the old value.
            /// </summary>
            protected object m_newValue;

            #endregion
        }

        #endregion
    }

}
