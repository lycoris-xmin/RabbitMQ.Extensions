using Lycoris.RabbitMQ.Extensions.Builder;
using Lycoris.RabbitMQ.Extensions.Models;
using Lycoris.RabbitMQ.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lycoris.RabbitMQ.Extensions.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMqApiService : IRabbitMqApiService
    {
        /// <summary>
        /// 
        /// </summary>
        private List<RabbitOption> _options { get => RabbitMQOptionsStore.GetAllRabbitMQOption(); }

        /// <summary>
        /// MQ监控
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<RabbitMqMonitorResponse> MonitorApiAsync(RabbitMqMonitorRequest input)
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{input.UserName}:{input.Passwrod}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var resp = await client.GetAsync($"http://{input.Host}:{input.Prot}/api/overview");

                if (resp == null || !resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"rabbit mq api request failed：{await resp?.Content?.ReadAsStringAsync() ?? ""} - {resp.StatusCode}");

                var res = await resp.Content.ReadAsStringAsync();

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<RabbitMqMonitorResponse>(res);

                result.Body = res;

                return result;
            }
        }
    }
}
