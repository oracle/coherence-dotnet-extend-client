/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Globalization;
using System.Reflection;

namespace Tangosol.Util
{
    /// <summary>
    /// Resolves a <b>System.Type</b> by name.
    /// </summary>
    /// <remarks>
    /// <p>
    /// It resolves type by either loading it directly from the
    /// assembly (if one is specified within a type name), or by
    /// iterating over all of the loaded assemblies and trying
    /// to find the type.</p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class TypeResolver
    {
        /// <summary>
        /// Gets a <b>System.Type</b> for the <paramref name="typeName"/>
        /// supplied as parameter.
        /// </summary>
        /// <param name="typeName">
        /// The name of a <b>System.Type</b> to resolve.
        /// </param>
        /// <returns>
        /// <b>System.Type</b> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the <paramref name="typeName"/> could not be resolved
        /// to a <see cref="System.Type"/>.
        /// </exception>
        public static Type Resolve(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw CreateTypeLoadException(typeName);
            }

            Type type;
            var  typeInfo = new TypeAssemblyInfo(typeName);
            try
            {
                type = GetTypeFromAssembly(typeInfo);
            }
            catch (Exception ex)
            {
                throw CreateTypeLoadException(typeName, ex);
            }
            if (type == null)
            {
                throw CreateTypeLoadException(typeName);
            }
            return type;
        }

        /// <summary>
        /// Gets an <b>System.Reflection.Assembly</b> and then the
        /// attendant <b>System.Type</b> referred to by the
        /// <paramref name="typeInfo"/> parameter.
        /// </summary>
        /// <param name="typeInfo">
        /// The assembly to be loaded.
        /// </param>
        /// <returns>
        /// A <b>System.Type</b>, or <see lang="null"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// <see cref="System.Reflection.Assembly.Load(AssemblyName)"/>
        /// </exception>
        private static Type GetTypeFromAssembly(TypeAssemblyInfo typeInfo)
        {
            Type         type         = null;
            string       assemblyName = typeInfo.AssemblyName.FullName == Assembly.GetExecutingAssembly().GetName().Name
                        ? typeof(TypeResolver).Assembly.FullName 
                        : typeInfo.AssemblyName.FullName;
            AssemblyName an     = assemblyName.Contains(",") 
                        ? new AssemblyName(assemblyName) 
                        : new AssemblyName { Name = assemblyName };

            Assembly assembly = Assembly.Load(an);
            if (assembly != null)
            {
                type = assembly.GetType(typeInfo.TypeName, true, true);
            }
            return type;
        }

        private static TypeLoadException CreateTypeLoadException(string typeName)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
        }

        private static TypeLoadException CreateTypeLoadException(string typeName, Exception ex)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
        }

        #region Inner Class: TypeAssemblyInfo

        /// <summary>
        /// Internal class used to hold data about a <b>System.Type</b>
        /// and  its containing assembly.
        /// </summary>
        internal class TypeAssemblyInfo
        {
            #region Properties

            /// <summary>
            /// The (unresolved) type name portion of the original type name.
            /// </summary>
            public string TypeName
            {
                get { return typeName; }
            }

            /// <summary>
            /// The name of the attandant assembly.
            /// </summary>
            public AssemblyName AssemblyName
            {
                get { return assemblyName; }
            }

            #endregion

            #region Fields

            private AssemblyName assemblyName;
            private string typeName;

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a new TypeAssemblyInfo class.
            /// </summary>
            /// <param name="assemblyQualifiedTypeName">
            /// The assembly qualified name of a <b>System.Type</b>.
            /// </param>
            public TypeAssemblyInfo(string assemblyQualifiedTypeName)
            {
                ParseTypeAndAssemblyName(assemblyQualifiedTypeName);
            }

            #endregion

            #region Methods

            private void ParseTypeAndAssemblyName(string assemblyQualifiedTypeName)
            {
                string[] parts = assemblyQualifiedTypeName.Split(',');
                if (parts.Length < 2)
                {
                    throw new ArgumentException("Both type and assembly name must be specified.");
                }
                typeName = parts[0].Trim();

                assemblyName = new AssemblyName {Name = parts[1].Trim()};
                for (int i = 2; i < parts.Length; i++)
                {
                    string[] pair = parts[i].Split('=');
                    SetNamePart(assemblyName, pair[0].Trim(), pair.Length > 1 ? pair[1].Trim() : null);
                }
            }

            private static void SetNamePart(AssemblyName an, string name, string value)
            {
                switch (name)
                {
                    case "Culture":
                        {
                            an.CultureInfo = value == "neutral" 
                                    ? CultureInfo.InvariantCulture
                                    : CultureInfo.CreateSpecificCulture(value);
                        }
                        break;

                    case "Version":
                        an.Version = new Version(value);
                        break;

                    case "PublicKeyToken":
                        if (value != "null")
                        {
                            an.SetPublicKeyToken(StringUtils.HexStringToByteArray(value));
                        }
                        break;
                }
            }

            #endregion

        }

        #endregion
    }
}