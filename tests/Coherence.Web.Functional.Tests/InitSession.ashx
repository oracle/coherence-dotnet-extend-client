<%@ WebHandler Language="C#" Class="InitSession" %>

/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections;
using System.Web;
using System.Web.SessionState;
using Tangosol.Web;

public class InitSession : AbstractSessionHandler, IRequiresSessionState
{
    protected override String ProcessRequestInternal(HttpContext context)
    {
        foreach (String arg in Request.QueryString.AllKeys)
        {
            switch (arg.ToUpperInvariant())
            {
                case "INT":
                    {
                        Session["INT"] = Int32.Parse(Request.QueryString[arg]);
                        break;
                    }
                case "STRING":
                    {
                        Session["STRING"] = Request.QueryString[arg];
                        break;
                    }
                case "DATE":
                    {
                        Session["DATE"] = DateTime.Parse(Request.QueryString[arg]);
                        break;
                    }
                case "BOOL":
                    {
                        Session["BOOL"] = Boolean.Parse(Request.QueryString[arg]);
                        break;
                    }
                case "BLOB":
                    {
                        int size = Int32.Parse(Request.QueryString[arg]);
                        Session["BLOB"] = CreateBlob(size);
                        break;
                    }
                case "COL":
                    {
                        int size = Int32.Parse(Request.QueryString[arg]);
                        IList col = new ArrayList(size);
                        for (int i = 0; i < size; i++)
                        {
                            col.Add(CreatePerson());
                        }
                        Session["COL"] = col;
                        break;
                    }
                default:
                    break;
            }
        }
        return "Session initialized.";
    }
}
