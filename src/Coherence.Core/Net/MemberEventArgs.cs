/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Text;

namespace Tangosol.Net
{
    /// <summary>
    /// An event which indicates that membership has changed.
    /// </summary>
    /// <remarks>
    /// Possible event types are:
    /// <list type="bullet">
    /// <item>
    /// An <see cref="IMember"/> has joined.
    /// </item>
    /// <item>
    /// An <see cref="IMember"/> is leaving.
    /// </item>
    /// <item>
    /// An <see cref="IMember"/> has left.
    /// </item>
    /// </list>
    /// <p>
    /// A MemberEventArgs object is sent as an argument to the
    /// <see cref="MemberEventHandler"/> methods.</p>
    /// </remarks>
    /// <author>Cameron Purdy  2002.12.12</author>
    /// <author>Ana Cikic  2006.11.08</author>
    public class MemberEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="IService"/> object that fired the event.
        /// </summary>
        /// <value>
        /// An object on which this event has occured.
        /// </value>
        public virtual IService Service
        {
            get { return m_source; }
        }

        /// <summary>
        /// Gets this event's type.
        /// </summary>
        /// <remarks>
        /// The event type is one of the <see cref="MemberEventType"/>
        /// enumerated constants.
        /// </remarks>
        /// <value>
        /// An event type.
        /// </value>
        public virtual MemberEventType EventType
        {
            get { return m_eventType; }
        }

        /// <summary>
        /// Gets the <see cref="IMember"/> associated with this event.
        /// </summary>
        /// <value>
        /// An <b>IMember</b>.
        /// </value>
        public virtual IMember Member
        {
            get { return m_member; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new MemberEventArgs.
        /// </summary>
        /// <param name="source">
        /// The source object that fired the event (a
        /// <see cref="IService"/>).
        /// </param>
        /// <param name="eventType">
        /// This event's type.
        /// </param>
        /// <param name="member">
        /// The <see cref="IMember"/> for which the event applies.
        /// </param>
        public MemberEventArgs(IService source, MemberEventType eventType, IMember member)
        {
            m_source    = source;
            m_eventType = eventType;
            m_member    = member;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a string representation of this MemberEventArgs object.
        /// </summary>
        /// <returns>
        /// A string representation of this MemberEventArgs object.
        /// </returns>
        public override string ToString()
        {
            IMember member = Member;
            StringBuilder sb = new StringBuilder("MemberEventArgs{Member=");

            sb.Append(member == null ? "Local" : member.ToString())
                .Append(' ')
                .Append(DESCRIPTIONS[(int) EventType])
                .Append(' ')
                .Append(Service.GetType().Name)
                .Append('}');

             return sb.ToString();
        }

        #endregion

        #region Data members

        /// <summary>
        /// IService object that fired the event.
        /// </summary>
        private IService m_source;

        /// <summary>
        /// This event's type.
        /// </summary>
        private MemberEventType m_eventType;

        /// <summary>
        /// Gets an IMember associated with this event.
        /// </summary>
        private IMember m_member;

        /// <summary>
        /// Descriptions of the various event types.
        /// </summary>
        private static readonly string[] DESCRIPTIONS = {"<unknown>", "JOINED", "LEAVING", "LEFT"};

        #endregion
    }

    #region Enum: MemberEventType

    /// <summary>
    /// Member event type enumeration.
    /// </summary>
    public enum MemberEventType
    {
        /// <summary>
        /// This event indicates that an <b>IMember</b> has joined.
        /// </summary>
        Joined = 1,

        /// <summary>
        /// This event indicates that an <b>IMember</b> is leaving.
        /// </summary>
        Leaving = 2,

        /// <summary>
        /// This event indicates that an <b>IMember</b> has left.
        /// </summary>
        Left = 3
    }

    #endregion
}
