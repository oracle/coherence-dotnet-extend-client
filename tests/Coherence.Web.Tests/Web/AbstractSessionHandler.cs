/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Web;
using System.Web.SessionState;

namespace Tangosol.Web
{
    public abstract class AbstractSessionHandler : WebTestUtil, IHttpHandler
    {
        protected abstract String ProcessRequestInternal(HttpContext context);

        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            Request  = context.Request;
            Response = context.Response;
            Session  = context.Session;

            try
            {
                String result = ProcessRequestInternal(context);
                Response.StatusCode  = SUCCESS;
                Response.ContentType = "text/plain";
                Response.Write(result);
            }
            catch (Exception e)
            {
                Response.StatusCode  = FAILURE;
                Response.ContentType = "text/plain";
                Response.Write("Request failed: " + e.Message);
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion

        protected const int SUCCESS = 200;
        protected const int FAILURE = 500;

        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public HttpSessionState Session { get; set; }
    }
}
