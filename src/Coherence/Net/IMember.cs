/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net
{
    /// <summary>
    /// The IMember interface represents a process connected to or running 
    /// within a cluster.
    /// </summary>
    /// <author>Gene Gleyzer  2002.02.08</author>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <since>Coherence 1.1</since>
    public interface IMember
    {
        /// <summary>
        /// The name of the cluster with which this member is associated.
        /// </summary>
        /// <since>12.2.1</since>
        string ClusterName { get; }

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
        string SiteName { get; set; }

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
        string RackName { get; set; }

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
        string MachineName { get; set; }

        /// <summary>
        /// Determine the configured name for the process (such as a JVM) in
        /// which this IMember resides.
        /// </summary>
        /// <remarks>
        /// This name is used for logging purposes and to differentiate among
        /// multiple processes on a a single machine.
        /// </remarks>
        /// <value>
        /// The configured process name or <c>null</c>.
        /// </value>
        /// <since>Coherence 3.2</since>
        string ProcessName { get; set; }

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
        string MemberName { get; set; }

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
        string RoleName { get; set; }
    }
}