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
    /// RabbitMQ 管理 API 服务，通过 RabbitMQ HTTP 管理接口查询节点状态和监控数据
    /// </summary>
    public sealed class RabbitMqApiService : IRabbitMqApiService
    {
        /// <summary>
        /// 获取所有已配置的 RabbitMQ 连接选项
        /// </summary>
        private List<RabbitOption> _options { get => RabbitMQOptionsStore.GetAllRabbitMQOption(); }

        /// <summary>
        /// 根据请求参数调用 RabbitMQ 管理 API 的 /api/overview 接口，返回节点监控概览
        /// </summary>
        /// <param name="input">监控请求参数（主机、端口、用户名、密码）</param>
        /// <returns>节点监控概览响应</returns>
        /// <exception cref="HttpRequestException">HTTP 请求失败时抛出</exception>
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
