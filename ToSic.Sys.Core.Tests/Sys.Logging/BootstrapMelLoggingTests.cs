using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
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
    public void Bind_ReplaysCompleteMessagesPropertiesAndSpecs()
    {
        var bootstrap = new BootstrapMelLogging();
        var message = new string('m', 9000);
        var url = "https://example.test/?value=" + new string('u', 9000);
        var largeSpec = new string('s', 9000);
        var specs = Enumerable.Range(0, 130).ToDictionary(index => $"Key{index}", index => $"Value{index}");
        specs["Large"] = largeSpec;
        var state = new List<KeyValuePair<string, object?>>
        {
            new("Message", message), new("Url", url), new("Specs", specs)
        };
        var store = new InsightsLogStore();
        using var host = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new InsightsLoggerProvider(store));
        });

        bootstrap.CreateLogger("ToSic.Boot").Log(LogLevel.Trace, default, state, null,
            static (values, _) => (string)values[0].Value!);
        state[0] = new("Message", "changed");
        specs["Key0"] = "changed";
        Equal(0, bootstrap.Bind(host));

        var entry = Single(store.List());
        Equal(message, entry.Message);
        Equal(message, entry.Properties["Message"]);
        Equal(url, entry.Properties["Url"]);
        Equal(131, entry.Specs.Count);
        Equal("Value0", entry.Specs["Key0"]);
        Equal(largeSpec, entry.Specs["Large"]);
    }

    [Fact]
    public void Bind_ReplaysCompleteNestedExceptionDiagnostic()
    {
        var bootstrap = new BootstrapMelLogging();
        var outer = new string('o', 9000);
        var inner = new string('i', 9000);
        var data = new string('d', 9000);
        var exception = Throws<InvalidOperationException>((Action)(() => throw new InvalidOperationException(outer, new ArgumentException(inner))));
        exception.Data["Payload"] = data;
        exception.InnerException!.Data["Code"] = 42;
        var details = exception.ToString();
        var stackTrace = exception.StackTrace;
        var store = new InsightsLogStore();
        using var host = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new InsightsLoggerProvider(store));
        });

        bootstrap.CreateLogger("ToSic.Boot").Log(LogLevel.Error, default,
            new[] { new KeyValuePair<string, object?>("Message", "failed") }, exception,
            static (_, _) => "failed");
        exception.Data["Payload"] = "changed";
        exception.InnerException.Data["Code"] = "changed";
        Equal(0, bootstrap.Bind(host));

        var diagnostic = Single(store.List()).Exception!;
        Equal(typeof(InvalidOperationException).FullName, diagnostic.Type);
        Equal(outer, diagnostic.Message);
        Equal(stackTrace, diagnostic.StackTrace);
        Equal(details, diagnostic.Details);
        Equal(data, diagnostic.Data["Payload"]);
        Equal(typeof(ArgumentException).FullName, diagnostic.InnerException!.Type);
        Equal(inner, diagnostic.InnerException.Message);
        Equal("42", diagnostic.InnerException.Data["Code"]);
    }

    [Fact]
    public void Bind_PassesDetachedExceptionToHostAndLiveExceptionAfterBind()
    {
        var bootstrap = new BootstrapMelLogging();
        var logger = bootstrap.CreateLogger("ToSic.Boot");
        var before = new InvalidOperationException("before", new ArgumentException("inner"));
        before.Data["Code"] = 42;
        logger.LogError(before, "before");
        using var host = new MelLogTests.RecordingLoggerFactory(true);

        Equal(0, bootstrap.Bind(host));
        var replayed = Single(host.Entries).Exception!;
        var after = new InvalidOperationException("after");
        logger.LogError(after, "after");

        NotSame(before, replayed);
        Equal(before.Message, replayed.Message);
        Equal(before.ToString(), replayed.ToString());
        Equal("inner", replayed.InnerException?.Message);
        Equal("42", replayed.Data["Code"]);
        Same(after, host.Entries[1].Exception);
    }

    [Fact]
    public void Bind_DropsOneOversizedEventWithoutEvictingEarlierEvents()
    {
        var bootstrap = new BootstrapMelLogging();
        var log = bootstrap.Factory.Create("Sys.BootLog", null, new CodeRef());
        log.A("before");
        log.A(new string('x', 1024 * 1024 + 1));
        log.A("after");
        using var host = new MelLogTests.RecordingLoggerFactory(true);

        Equal(1, bootstrap.Bind(host));
        Equal(["before", "after"], host.Entries.Select(entry => entry.Value("Message")));
    }

    [Fact]
    public void Bind_EvictsWholeOldestEventWhenByteBudgetIsFull()
    {
        var bootstrap = new BootstrapMelLogging();
        var log = bootstrap.Factory.Create("Sys.BootLog", null, new CodeRef());
        log.A(new string('a', 300_000));
        var latest = new string('b', 300_000);
        log.A(latest);
        using var host = new MelLogTests.RecordingLoggerFactory(true);

        Equal(1, bootstrap.Bind(host));
        Equal(latest, Single(host.Entries).Value("Message"));
    }

    [Fact]
    public void Write_DoesNotRetainLiveExceptionOrDataObjectsBeforeBind()
    {
        var bootstrap = new BootstrapMelLogging();
        var (exception, payload) = WriteTemporaryException(bootstrap);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        False(exception.IsAlive);
        False(payload.IsAlive);
        GC.KeepAlive(bootstrap);
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Exception, WeakReference Payload) WriteTemporaryException(BootstrapMelLogging bootstrap)
    {
        var payload = new object();
        var exception = new InvalidOperationException("temporary");
        exception.Data["Payload"] = payload;
        bootstrap.CreateLogger("ToSic.Boot").LogError(exception, "temporary");
        return (new(exception), new(payload));
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
