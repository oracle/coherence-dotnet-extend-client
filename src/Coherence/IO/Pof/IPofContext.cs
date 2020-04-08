/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// The <b>IPofContext</b> interface represents a set of user types that
    /// can be serialized to and deserialized from a POF stream.
    /// </summary>
    /// <author>Cameron Purdy/Jason Howes  2006.07.11</author>
    /// <author>Goran Milosavljevic  2006.08.09</author>
    /// <since>Coherence 3.2</since>
    public interface IPofContext : ISerializer
    {
        /// <summary>
        /// Return an <see cref="IPofSerializer"/> that can be used to
        /// serialize and deserialize an object of the specified user type to
        /// and from a POF stream.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of the user type that can be serialized and
        /// deserialized using the returned <b>IPofSerializer</b>; must be
        /// non-negative.
        /// </param>
        /// <returns>
        /// An <b>IPofSerializer</b> for the specified user type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        IPofSerializer GetPofSerializer(int typeId);

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// object.
        /// </summary>
        /// <param name="o">
        /// An instance of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given object is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        int GetUserTypeIdentifier(object o);

        /// <summary>
        /// Determine the user type identifier associated with the given
        /// type.
        /// </summary>
        /// <param name="type">
        /// A user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type is unknown to
        /// this <b>IPofContext</b>.
        /// </exception>
        int GetUserTypeIdentifier(Type type);

        /// <summary>
        /// Determine the user type identifier associated with the given type
        /// name.
        /// </summary>
        /// <param name="typeName">
        /// The name of a user type; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// The type identifier of the user type associated with the given
        /// type name.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the user type associated with the given type name is unknown
        /// to this <b>IPofContext</b>.
        /// </exception>
        int GetUserTypeIdentifier(string typeName);

        /// <summary>
        /// Determine the name of the type associated with a user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The name of the type associated with the specified user type
        /// identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        string GetTypeName(int typeId);

        /// <summary>
        /// Determine the type associated with the given user type
        /// identifier.
        /// </summary>
        /// <param name="typeId">
        /// The user type identifier; must be non-negative.
        /// </param>
        /// <returns>
        /// The type associated with the specified user type identifier.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified user type is negative or unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        Type GetType(int typeId);

        /// <summary>
        /// Determine if the given object is of a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="o">
        /// The object to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the specified object is of a valid user type.
        /// </returns>
        bool IsUserType(object o);

        /// <summary>
        /// Determine if the given type is a user type known to this
        /// <b>IPofContext</b>.
        /// </summary>
        /// <param name="type">
        /// The type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the specified type is a valid user type.
        /// </returns>
        bool IsUserType(Type type);

        /// <summary>
        /// Determine if the type with the given name is a user type known to
        /// this <b>IPofContext</b>.
        /// </summary>
        /// <param name="typeName">
        /// The name of the type to test; must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// <b>true</b> iff the type with the specified name is a valid user
        /// type.
        /// </returns>
        bool IsUserType(string typeName);
    }
}