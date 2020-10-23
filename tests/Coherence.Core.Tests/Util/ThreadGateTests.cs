/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;
using NUnit.Framework;

namespace Tangosol.Util
{
    [TestFixture]
    public class ThreadGateTests
    {
        /// <summary>
        /// This is basic single-thread test of ThreadGateSlim.
        /// </summary>
        [Test]
        public void ThreadGateTestSlim()
        {
            Gate gate = new ThreadGateSlim();
            Exception ex = null;

            Assert.IsNotNull(gate);

            Assert.IsFalse(gate.IsEnteredByCurrentThread);

            //gate is empty
            //cannot exit if not entered
            try
            {
                gate.Exit();
            }
            catch (Exception e)
            {
                ex = e;
                Assert.IsInstanceOf(typeof(SynchronizationLockException), e);
            }
            Assert.IsNotNull(ex);
            ex = null;

            //cannot open it if not closed
            try
            {
                gate.Open();
            }
            catch (Exception e)
            {
                ex = e;
                Assert.IsInstanceOf(typeof(SynchronizationLockException), e);
            }
            Assert.IsNotNull(ex);
            ex = null;

            //current thread enters the gate
            bool entered = gate.Enter(-1);
            Assert.IsTrue(entered);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            //enters again
            entered = gate.Enter(-1);
            Assert.IsTrue(entered);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            //exits one time
            gate.Exit();
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            gate.Exit();
            //closes the gate
            bool closed = gate.Close(-1);
            Assert.IsTrue(closed);
            Assert.IsFalse(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
          
            Assert.IsFalse(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
            //open gate
            gate.Open();
            //closes again
            closed = gate.Close(-1);
            Assert.IsTrue(closed);
            Assert.IsFalse(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
            //opens one time
            gate.Open();

            //closes the gate
            gate.Close(-1);
            //destroys the gate
            Assert.IsFalse(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
        }

        /// <summary>
        /// This is basic single-thread test of ThreadGate.
        /// </summary>
        [Test]
        public void ThreadGateTest()
        {
            Gate gate = new ThreadGate();
            Exception ex = null;

            Assert.IsNotNull(gate);

            Assert.IsFalse(gate.IsEnteredByCurrentThread);

            //gate is empty
            //cannot exit if not entered
            try
            {
                gate.Exit();
            }
            catch (Exception e)
            {
                ex = e;
                Assert.IsInstanceOf(typeof(SynchronizationLockException), e);
            }
            Assert.IsNotNull(ex);
            ex = null;

            //cannot open it if not closed
            try
            {
                gate.Open();
            }
            catch (Exception e)
            {
                ex = e;
                Assert.IsInstanceOf(typeof(SynchronizationLockException), e);
            }
            Assert.IsNotNull(ex);
            ex = null;

            //current thread enters the gate
            bool entered = gate.Enter(-1);
            Assert.IsTrue(entered);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            //enters again
            entered = gate.Enter(-1);
            Assert.IsTrue(entered);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            //exits one time
            gate.Exit();
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsFalse(gate.IsClosedByCurrentThread);
            //closes the gate
            bool closed = gate.Close(-1);
            Assert.IsTrue(closed);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);

            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
            //open gate
            gate.Open();
            //closes again
            closed = gate.Close(-1);
            Assert.IsTrue(closed);
            Assert.IsTrue(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
            //opens one time
            gate.Open();
            //opens two times
            gate.Exit();
            //closes the gate
            gate.Close(-1);
            //destroys the gate
            Assert.IsFalse(gate.IsEnteredByCurrentThread);
            Assert.IsTrue(gate.IsClosedByCurrentThread);
        }
    }
}