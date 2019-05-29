using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace PartsUnlimited.BackgroundService
{
    public abstract class ScopedProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedProcessor(IServiceScopeFactory serviceScopeFactory) : base()
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task<Task> Process()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await ProcessInScope(scope.ServiceProvider);
            }
            return Task.CompletedTask;
        }

        public abstract Task<Task> ProcessInScope(IServiceProvider serviceProvider);
    }
}
