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
* [Extensions](#extensions)
  * [Serialization](#serialization)
    * [JSON](#json)
    * [MessagePack](#messagepack)
  * [Telemetry](#telemetry)


## Quick Start

Install a package via NuGet
```
PM> Install-Package FloxDc.CacheFlow -Version 1.7.0
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

The library builds on top of standard caching interfaces of .NetCore, so most of its methods have same names as regual caching services and use same option types.


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

When you work with immutable data you may want to cache it both distributed and local. There is a `DoubleFlow` approach for that case. Note some methods of `DoubleFlow` may return a `ValueTask` where `DistributedFlow` returns a `Task`


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

There is a set of options you can use to configure CacheFlow

|Parameter              |Default    |Meaning
|-----------------------|-----------|-
|CacheKeyDelimiter      |::         |Sets a delimiter which uses in key naming
|CacheKeyPrefix         |           |Sets a prefix to cache keys
|DataLoggingLevel       |_Normal_   |Sets a logging level of cache values and execution points
|SuppressCacheExceptions|_true_     |Enables suppression of throwing exceptions, caused by caching service itself


#### Data Logging Levels

The library can produce monitoring events of different types

|Level      |Behavior
|-----------|-
|_Disabled_ |Emits only tracing events
|_Normal_   |Traces events, logs operations, their result states, and cache keys
|_Sensitive_|Traces events, logs operations, their result states, cache keys, and _cached values_


#### Exception Suppression

|Warning!|
|--------|

By default exception supression is on and it _may slow down_ your application. Turn off the option if you confident in your caching system.


#### Named instances 

You could use typed service insances to autoprefix cache keys with the class name:
```csharp
public MyClass(IMemoryFlow<MyClass> cache)
```


## Extensions

### Serialization

By default CacheFlow uses the binary serializer which isn't suitable well for real world task, so I recommend to replace it with another one. There are two existing options, and also you could use your own implementation.


#### Json

A `Newtonsoft.Json` serializer.

Install a package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.Json -Version 1.7.0
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
PM> Install-Package FloxDc.CacheFlow.MessagePack -Version 1.7.0
``` 

And add following lines to `Startup.cs`:
```csharp
var messagePackOptions = MessagePackSerializerOptions.Standard;

services.AddMemoryFlow()
    .AddCacheFlowMessagePackSerialization(messagePackOptions, StandardResolver.Instance);
```

Keep in mind `MessagePack` requires to specify a structured map of serialized data.


### Telemetry

CacheFlow emits `DiagnosticSource` events and execution state logs. There is an integration with `OpenTelemerty` already in the place.

Install a package via NuGet
```
PM> Install-Package FloxDc.CacheFlow.OpenTelemerty -Version 1.7.1
``` 

```csharp
services.AddOpenTelemetry(builder =>
    {
        builder.UseJaeger(options =>
            {
                options.ServiceName = serviceName;
                options.AgentHost = agentHost;
                options.AgentPort = agentPort;
            })
            .AddCacheFlowInstrumentation()
            .SetResource(Resources.CreateServiceResource(serviceName))
            .SetSampler(new AlwaysOnSampler());
    });
```
