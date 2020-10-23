<%@ Page Language="C#" %>
<%@ Import Namespace="Tangosol.Net"%>
<%@ Import Namespace="Tangosol.IO.Resources"%>
<%@ Import Namespace="System.Web.Hosting"%>

<!--
  Copyright (c) 2000, 2020, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  http://oss.oracle.com/licenses/upl.
-->

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    private INamedCache cache = CacheFactory.GetCache("dist-dummy");
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Resources</title>
</head>
<body style="font-family: Verdana">
    <form id="form1" runat="server">
    <div>
        <%= AppDomain.CurrentDomain.BaseDirectory %>
        <table border="1">
            <tr>
                <td><b>Resource</b></td>
                <td><b>Absolute Path</b></td>
            </tr>
            <tr>
                <td>file://resource.xml</td>
                <td><%= ResourceLoader.GetResource("file://resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>file://../resource.xml</td>
                <td><%= ResourceLoader.GetResource("file://../resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>file://../../resource.xml</td>
                <td><%= ResourceLoader.GetResource("file://../../resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>resource.xml</td>
                <td><%= ResourceLoader.GetResource("resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>web://resource.xml</td>
                <td><%= ResourceLoader.GetResource("web://resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>~/resource.xml</td>
                <td><%= ResourceLoader.GetResource("~/resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>web://~/resource.xml</td>
                <td><%= ResourceLoader.GetResource("web://~/resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>../Config/resource.xml</td>
                <td><%= ResourceLoader.GetResource("../Config/resource.xml").AbsolutePath %></td>
            </tr>
            <tr>
                <td>web://../Config/resource.xml</td>
                <td><%= ResourceLoader.GetResource("web://../Config/resource.xml").AbsolutePath %></td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
