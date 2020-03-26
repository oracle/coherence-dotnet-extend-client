/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Oracle.MSBuild
{
    public sealed class Run : Task
    {
        [Required]
        public string Command { get; set;}

        public string WorkingDirectory { get; set; }

        public bool UseShellExecute { get; set; }

        public bool WaitForExit { get; set; }

        public override bool Execute()
        {
            var startinfo = new ProcessStartInfo(Command)
                                {
                                        UseShellExecute = UseShellExecute,
                                        WorkingDirectory = WorkingDirectory
                                };

            Process p = Process.Start(startinfo);

            if (p != null && WaitForExit)
            {
                p.WaitForExit();
            }
            return p != null;
        }
    }
}
