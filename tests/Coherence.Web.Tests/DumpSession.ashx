<%@ WebHandler Class="DumpSession" Language="C#" %>
/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Tangosol.Util;
using Tangosol.Web;

public class DumpSession : AbstractSessionHandler, IReadOnlySessionState
{
    protected override String ProcessRequestInternal(HttpContext context)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("INT")   .Append(" = ").Append(Session["INT"]).AppendLine();
        sb.Append("STRING").Append(" = ").Append(Session["STRING"]).AppendLine();
        sb.Append("DATE")  .Append(" = ").Append(Session["DATE"]).AppendLine();
        sb.Append("BOOL")  .Append(" = ").Append(Session["BOOL"]).AppendLine();
        sb.Append("BOOL")  .Append(" = ").Append(BlobToString((byte[]) Session["BLOB"])).AppendLine();
        sb.Append("COL")   .Append(" = ").Append(ColToString((IList) Session["COL"])).AppendLine();

        return sb.ToString();
    }

    private String ColToString(IList col)
    {
        return col == null ? null : col.Count + " x " + col[0];
    }

    private String BlobToString(byte[] blob)
    {
        return blob == null ? null : NumberUtils.ToHexEscape(blob);
    }
}
