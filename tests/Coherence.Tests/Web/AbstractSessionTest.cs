/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Tangosol.Net;
using Tangosol.Web.Controllers;

namespace Tangosol.Web;

public abstract class AbstractSessionTest
{
    [SetUp]
    public void SetUp()
    {
        TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
    }

    [TearDown]
    public void TearDown()
    {
        TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
    }

    #region Tests

    protected async Task TestWithClient(CookieHttpClient client, int existingSessions, int existingOverflowAttrs, bool split)
    {
        int sessions = existingSessions;
        int overflowAttrs = existingOverflowAttrs;
        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-small"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(++sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-small"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
                { { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 } }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-large1"))
        {
            if (split)
            {
                overflowAttrs++;
            }

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.LARGE_VALUE_1 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-large2"))
        {
            if (split)
            {
                overflowAttrs++;
            }

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.LARGE_VALUE_1 },
                { SessionController.LARGE_KEY_2, SessionController.LARGE_VALUE_2 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-large2"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.LARGE_VALUE_1 },
                { SessionController.LARGE_KEY_2, SessionController.LARGE_VALUE_2 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/large-to-small"))
        {
            overflowAttrs = existingOverflowAttrs;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_2, SessionController.SMALL_VALUE_2 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/small-to-large"))
        {
            if (split)
            {
                overflowAttrs = existingOverflowAttrs + 2;
            }

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.LARGE_VALUE_1 },
                { SessionController.LARGE_KEY_2, SessionController.LARGE_VALUE_2 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/remove-large1"))
        {
            if (split)
            {
                overflowAttrs--;
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_2, SessionController.LARGE_VALUE_2 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/remove-large2"))
        {
            if (split)
            {
                overflowAttrs--;
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/remove-small"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }


        using (var response = await client.PostAsync("http://localhost/write-small"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/write-large1"))
        {
            if (split)
            {
                overflowAttrs++;
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(new Dictionary<string, string>
            {
                { SessionController.SMALL_KEY_1, SessionController.SMALL_VALUE_1 },
                { SessionController.LARGE_KEY_1, SessionController.LARGE_VALUE_1 }
            }), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.PostAsync("http://localhost/clear"))
        {
            if (split)
            {
                overflowAttrs--;
            }
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }

        using (var response = await client.GetAsync("http://localhost/read"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
            string output = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(ExpectedOutput(), output);
            Assert.AreEqual(sessions, SessionCache.Count);
            Assert.AreEqual(overflowAttrs, OverflowAttrCache.Count);
        }
    }

    #endregion

    #region Helper methods

    protected string ExpectedOutput()
    {
        return ExpectedOutput(null);
    }

    protected string ExpectedOutput(Dictionary<string, string> map)
    {
        map ??= new Dictionary<string, string>();
        StringBuilder expected = new StringBuilder();
        expected.Append(SessionController.SMALL_KEY_1).Append(':');
        expected.Append(map.GetValueOrDefault(SessionController.SMALL_KEY_1, ""));

        expected.AppendLine();
        expected.Append(SessionController.SMALL_KEY_2).Append(':');
        expected.Append(map.GetValueOrDefault(SessionController.SMALL_KEY_2, ""));

        expected.AppendLine();
        expected.Append(SessionController.LARGE_KEY_1).Append(':');
        expected.Append(map.GetValueOrDefault(SessionController.LARGE_KEY_1, ""));

        expected.AppendLine();
        expected.Append(SessionController.LARGE_KEY_2).Append(':');
        expected.Append(map.GetValueOrDefault(SessionController.LARGE_KEY_2, ""));
        expected.AppendLine();

        return expected.ToString();
    }

    #endregion

    #region Properties

    protected static INamedCache SessionCache { get; set; }

    protected static INamedCache OverflowAttrCache { get; set; }

    #endregion

    #region Inner class: CookieHttpClient

    /// <summary>
    /// Incomplete http client with cookie support based on <see cref="HttpClient"/>
    /// </summary>
    public class CookieHttpClient : IDisposable
    {
        #region Constructors

        public CookieHttpClient(HttpClient client)
        {
            m_client    = client;
            m_container = new CookieContainer();
        }
        #endregion

        #region HttpClient

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);

            CookieCollection collection = m_container.GetCookies(uri);
            if (collection.Count > 0)
            {
                request.Headers.Add("Cookie", GetCookieStrings(collection));
            }

            var response = await m_client.SendAsync(request);

            if (response.Headers.Contains("Set-Cookie"))
            {
                foreach (string s in response.Headers.GetValues("Set-Cookie"))
                {
                    m_container.SetCookies(uri, s);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            Console.WriteLine("url: " + uri);
            return await SendAsync(HttpMethod.Get, new Uri(uri));
        }

        public async Task<HttpResponseMessage> PostAsync(string uri)
        {
            return await SendAsync(HttpMethod.Post, new Uri(uri));
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            m_client.Dispose();
        }

        #endregion

        private static HttpContent GetHttpContent(object content)
        {

            HttpContent httpContent =
                new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(content, content.GetType()));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpContent;
        }

        private static IEnumerable<string> GetCookieStrings(CookieCollection collection)
        {
            List<string> output = new List<string>(collection.Count);
            foreach (Cookie cookie in collection)
            {
                output.Add(cookie.Name + "=" + cookie.Value);
            }

            return output;
        }

        #region Data members

        private HttpClient      m_client;
        private CookieContainer m_container;

        #endregion
    }
    #endregion
}