<%@ WebHandler Language="C#" Class="EndSession" %>

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

public class EndSession : AbstractSessionHandler, IReadOnlySessionState
{
    protected override String ProcessRequestInternal(HttpContext context)
    {
        Session.Abandon();
        return "Session terminated.";
    }
}
