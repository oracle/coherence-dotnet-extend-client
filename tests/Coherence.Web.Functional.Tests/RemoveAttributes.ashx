<%@ WebHandler Class="RemoveAttributes" Language="C#" %>

/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;
using System.Web;
using System.Web.SessionState;

using Tangosol.Web;

public class RemoveAttributes : AbstractSessionHandler, IRequiresSessionState
{
    protected override String ProcessRequestInternal(HttpContext context)
    {
        String[] attrNames = Request.QueryString.GetValues("name");
        foreach (String name in attrNames)
        {
            Session.Remove(name);
        }

        return "Attribute(s) removed.";
    }
}
