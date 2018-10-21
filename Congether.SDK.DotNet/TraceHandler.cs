using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Congether.SDK.DotNet
{
    /// <summary>
    /// SDK for Tracer-Module
    /// </summary>
    public class TraceHandler
    {
        private bool queueRunning = false;
        private const int maxQueueLength = 100;
        private DateTime? lastSent = null;
        private DateTime? lastLog = null;
        private object _lockObj = new object();
        private Random rnd = new Random();
        private List<EndpointMessageQueue> _pendingQueues = new List<EndpointMessageQueue>();
        private EndpointMessageQueue _currentQueue;
        CongetherClient client = null;
        internal TraceHandler(CongetherClient client)
        {
            this.client = client;
            var timer = new System.Timers.Timer(10000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WorkQueue();
        }

        /// <summary>
        /// Write Log-Entry to Tracer
        /// </summary>
        /// <param name="log">Log-message</param>
        /// <param name="timestamp">Optional timestamp of log. If empty, current DateTime will be used.</param>
        /// <returns>Async Task</returns>
        public async Task Log(EndpointLogEvent log, DateTime? timestamp = null)
        {
            if (!timestamp.HasValue) timestamp = DateTime.Now;

            var queue = await GetCurrentQueue();
            queue.Content.Add(new EndpointMessage() { Timestamp = timestamp.Value.ToUniversalTime(), LogEvent = log });
            if(queue.Content.Count > maxQueueLength)
            {
                WorkQueue();
            }
        }

        /// <summary>
        /// Write Metric-Entry to Tracer
        /// </summary>
        /// <param name="metric">Metric-Message</param>
        /// <param name="timestamp">Optional timestamp of log. If empty, current DateTime will be used.</param>
        /// <returns>Async Task</returns>
        public async Task Metric(EndpointMetricEvent metric, DateTime? timestamp = null)
        {
            if (!timestamp.HasValue) timestamp = DateTime.Now;

            var queue = await GetCurrentQueue();
            queue.Content.Add(new EndpointMessage() { Timestamp = timestamp.Value.ToUniversalTime(), MetricEvent = metric });
            if (queue.Content.Count > maxQueueLength)
            {
                WorkQueue();
            }
        }

        private async Task<EndpointMessageQueue> GetCurrentQueue()
        {
            if(_currentQueue == null)                
            {
                lock (_lockObj)
                {
                    _currentQueue = new EndpointMessageQueue();
                    _currentQueue.Content = new System.Collections.ObjectModel.ObservableCollection<EndpointMessage>();
                }
            }
            return _currentQueue;
        }

        private async Task WorkQueue()
        {
            if (queueRunning)
                return;

            queueRunning = true;

            foreach(var pending in _pendingQueues)
            {
                await SendQueue(pending);
            }

            if (_currentQueue != null)
            {
                var queueToSend = _currentQueue;
                lock (_lockObj)
                {
                    _currentQueue = null;
                }
                queueToSend.Rand = this.rnd.Next(100, 9999999).ToString();
                queueToSend.Endpoint = await this.client.GetEndpointInfo();

                await SendQueue(queueToSend);
            }

            queueRunning = false;
            
        }

        private async Task SendQueue(EndpointMessageQueue queueToSend)
        {
            try
            {
                await this.client.SendQueue(queueToSend);
                if (this._pendingQueues.Contains(queueToSend))
                    this._pendingQueues.Remove(queueToSend);
            }
            catch (Exception ex)
            {
                if (!this._pendingQueues.Contains(queueToSend))
                    this._pendingQueues.Add(queueToSend);
            }
        }

    }
}
