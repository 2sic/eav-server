using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.Logging;

public class BootstrapMelLoggingTests
{
    [Fact]
    public void Bind_PublishesBootEventsOnce_BeforeAndAfterDi()
    {
        var bootstrap = new BootstrapMelLogging();
        var services = new ServiceCollection();
        services.AddSingleton<ILogFactory>(bootstrap.Factory);
        services.AddSysCoreMelInsightsLogging();
        using var provider = services.BuildServiceProvider();
        var log = BootLog.Start(bootstrap.Factory.Create("Sys.BootLog", null, new CodeRef()));
        log.A("before DI");
        log.Ex(new InvalidOperationException("pre-DI failure"));
        var call = log.Fn("crosses DI", timer: true);
        var beforeBinding = DateTime.UtcNow;

        Equal(0, bootstrap.Bind(provider.GetRequiredService<ILoggerFactory>()));
        call.Done();
        log.A("after DI");
        Equal(0, bootstrap.Bind(provider.GetRequiredService<ILoggerFactory>()));

        var group = Single(provider.GetRequiredService<IInsightsLogSnapshotReader>().ListGroups());
        Equal(["before DI", "after DI"], group.Events.Where(entry => entry.Message?.EndsWith(" DI") == true).Select(entry => entry.Message));
        Single(group.Events, entry => entry.Operation?.Contains("crosses DI") == true);
        Equal("boot-log", Single(group.Segments));
        True(group.Events[0].TimestampUtc <= beforeBinding);
        Contains(group.Events, entry => entry.Message == "Starting Boot Log");
        Contains(group.Events, entry => entry.Exception?.Type == typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public async Task Bind_KeepsConcurrentHandoffOrderedWithoutDuplicates()
    {
        var bootstrap = new BootstrapMelLogging();
        var log = bootstrap.Factory.Create("Sys.BootLog", null, new CodeRef());
        log.A("before");
        var host = new BlockingLoggerFactory();
        var binding = Task.Run(() => bootstrap.Bind(host));
        True(await Task.Run(() => host.Entered.Wait(TimeSpan.FromSeconds(5))));

        var concurrentWrite = Task.Run(() => log.A("during"));
        host.Release.Set();
        await Task.WhenAll(binding, concurrentWrite);
        log.A("after");

        Equal(["before", "during", "after"], host.Messages.ToArray());
    }

    [Fact]
    public void Bind_ReportsOverflow_AndRejectsAnotherHost()
    {
        var bootstrap = new BootstrapMelLogging();
        var log = bootstrap.Factory.Create("Sys.BootLog", null, new CodeRef());
        for (var index = 0; index < 4097; index++)
            log.A($"event {index}");
        using var host = new MelLogTests.RecordingLoggerFactory(true);

        var dropped = bootstrap.Bind(host);
        True(dropped > 0);
        Equal(4097, dropped + host.Entries.Count);
        Throws<InvalidOperationException>(() => bootstrap.Bind(new MelLogTests.RecordingLoggerFactory(true)));
    }

    [Fact]
    public void LinksAndStores_RejectMixedStacks()
    {
        using var host = new MelLogTests.RecordingLoggerFactory(true);
        var mel = new MelLogFactory(host).Create("Mel", null, new CodeRef());
        var legacy = new Log("Legacy");

        Throws<InvalidOperationException>(() => new Log("Child", mel));
        Throws<InvalidOperationException>(() => IsType<MelLog>(mel).Link(legacy));
        Throws<InvalidOperationException>(() => new MelLogStore().Add("boot-log", legacy));
        Throws<InvalidOperationException>(() => new LogStoreLive().Add("boot-log", mel));
    }

    private sealed class BlockingLoggerFactory : ILoggerFactory
    {
        internal readonly ManualResetEventSlim Entered = new();
        internal readonly ManualResetEventSlim Release = new();
        internal readonly ConcurrentQueue<string> Messages = new();
        private int _first;

        public ILogger CreateLogger(string categoryName) => new BlockingLogger(this);
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }

        private sealed class BlockingLogger(BlockingLoggerFactory owner) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (Interlocked.Exchange(ref owner._first, 1) == 0)
                {
                    owner.Entered.Set();
                    owner.Release.Wait(TimeSpan.FromSeconds(5));
                }
                owner.Messages.Enqueue(formatter(state, exception));
            }
        }

        private sealed class NullScope : IDisposable
        {
            internal static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
