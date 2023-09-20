using System.Threading;

namespace Lycoris.RabbitMQ.Extensions.DataModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ListenResult
    {
        readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// CancellationToken
        /// </summary>
        public CancellationToken Token { get { return cancellationTokenSource.Token; } }

        /// <summary>
        /// 是否已停止
        /// </summary>
        public bool Stoped { get { return cancellationTokenSource.IsCancellationRequested; } }

        /// <summary>
        /// ctor
        /// </summary>
        public ListenResult()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
