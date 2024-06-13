/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System.Linq;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Tangosol.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{

    #region Constructors

    public SessionController(ILogger<SessionController> logger)
    {
        m_logger = logger;
    }

    #endregion

    [HttpGet("/read")]
    public IActionResult Read()
    {
        m_logger.LogInformation("read sessionId: " + HttpContext.Session.Id);
        StringBuilder sb = new StringBuilder();
        sb.Append(SMALL_KEY_1).Append(':').Append(HttpContext.Session.GetString(SMALL_KEY_1)).AppendLine();
        sb.Append(SMALL_KEY_2).Append(':').Append(HttpContext.Session.GetString(SMALL_KEY_2)).AppendLine();
        sb.Append(LARGE_KEY_1).Append(':').Append(HttpContext.Session.GetString(LARGE_KEY_1)).AppendLine();
        sb.Append(LARGE_KEY_2).Append(':').Append(HttpContext.Session.GetString(LARGE_KEY_2)).AppendLine();
        return Ok(sb.ToString());
    }

    #region Actions

    [HttpPost("/write-small")]
    public IActionResult WriteSmall()
    {
        m_logger.LogInformation("write-small sessionId: " + HttpContext.Session.Id);
        HttpContext.Session.SetString(SMALL_KEY_1, SMALL_VALUE_1);
        return Ok();
    }

    [HttpPost("/write-large-null")]
    public IActionResult WriteLargeNull()
    {
        m_logger.LogInformation("write-null sessionId: " + HttpContext.Session.Id);
        HttpContext.Session.SetString(SMALL_KEY_1, null);
        return Ok();
    }

    [HttpPost("/write-large1")]
    public IActionResult WriteLarge1()
    {
        HttpContext.Session.SetString(LARGE_KEY_1, LARGE_VALUE_1);
        return Ok();
    }

    [HttpPost("/write-large2")]
    public IActionResult WriteLarge2()
    {
        HttpContext.Session.SetString(LARGE_KEY_2, LARGE_VALUE_2);
        return Ok();
    }

    [HttpPost("/small-to-large")]
    public IActionResult SmallToLarge()
    {
        var session = HttpContext.Session;
        if (session.Keys.Contains(LARGE_KEY_1))
        {
            session.SetString(LARGE_KEY_1, LARGE_VALUE_1);
        }

        if (session.Keys.Contains(LARGE_KEY_2))
        {
            session.SetString(LARGE_KEY_2, LARGE_VALUE_2);
        }

        return Ok();
    }

    [HttpPost("/large-to-small")]
    public IActionResult LargeToSmall()
    {
        var session = HttpContext.Session;
        if (session.Keys.Contains(LARGE_KEY_1))
        {
            session.SetString(LARGE_KEY_1, SMALL_VALUE_1);
        }

        if (session.Keys.Contains(LARGE_KEY_2))
        {
            session.SetString(LARGE_KEY_2, SMALL_VALUE_2);
        }

        return Ok();
    }

    [HttpPost("/remove-large1")]
    public IActionResult RemoveLarge1()
    {
        HttpContext.Session.Remove(LARGE_KEY_1);
        return Ok();
    }

    [HttpPost("/remove-large2")]
    public IActionResult RemoveLarge2()
    {
        HttpContext.Session.Remove(LARGE_KEY_2);
        return Ok();
    }

    [HttpPost("/remove-small")]
    public IActionResult RemoveSmall()
    {
        HttpContext.Session.Remove(SMALL_KEY_1);
        return Ok();
    }

    [HttpPost("/clear")]
    public IActionResult Clear()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    #endregion

    #region Data members

    private ILogger<SessionController> m_logger;

    public static readonly string SMALL_KEY_1 = "small-key-1";
    public static readonly string SMALL_KEY_2 = "small-key-2";
    public static readonly string LARGE_KEY_1 = "large-key-1";
    public static readonly string LARGE_KEY_2 = "large-key-2";


    public static readonly string SMALL_VALUE_1 = "small-value-1";
    public static readonly string SMALL_VALUE_2 = "small-value-2";
    public static readonly string LARGE_VALUE_1 = new('1', 2000);
    public static readonly string LARGE_VALUE_2 = new('2', 2000);

    #endregion
}