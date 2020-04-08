/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Extractor;

namespace Tangosol.Util.Transformer
{
    /// <summary>
    /// <b>ExtractorEventTransformer</b> is a special purpose
    /// <see cref="ICacheEventTransformer"/> implementation that transforms
    /// emitted events, extracting one or more properties from either the
    /// "OldValue" or the "NewValue".
    /// </summary>
    /// <remarks>
    /// This transformation will generally result in the change of the
    /// values' data type.
    /// <p/>
    /// Example: the following code will register a listener to receive
    /// events only if the value of the "AccountBalance" property changes.
    /// The transformed event's "NewValue" will be a <b>IList</b> containing
    /// the "LastTransactionTime" and "AccountBalance" properties. The
    /// "OldValue" will always be <c>null</c>.
    /// <code>
    /// IFilter filter = new ValueChangeEventFilter("AccountBalance");
    /// IValueExtractor extractor = new MultiExtractor("LastTransactionTime,AccountBalance");
    /// ICacheEventTransformer transformer = new ExtractorEventTransformer(null, extractor);
    /// 
    /// cache.AddCacheListener(listener,
    ///                        new CacheEventTransformerFilter(filter,
    ///                                                        transformer),
    ///                        false);
    /// </code>
    /// </remarks>
    /// <author>Gene Gleyzer  2008.06.01</author>
    /// <author>Goran Milosavljevic  2008.07.02</author>
    /// <since> Coherence 3.4</since>
    public class ExtractorEventTransformer : ICacheEventTransformer, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Return a <b>IValueExtractor</b> used to transfrom the event's
        /// OldValue.
        /// </summary>
        /// <returns>
        /// An extractor from the OldValue.
        /// </returns>
        public virtual IValueExtractor OldValueExtractor
        {
            get { return m_extractorOld; }
        }

        /// <summary>
        /// Return a <b>IValueExtractor</b> used to transfrom the event's
        /// NewValue.
        /// </summary>
        /// <returns>
        /// An extractor from the NewValue.
        /// </returns>
        public virtual IValueExtractor NewValueExtractor
        {
            get { return m_extractorNew; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ExtractorEventTransformer()
        {}

        /// <summary> 
        /// Construct a <b>ExtractorEventTransformer</b> that transforms
        /// <b>CacheEventArgs</b> values based on the specified extractor.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// Note: The specified extractor will be applied to both old and new
        /// values.
        /// </remarks>
        /// <param name="extractor"> 
        /// <b>IValueExtractor</b> to extract <b>CacheEventArgs</b> values.
        /// </param>
        public ExtractorEventTransformer(IValueExtractor extractor):this(extractor, extractor)
        {
        }
        
        /// <summary> 
        /// Construct a <b>ExtractorEventTransformer</b> that transforms
        /// <b>CacheEventArgs</b>'s values based on the specified method name.
        /// </summary>
        /// <remarks>
        /// The name could be a comma-delimited sequence of method names
        /// which will result in a <see cref="MultiExtractor"/> that is based
        /// on a corresponding array of <b>IValueExtractor</b> objects;
        /// individual array elements will be either
        /// <see cref="ReflectionExtractor"/> or
        /// <see cref="ChainedExtractor"/> objects.
        /// <p/>
        /// Note: The specified extractor will be applied to both old and new values.
        /// </remarks>
        /// <param name="methodName"> 
        /// The name of the method to invoke via reflection.
        /// </param>
        public ExtractorEventTransformer(String methodName)
            : this(methodName.IndexOf(',') < 0?(methodName.IndexOf('.') < 0?new ReflectionExtractor(methodName):(IValueExtractor) new ChainedExtractor(methodName)): new MultiExtractor(methodName))
        {
        }
        
        /// <summary> 
        /// Construct a <b>ExtractorEventTransformer</b> that transforms
        /// <b>CacheEventArgs</b> values based on the specified extractors.
        /// </summary>
        /// <remarks>
        /// Passing <c>null</c> indicates that the corresponding values
        /// should be skipped completely.
        /// </remarks>
        /// <param name="extractorOld"> 
        /// Extractor to extract the OldValue property(s).
        /// </param>
        /// <param name="extractorNew">
        /// Extractor to extract the NewValue property(s).
        /// </param>
        public ExtractorEventTransformer(IValueExtractor extractorOld, IValueExtractor extractorNew)
        {
            m_extractorOld = extractorOld;
            m_extractorNew = extractorNew;
        }

        #endregion

        #region ICacheEventTransformer implementation

        /// <summary>
        /// Transform the specified <b>CacheEventArgs</b> using the
        /// corresponding extractors.
        /// </summary>
        /// <param name="eventArgs">
        /// <b>CacheEventArgs</b> object to transform.
        /// </param>
        /// <returns>
        /// A modified <b>CacheEventArgs</b> object that contains extracted
        /// values.
        /// </returns>
        public virtual CacheEventArgs Transform(CacheEventArgs eventArgs)
        {
            IValueExtractor extractorOld = OldValueExtractor;
            IValueExtractor extractorNew = NewValueExtractor;

            return new CacheEventArgs(eventArgs.Cache,
                                      eventArgs.EventType,
                                      eventArgs.Key,
                                      extractorOld == null ? null : extractorOld.Extract(eventArgs.OldValue),
                                      extractorNew == null ? null : extractorNew.Extract(eventArgs.NewValue),
                                      eventArgs.IsSynthetic);
        }

        #endregion

        #region IPortableObject implementation

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
            bool isSame = reader.ReadBoolean(0);
            
            m_extractorOld = (IValueExtractor) reader.ReadObject(1);
            m_extractorNew = isSame ? m_extractorOld : (IValueExtractor) reader.ReadObject(2);
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
            IValueExtractor extractorOld = m_extractorOld;
            IValueExtractor extractorNew = m_extractorNew;
            
            if (Equals(extractorOld, extractorNew))
            {
                writer.WriteBoolean(0, true);
                writer.WriteObject(1, extractorNew);
            }
            else
            {
                writer.WriteBoolean(0, false);
                writer.WriteObject(1, extractorOld);
                writer.WriteObject(2, extractorNew);
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>ExtractorEventTransformer</b> with another object
        /// to determine equality.
        /// </summary>
        /// <param name="that">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>ExtractorEventTransformer</b> and the
        /// passed object are equivalent.
        /// </returns>
        public bool Equals(ExtractorEventTransformer that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            return Equals(that.m_extractorOld, m_extractorOld) 
                && Equals(that.m_extractorNew, m_extractorNew);
        }

        /// <summary>
        /// Compare the <b>ExtractorEventTransformer</b> with another object
        /// to determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>ExtractorEventTransformer</b> and the
        /// passed object are equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o)) return false;
            if (ReferenceEquals(this, o)) return true;
            return o is ExtractorEventTransformer
                ? Equals((ExtractorEventTransformer) o)
                : false;
        }

        /// <summary>
        /// Determine a hash value for the <b>ExtractorEventTransformer</b>
        /// object according to the general <b>Object.GetHashCode</b>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this object.
        /// </returns>
        public override int GetHashCode()
        {
            IValueExtractor extractorOld = m_extractorOld;
            IValueExtractor extractorNew = m_extractorNew;
            
            return (extractorOld == null ? 0 : extractorOld.GetHashCode()) +
                   (extractorNew == null ? 0 : extractorNew.GetHashCode());
        }

        /// <summary>
        /// Provide a human-readable representation of this object. 
        /// </summary>
        /// <returns>
        /// A String whose contents represent the value of this object.
        /// </returns>
        public override String ToString()
        {
            IValueExtractor extractorOld = OldValueExtractor;
            IValueExtractor extractorNew = NewValueExtractor;

            String result = GetType().Name + "{";
            if (Equals(extractorOld, extractorNew))
            {
                result += "extractors=" + extractorOld + "}";
            } 
            else
            {
                result += "extractor old=" + extractorOld + ", extractor new=" + extractorNew + "}";
            } 

            return result;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The <b>OldValue</b> extractor.
        /// </summary>
        private IValueExtractor m_extractorOld;
        
        /// <summary>
        /// The <b>NewValue</b> extractor.
        /// </summary>
        private IValueExtractor m_extractorNew;

        #endregion
    }
}