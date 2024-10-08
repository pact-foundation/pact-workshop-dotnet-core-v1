using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Text.Json;

namespace tests.Middleware
{
    public class ProviderStateMiddleware
    {

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RequestDelegate _next;

        private readonly IDictionary<string, Func<IDictionary<string, object>, Task>> _providerStates;

        public ProviderStateMiddleware(RequestDelegate next)
        {
            _next = next;
            _providerStates = new Dictionary<string, Func<IDictionary<string, object>, Task>>
            {

                ["There is no data"] = RemoveAllData,
                ["There is data"] = AddData
            };
        }

        private async Task RemoveAllData(IDictionary<string, object> parameters)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");
            var deletePath = Path.Combine(path, "somedata.txt");

            if (File.Exists(deletePath))
            {
                await Task.Run(() => File.Delete(deletePath));
            }
        }

        private async Task AddData(IDictionary<string, object> parameters)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../../../data");

            // Create the directory if it doesn't exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var writePath = Path.Combine(path, "somedata.txt");

            if (!File.Exists(writePath))
            {
                using (var fileStream = new FileStream(writePath, FileMode.CreateNew))
                {
                    await fileStream.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Handle the request
        /// </summary>
        /// <param name="context">Request context</param>
        /// <returns>Awaitable</returns>
        public async Task InvokeAsync(HttpContext context)
        {

            if (!(context.Request.Path.Value?.StartsWith("/provider-states") ?? false))
            {
                await this._next.Invoke(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;


            if (context.Request.Method == HttpMethod.Post.ToString().ToUpper())
            {
                string jsonRequestBody;

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    jsonRequestBody = await reader.ReadToEndAsync();
                }

                try
                {

                    ProviderState providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody, Options);

                    if (!string.IsNullOrEmpty(providerState?.State))
                    {
                        await this._providerStates[providerState.State].Invoke(providerState.Params);
                    }
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Failed to deserialise JSON provider state body:");
                    await context.Response.WriteAsync(jsonRequestBody);
                    await context.Response.WriteAsync(string.Empty);
                    await context.Response.WriteAsync(e.ToString());
                }
            }
        }
    }
}