# Outrage Futures

Provides a hosted service that is able to process asynchronous calls in the future, optionally at some set later time.

# Getting started

* Install the Outrage.Futures package.

* Build a service to call in the future and register it with dependency injection.

* Call services.AddFutures() to register the hosted service.

Note: AddFutures uses a hosted service, if you dont use a host builder you should start the background service yourself.  An example is in the test cases.

# Build a service

A service as a minimum needs to be registered with dependencyu injection, and it needs a method on it.

The service method can be synchronous as asynchronous, but if you want to be able to locate the result of the method at some later stage (other than in logs) you should return `Task<ITaskResult>`

The simplest asynchronous future service:

```c#
namespace SomeNamespace;

public class AsyncTaskService {
	public Task ExecuteAsync() {
		Console.WriteLine("Task was completed.");
		return Task.CompletedTask;
	}
}
```

# Call the future

To call the future once it is registered, get an instance of IFuturesService and call AddFuture:

```c#
var futuresService = serviceProvider.GetRequiredService<IFuturesService>();
await futuresService.AddFuture<AsyncTaskService>("ExecuteAsync");
```

Although we are awaiting AddFuture, it will return immediately and the task performed later, and in the background.

# Pass parameters

Imagine the ExecuteAsync method received an int, as in `public Task ExecuteAsync(int number)`.

```c#
var futuresService = serviceProvider.GetRequiredService<IFuturesService>();
await futuresService.AddFuture<AsyncTaskService>("ExecuteAsync", new object[] { someValue });
```

# Delay it until later

You can also ask the futures processor to delay execution until some later time.

```c#
var futuresService = serviceProvider.GetRequiredService<IFuturesService>();
await futuresService.AddFuture<AsyncTaskService>("ExecuteAsync", scheduledTime: DateTimeOffset.UtcNow.AddMinutes(10));
```

# Failed, retry

If your future fails, it will automatically retry itself every 20 seconds, until the retrycount you passed to AddFutures is exhausted.

```c#
services.AddFutures(100); // Retry up to 100 times.
```

To avoid retries you can either pass 0 to the AddFutures method (the default is 10) or your service can wrap any exception it generated in a NoRetryException and throw that instead.

# More persistent persistance

The default persistence mechanism in thread safe but in memory only, and it does not retain the contents of the ITaskResult objects that the methods return.
An alternate persistence layer can me implemented and registered using the IFuturesStorage version of AddFutures.
This would allow you to:
* Retain persistent futures between restarts
* Store future call results for later analysis

```c#
services.AddFutures<PostgresqlFuturesStorageService>();
```

# Periodicity

If you need a future to happen continuously, get it to add itself to the future queue at the end.  These calls in your future method might be, to recall itself in 10 minutes:

```c#
var futuresService = serviceProvider.GetRequiredService<IFuturesService>();
await FuturesService<Myself>("MyMethodAsync", new object[] { runCycle + 1}, scheduledTime: DateTimeOffset.UtcNow.AddMinutes(10));
```

It is tempting to just inject thousands of futures with different scheduled times.  Resist the urge.
