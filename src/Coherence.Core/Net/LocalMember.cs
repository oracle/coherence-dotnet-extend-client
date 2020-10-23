/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Net;
using System.Text;
using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net
{
    /// <summary>
    /// Simple <see cref="IMember"/> implementation used as "local" member
    /// for <see cref="RemoteService"/> instances started by
    /// <see cref="IConfigurableCacheFactory"/>.
    /// </summary>
    /// <seealso cref="IMember"/>
    /// <author>Ana Cikic  2006.11.14</author>
    public class LocalMember : IMember, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The name of the cluster with which this member is associated.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual string ClusterName { get; set; }

        /// <summary>
        /// Determine the configured name for the site (such as a data
        /// center) in which this IMember resides.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// multiple geographic sites.
        /// </remarks>
        /// <returns>
        /// The configured site name or <c>null</c>.
        /// </returns>
        /// <since>Coherence 3.2</since>
        public virtual string SiteName
        {
            get { return m_siteName; }
            set { m_siteName = value; }
        }

        /// <summary>
        /// Determine the configured name for the rack (such as a physical
        /// rack, cage or blade frame) in which this IMember resides.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// multiple racks within a particular data center, for example.
        /// </remarks>
        /// <value>
        /// The configured rack name or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        public virtual string RackName
        {
            get { return m_rackName; }
            set { m_rackName = value; }
        }

        /// <summary>
        /// Determine the configured name for the machine (such as a host
        /// name) in which this IMember resides.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// multiple servers, and may be used as the basis for determining
        /// the MachineId property.
        /// </remarks>
        /// <value>
        /// The configured machine name or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        public virtual string MachineName
        {
            get { return m_machineName; }
            set { m_machineName = value; }
        }

        /// <summary>
        /// Determine the configured name for the process in which this
        /// IMember resides.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// multiple processes on a a single machine.
        /// </remarks>
        /// <value>
        /// The configured process name or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        public virtual string ProcessName
        {
            get { return m_processName; }
            set { m_processName = value; }
        }

        /// <summary>
        /// Determine the configured name for the IMember.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// members running within a particular process.
        /// </remarks>
        /// <value>
        /// The configured IMember name or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        public virtual string MemberName
        {
            get { return m_memberName; }
            set { m_memberName = value; }
        }

        /// <summary>
        /// Determine the configured role name for the IMember.
        /// </summary>
        /// <remarks>
        /// This role is completely definable by the application, and can be
        /// used to determine what members to use for specific purposes, such
        /// as to send particular types of work to.
        /// </remarks>
        /// <value>
        /// The configured role name for the IMember or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        public virtual string RoleName
        {
            get { return m_roleName; }
            set { m_roleName = value; }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns a string representation of the location information
        /// for this IMember object.
        /// </summary>
        /// <returns>
        /// A string representation of the location information for this
        /// IMember object.
        /// </returns>
        /// <since>Coherence 3.7</since>
        private string GetLocationInfo()
        {
            StringBuilder sb = new StringBuilder();

            string siteName = SiteName;
            if (!String.IsNullOrEmpty(siteName))
            {
                sb.Append(",site:").Append(siteName);
            }

            string rackName = RackName;
            if (!String.IsNullOrEmpty(rackName))
            {
                sb.Append(",rack:").Append(rackName);
            }

            string machineName = MachineName;
            if (!String.IsNullOrEmpty(machineName))
            {
                sb.Append(",machine:").Append(machineName);
            }

            string processName = ProcessName;
            if (!String.IsNullOrEmpty(processName))
            {
                sb.Append(",process:").Append(processName);
            }

            string memberName = MemberName;
            if (!String.IsNullOrEmpty(memberName))
            {
                sb.Append(",member:").Append(memberName);
            }

            return sb.Length == 0 ? "" : sb.ToString(1, sb.Length - 1);
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
            m_uuid        = (UUID) reader.ReadObject(0);
            ClusterName   = reader.ReadString(6);
            m_siteName    = reader.ReadString(7);
            m_rackName    = reader.ReadString(8);
            m_machineName = reader.ReadString(9);
            m_processName = reader.ReadString(10);
            m_memberName  = reader.ReadString(11); ;
            m_roleName    = reader.ReadString(12);
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
            writer.WriteObject(0, m_uuid);
            writer.WriteString(6, ClusterName);
            writer.WriteString(7, m_siteName);
            writer.WriteString(8, m_rackName);
            writer.WriteString(9, m_machineName);
            writer.WriteString(10, m_processName);
            writer.WriteString(11, m_memberName);
            writer.WriteString(12, m_roleName);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a string representation of this IMember object.
        /// </summary>
        /// <returns>
        /// A string representation of this IMember object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Member(");
            
            string location = GetLocationInfo();
            if (!String.IsNullOrEmpty(location))
            {
                sb.Append("Location=").Append(location);
            }

            string roleName = RoleName;
            if (!String.IsNullOrEmpty(roleName))
            {
                sb.Append(", Role=").Append(roleName);
            }

            sb.Append(')');
            return sb.ToString();
        }

        #endregion

        #region Data members

        private UUID   m_uuid = new UUID(DateTimeUtils.GetSafeTimeMillis(), NetworkUtils.GetLocalHostAddress(), 0, 0);
        private string m_roleName;
        private string m_memberName;
        private string m_processName;
        private string m_machineName;
        private string m_rackName;
        private string m_siteName;

        #endregion
    }
}