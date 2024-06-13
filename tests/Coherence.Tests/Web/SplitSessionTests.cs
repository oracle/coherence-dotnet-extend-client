/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Tangosol.Net;

namespace Tangosol.Web;

[TestFixture]
public class SplitSessionTests : AbstractSessionTest
{
    #region Setup and Teardown

    [OneTimeSetUp]
    public static void InitializeSessionStore()
    {
        var options = new CoherenceSessionOptions();
        var ctx = new DefaultOperationalContext(options.CoherenceConfig);
        var ccf = new DefaultConfigurableCacheFactory(options.CacheConfig);
        ccf.OperationalContext = ctx;

        SessionCache      = ccf.EnsureCache(SESSION_CACHE_NAME);
        OverflowAttrCache = ccf.EnsureCache(OVERFLOW_ATTR_CACHE_NAME);
    }

    [SetUp]
    public void Setup()
    {
        SessionCache.Clear();
        OverflowAttrCache.Clear();
    }

    #endregion

    #region Tests

    [Test]
    public async Task TestSplitSession()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.ConfigureLogging(conf => { conf.AddSystemdConsole(); });
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddHttpLogging(_ => { });
                    services.AddControllers();
                    services.AddEndpointsApiExplorer();

                    services.UseCoherenceSession(options =>
                    {
                        options.SessionType       = CoherenceSessionOptions.HttpSessionType.Split;
                    });

                    // services.AddRouting();
                    services.AddSession(options =>
                    {
                        options.IdleTimeout        = TimeSpan.FromSeconds(60);
                        options.Cookie.HttpOnly    = true;
                        options.Cookie.IsEssential = true;
                    });
                });
                webHost.Configure(app =>
                {
                    app.UseHttpLogging();
                    app.UseAuthorization();
                    app.UseSession();
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                });
            });
        var host = await hostBuilder.StartAsync();
        var client1 = new CookieHttpClient(host.GetTestClient());
        await TestWithClient(client1, 0, 0, true);
        await WriteOverflowAttributes(client1);
        var client2 = new CookieHttpClient(host.GetTestClient());
        await TestWithClient(client2, 1, 2, true);
    }

    #endregion

    #region Helper methods

    protected async Task WriteOverflowAttributes(CookieHttpClient client)
    {
        using (var response = await client.PostAsync("/write-large1"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(1, SessionCache.Count);
            Assert.AreEqual(1, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("/write-large2"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(1, SessionCache.Count);
            Assert.AreEqual(2, OverflowAttrCache.Count);
        }
    }

    #endregion

    #region Data members

    private const string SESSION_CACHE_NAME = "aspnet-session-storage";
    private const string OVERFLOW_ATTR_CACHE_NAME = "aspnet-session-overflow";

    #endregion
}