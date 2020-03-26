<%@ Import Namespace="Tangosol.Net"%>
<%@ Application Language="C#" %>
<!--
  Copyright (c) 2000, 2020, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  http://oss.oracle.com/licenses/upl.
-->
<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        CacheFactory.Log("=== Application_Start ===", 0);
    }

    void Application_End(object sender, EventArgs e)
    {
        CacheFactory.Log("=== Application_End ===", 0);
    }

    void Application_Error(object sender, EventArgs e)
    {
        CacheFactory.Log("=== Application_Error ===", 0);
    }

    void Session_Start(object sender, EventArgs e)
    {
        CacheFactory.Log("=== Session_Start ===", 0);
    }

    void Session_End(object sender, EventArgs e)
    {
        CacheFactory.Log("=== Session_End ===", 0);
    }

</script>
