using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using ASPNetCore_WebAccess; // WebAccessRun sınıfının bulunduğu namespace

namespace YildizOtoMasyonKart.Services
{
    public class BackgroundServiceExample : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundServiceExample(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1)); // 1 dakikada bir çalışır
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var context = _httpContextAccessor.HttpContext;
            WebAccessRun webAccess_Run = new WebAccessRun(context);
            webAccess_Run.WebAccess.GetResponse();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
