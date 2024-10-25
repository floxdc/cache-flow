# CacheFlow

CacheFlow is a cache management system for .Net Core. It helps you use cache and handle complex cases like value serialization, default values, and the Get or Set pattern.

### Table of Content
* [Quick Start](#quick-start)
* [Caching Strategies](#caching-strategies)
  * [In-Memory](#in-memory)
  * [Distributed](#distributed)
  * [Both In-Memory and Distributed](#both-in-memory-and-distributed)
* [Options](#options)
  * [Data Logging Levels](#data-logging-levels)
  * [Exception Suppression](#exception-suppression)
  * [Named Instances](#named-instances)
  * [Time Spans](#time-spans)
* [Extensions](#extensions)
  * [Serialization](#serialization)
    * [JSON](#json)
    * [MessagePack](#messagepack)
  * [Telemetry](#telemetry)


The library reduces boilerplate code and builds on standard .Net caching interfaces. Most methods have the same names and option types as regular caching services. It also adds convenience features like the `GetOrSet` method.

Use this:
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
This is the simplest example. The library also includes safety checks, serialization, logging, and other useful features.


## Quick Start

Install the package via [NuGet](https://www.nuget.org/packages/FloxDc.CacheFlow/)
```
PM> Install-Package FloxDc.CacheFlow -Version 1.13.0
``` 

Add the following lines to your `Startup.cs` file:
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
|GetOrSet       |Tries to get a value from the cache, and sets it if not found
|GetOrSetAsync  |Tries to get a value from the cache, and sets it if not found
|Remove         |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from the cache


### Distributed

#### DistributedFlow

|Method         |Description
|---------------|-
|GetAsync       |Gets a value from the cache
|GetOrSet       |Tries to get a value from the cache, and sets it if not found
|GetOrSetAsync  |Tries to get a value from the cache, and sets it if not found
|Refresh        |Refreshes a specified cache entry
|RefreshAsync   |Refreshes a specified cache entry
|Remove         |Removes a specified cache entry
|RemoveAsync    |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|SetAsync       |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from the cache


### Both In-Memory And Disitibuted 

For immutable data, you may want to cache it both distributed and in-memory. Use the `DoubleFlow` approach. 

**Warning!**
Note that some methods of `DoubleFlow` may return a `ValueTask` where `DistributedFlow` returns a `Task`.

#### DoubleFlow

|Method         |Description
|---------------|-
|GetAsync       |Gets a value from the cache
|GetOrSet       |Tries to get a value from the cache, and sets it if not found
|GetOrSetAsync  |Tries to get a value from the cache, and sets it if not found
|Refresh        |Refreshes a specified cache entry
|RefreshAsync   |Refreshes a specified cache entry
|Remove         |Removes a specified cache entry
|RemoveAsync    |Removes a specified cache entry
|Set            |Sets a cache entry with a provided value
|SetAsync       |Sets a cache entry with a provided value
|TryGetValue    |Tries to get a value from the cache


## Options

You can configure CacheFlow with the following options:

|Parameter              |Default    |Meaning
|-----------------------|-----------|-
|CacheKeyDelimiter      |::         |Delimiter used for key construction
|CacheKeyPrefix         |           |Prefix for all cache keys within an app
|DataLoggingLevel       |_Normal_   |Logging level for cache values and execution points (hit, miss, data calculation, etc.)
|SuppressCacheExceptions|_true_     |Suppresses exceptions caused by the caching service itself. Useful if your app can tolerate invalid cache requests.


#### Data Logging Levels

CacheFlow can produce different types of monitoring events:

|Level      |Behavior
|-----------|-
|_Disabled_ |Emits only tracing events
|_Normal_   |Traces events, logs operations, their result states, and cache keys
|_Sensitive_|Traces events, logs operations, their result states, cache keys, and _cached values_


#### Exception Suppression

**Warning!** 
By default, exception suppression is on and it may slow down your application.\
Turn off this option if you are confident in your caching system.


#### Named instances 

You could use typed service instances to autoprefix cache keys with the class name:
```csharp
public MyClass(IMemoryFlow<MyClass> cache)
```
This is useful if you want to find a key in a database.


#### Time Spans

To avoid overlaps in caching, you can use the following `TimeSpan` extensions:

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

By default, CacheFlow uses the `System.Text.Json serializer`. 
Add no extension methods during the configuration step to use it. 
There are two more pre-built options, and you can also implement your own serializer.


#### Json

A `Newtonsoft.Json` serializer.

Install the package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.Json -Version 1.13.0
``` 

Add the following lines to your configuration:
```csharp
services.AddMemoryFlow()
    .AddCacheFlowJsonSerialization();
```


#### MessagePack

A neuecc's `MessagePack` serializer.

Install the package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.MessagePack -Version 1.13.0
``` 

Add the following lines to `Startup.cs`:
```csharp
var messagePackOptions = MessagePackSerializerOptions.Standard;

services.AddMemoryFlow()
    .AddCacheFlowMessagePackSerialization(messagePackOptions, StandardResolver.Instance);
```

Note: `MessagePack` requires a structured map of serialized data


### Telemetry

The package supports .Net activity-based telemetry. 
Register a source from the `FloxDc.CacheFlow.Telemetry.ActivitySourceHelper` namespace to enable it. 
Here's an example of `OpenTelemetry` configuration with the source:

```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    // other configs
    .AddSource(ActivitySourceHelper.CacheFlowActivitySourceName)
    // other configs
    .Build();    
```