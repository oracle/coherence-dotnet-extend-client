/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Oracle.MSBuild
{
    public class Sleep : Task
    {
        public override bool Execute()
        {
            Thread.Sleep(Timeout);
            return true;
        }

        [Required]
        public Int32 Timeout
        {
            get; set;
        }
    }
}
