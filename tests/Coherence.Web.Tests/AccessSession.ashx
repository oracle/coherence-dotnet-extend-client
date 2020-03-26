<%@ WebHandler Class="AccessSession" Language="C#" %>

/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;
using System.Text;
using System.Web;
using System.Web.SessionState;

using Tangosol.Web;

public class AccessSession : AbstractSessionHandler, IReadOnlySessionState
{
    protected override String ProcessRequestInternal(HttpContext context)
    {
        String[] attrNames = Request.QueryString.GetValues("name");
        StringBuilder sb = new StringBuilder();

        foreach (String name in attrNames)
        {
            sb.Append(name).Append(" = ").Append(Session[name]).AppendLine();
        }

        return sb.ToString();
    }
}