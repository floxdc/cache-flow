# CacheFlow

CacheFlow is a cache management system for .Net Core. It ables you not only use cache but handles various complex use cases i.e. value serialization, default values, the Get or Set pattern.


### Table of Content
* [Quick Start](#quick-start)
* [Caching strategies](#caching-strategies)
  * [In-Memory](#in-memory)
  * [Distributed](#distributed)
  * [Both In-Memory And Disitibuted ](#both-in-memory-and-disitibuted)
* [Options](#options)
  * [Data Logging Levels](#data-logging-levels)
  * [Exception Suppression](#exception-suppression)
  * [Named instances](#named-instances)
  * [Time Spans](#time-spans)
* [Extensions](#extensions)
  * [Serialization](#serialization)
    * [JSON](#json)
    * [MessagePack](#messagepack)
  * [Telemetry](#telemetry)


The library removes boilerplate code and builds on top of standard caching interfaces of .Net, so most of its methods have same names as regular caching services and use same option types. In addition to standard features it adds a little bit of comfort. For instance one may use the `GetOrSet` method instead of `Get` and `Set` separately.
Just use this:
```csharp
public T CacheFlowWay<T>(string key, TimeSpan expirationTime)
    => GetOrSet(key, CalculateResult, timeout)
```
instead of this:
```csharp
public T UsualWay<T>(string key, TimeSpan expirationTime)
{
    if (Cache.TryGetValue(key, out T result))
        return result;

    result = CalculateResult();

    Cache.Set(key, result, expirationTime);
    return result;
}
```
And that's the simpliest example. In addition the library contains safity checks, serialization, logging, and other handy features.


## Quick Start

Install a package via [NuGet](https://www.nuget.org/packages/FloxDc.CacheFlow/)
```
PM> Install-Package FloxDc.CacheFlow -Version 1.10.0
``` 

And add following lines to your `Startup.cs` file:
```csharp
services.AddMemoryCache()
    .AddStackExchangeRedisCache(options => 
    { 
        options.Configuration = Configuration["Redis:Endpoint"]; 
    })
    .AddDoubleFlow();
```


## Caching strategies

### In-Memory

#### MemoryFlow

|Method         |Description
|---------------|-
|GetOrSet       |Tries to get a value from a cache, and sets it if no entries were found
|GetOrSetAsync  |Tries to get a value from a cache, and sets it if no entries were found
|Remove         |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from a cache


### Distributed

#### DistributedFlow

|Method         |Description
|---------------|-
|GetAsync       |Gets a value from a cache
|GetOrSet       |Tries to get a value from a cache, and sets it if no entries were found
|GetOrSetAsync  |Tries to get a value from a cache, and sets it if no entries were found
|Refresh        |Refreshes a specified cache entry
|RefreshAsync   |Refreshes a specified cache entry
|Remove         |Removes a specified cache entry
|RemoveAsync    |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|SetAsync       |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from a cache


### Both In-Memory And Disitibuted 

When you work with immutable data you may want to cache it both distributed and in-memory. There is a `DoubleFlow` approach for that case. Note some methods of the `DoubleFlow` may return a `ValueTask` where the `DistributedFlow` returns a `Task`.


#### DoubleFlow

|Method         |Description
|---------------|-
|GetAsync       |Gets a value from a cache
|GetOrSet       |Tries to get a value from a cache, and sets it if no entries were found
|GetOrSetAsync  |Tries to get a value from a cache, and sets it if no entries were found
|Refresh        |Refreshes a specified cache entry
|RefreshAsync   |Refreshes a specified cache entry
|Remove         |Removes a specified cache entry
|RemoveAsync    |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|SetAsync       |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from a cache


## Options

There is a set of options you can use to configure CacheFlow:

|Parameter              |Default    |Meaning
|-----------------------|-----------|-
|CacheKeyDelimiter      |::         |Sets a delimiter which uses for key construction
|CacheKeyPrefix         |           |Sets a prefix to all cache keys within an app
|DataLoggingLevel       |_Normal_   |Sets a logging level of cache values and execution points, like hit, miss, data calculation etc.
|SuppressCacheExceptions|_true_     |Enables exception throwing suppression, for error caused by caching service itself. Suitable when your app tolerate for invalid cache requests.


#### Data Logging Levels

The library can produce monitoring events of different types:

|Level      |Behavior
|-----------|-
|_Disabled_ |Emits only tracing events
|_Normal_   |Traces events, logs operations, their result states, and cache keys
|_Sensitive_|Traces events, logs operations, their result states, cache keys, and _cached values_


#### Exception Suppression

**Warning!**

By default exception supression is on and it _may slow down_ your application. Turn off the option if you confident in your caching system.


#### Named instances 

You could use typed service insances to autoprefix cache keys with the class name:
```csharp
public MyClass(IMemoryFlow<MyClass> cache)
```
Useful if one want to find a key in a database.


#### Time Spans

If you want to avoid overlaps in caching, you may use following `TimeSpan` extensions:

|Method Name          |Time Frame|
|---------------------|----------|
|BeforeMinuteEnds     |up to 1 minute
|BeforeTwoMinutesEnd  |up to 2 minutes
|BeforeFiveMinutesEnd |up to 5 minutes
|BeforeTenMinutesEnd  |up to 10 minutes
|BeforeQuarterHourEnds|up to 15 minutes
|BeforeHalfHourEnds   |up to 30 minutes
|BeforeHourEnds       |up to 60 minutes
|BeforeDayEnds        |up to 24 hours


## Extensions

### Serialization

By default CacheFlow uses the `System.Text.Json` serializer. Add no extension metoods on a configuration step to use that method. There are two more pre-built options, and also you could implement your own serializer as well.


#### Json

A `Newtonsoft.Json` serializer.

Install a package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.Json -Version 1.10.0
``` 

And add following lines to your configuration:
```csharp
services.AddMemoryFlow()
    .AddCacheFlowJsonSerialization();
```


#### MessagePack

A neuecc's `MessagePack` serializer.

Install a package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.MessagePack -Version 1.10.0
``` 

And add following lines to `Startup.cs`:
```csharp
var messagePackOptions = MessagePackSerializerOptions.Standard;

services.AddMemoryFlow()
    .AddCacheFlowMessagePackSerialization(messagePackOptions, StandardResolver.Instance);
```

Keep in mind `MessagePack` requires to specify a structured map of serialized data.


### Telemetry

The package supports standard .Net activity-based telemetry. Register a source from a namespace `FloxDc.CacheFlow.Telemetry.ActivitySourceHelper` to enable it. Here's an example of `OpenTelemetry` configuration with the source:

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    // other configs
    .AddSource(ActivitySourceHelper.CacheFlowActivitySourceName)
    // other configs
    .Build();    
```