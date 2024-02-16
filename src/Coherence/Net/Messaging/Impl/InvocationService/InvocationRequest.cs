/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Net.Messaging.Impl.InvocationService
{
    /// <summary>
    /// The InvocationRequest is an <see cref="InvocationServiceRequest"/>
    /// sent to execute an <see cref="IInvocable"/> object on the server.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <seealso cref="InvocationServiceRequest"/>
    public class InvocationRequest : InvocationServiceRequest
    {
        #region Properties

        /// <summary>
        /// Return the type identifier for this <b>Message</b>.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        /// <seealso cref="Message.TypeId"/>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        /// <summary>
        /// The <see cref="IInvocable"/> task to execute.
        /// </summary>
        /// <value>
        /// The <b>IInvocable</b> task to execute.
        /// </value>
        public virtual IInvocable Task
        {
            get { return m_task; }
            set { m_task = value; }
        }

        #endregion

        #region IPortableObject implementation

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
        /// <seealso cref="Request.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(1, Task);
        }

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
        /// <seealso cref="Request.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Task = (IInvocable) reader.ReadObject(1);
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Task=" + Task;
        }
        
        #endregion

        #region Data members

        private IInvocable m_task;

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 1;

        #endregion
    }
}