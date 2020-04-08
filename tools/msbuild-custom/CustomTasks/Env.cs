/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Oracle.MSBuild
{
    public sealed class Env : Task
    {
        [Required]
        public string Variable
        {
            get; set;
        }

        [Required]
        public string Value
        {
            get; set;
        }

        public override bool Execute()
        {
            Environment.SetEnvironmentVariable(Variable, Value);
            Console.WriteLine("Set {0} to {1}", Variable, Value);
            return true;
        }
    }
}
