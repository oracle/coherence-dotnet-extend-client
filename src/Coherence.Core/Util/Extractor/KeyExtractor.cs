/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Extractor
{

    /// <summary>
    /// The KeyExtractor is a special purpose <see cref="IValueExtractor"/>
    /// implementation that serves as an indicator that a query should be run
    /// against the key objects rather than the values.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The major difference between the KeyExtractor and a standard
    /// <see cref="ReflectionExtractor"/> is that when used in various
    /// <see cref="IFilter"/> implementations it forces the evaluation of
    /// entry keys rather than entry values.</p>
    /// <p>
    /// For example, consider a key object that consists of two properties:
    /// "FirstName" and "LastName". To retrieve all keys that have a value of
    /// the "LastName" property equal to "Smith", the following query could
    /// be used:</p>
    /// <pre>
    /// IValueExtractor extractor = new KeyExtractor("LastName");
    /// ICollection keys = cache.GetKeys(new EqualsFilter(extractor, "Smith"));
    /// </pre>
    /// As of Coherence 3.5, the same effect can be achieved for subclasses 
    /// of the AbstractExtractor, for example:
    /// <pre>
    /// IValueExtractor extractor = new ReflectionExtractor("LastName", 
    /// null,AbstractExtractor.KEY);
    /// ICollection keys = cache.GetKeys(new EqualsFilter(extractor, "Smith"));
    /// </pre>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.06.12</author>
    /// <author>Ana Cikic  2006.09.12</author>
    /// <since>Coherence 3.2</since>
    public class KeyExtractor : AbstractExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The underlying <see cref="IValueExtractor"/>.
        /// </summary>
        /// <value>
        /// The underlying <b>IValueExtractor</b>.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get { return m_extractor; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public KeyExtractor() : this((IValueExtractor) null)
        {}

        /// <summary>
        /// Construct a KeyExtractor based on a specified
        /// <see cref="IValueExtractor"/>.
        /// </summary>
        /// <param name="extractor">
        /// The underlying <b>IValueExtractor</b>.
        /// </param>
        public KeyExtractor(IValueExtractor extractor)
        {
            m_target    = KEY;
            m_extractor = extractor == null ? IdentityExtractor.Instance : extractor;
        }

        /// <summary>
        /// Construct a KeyExtractor for a specified member name.
        /// </summary>
        /// <param name="member">
        /// A member name to construct an underlying
        /// <see cref="ReflectionExtractor"/> for; this parameter can also be
        /// a dot-delimited sequence of member names which would result in a
        /// KeyExtractor based on the <see cref="ChainedExtractor"/> that is
        /// based on an array of corresponding <b>ReflectionExtractor</b>
        /// objects.
        /// </param>
        public KeyExtractor(string member)
        {
            Debug.Assert(member != null, "Member name is missing");

            m_target    = KEY;
            m_extractor = member.IndexOf('.') < 0
                              ? new ReflectionExtractor(member)
                              : (IValueExtractor) new ChainedExtractor(member);
        }

        #endregion

        #region IValueExtractor implementation

        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The returned value may be <c>null</c>.
        /// </remarks>
        /// <param name="obj">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If this IValueExtractor is incompatible with the passed object to
        /// extract a value from and the implementation <b>requires</b> the
        /// passed object to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this AbstractExtractor cannot handle the passed object for any
        /// other reason; an implementor should include a descriptive
        /// message.
        /// </exception>
        public override object Extract(object obj)
        {
            return m_extractor.Extract(obj);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the KeyExtractor with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two KeyExtractor objects are considered equal if their underlying
        /// <b>IValueExtractors</b> are equal.
        /// </remarks>
        /// <param name="o">
        /// The reference object with which to compare.
        /// </param>
        /// <returns>
        /// <b>true</b> if this KeyExtractor and the passed object are
        /// equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is KeyExtractor)
            {
                KeyExtractor target = (KeyExtractor) o;
                return Equals(m_extractor, target.m_extractor);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the KeyExtractor object according
        /// to the general <b>Object.GetHashCode</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this IValueExtractor object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_extractor.GetHashCode();
        }

        /// <summary>
        /// Provide a human-readable description of this KeyExtractor
        /// object.
        /// </summary>
        /// <returns>
        /// A human-readable description of this KeyExtractor object.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + "(extractor=" + m_extractor + ")";
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
            m_extractor = (IValueExtractor) reader.ReadObject(0);
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
            writer.WriteObject(0, m_extractor);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying IValueExtractor.
        /// </summary>
        protected IValueExtractor m_extractor;

        #endregion
    }
}