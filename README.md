# RedisClient
#### Another .NET Redis client, for the craic.
##### Status: ALPHA 
[![Build status](https://ci.appveyor.com/api/projects/status/we9e8or6ajlm72pu/branch/master?svg=true)](https://ci.appveyor.com/project/vtortola/redisclient/branch/master)

An experimental .NET Redis client that uses a special syntax for easing LUA script invocation named ["procedures"](//github.com/vtortola/RedisClient/wiki/Procedures). The interface is based on templated strings, allowing to execute custom defined server side procedures as regular Redis commands. Executions are done through "channels", in essence virtual connections that provide seamless access to Redis through three different connection pools.

 * Templated strings interface [(more about parameter binding)](//github.com/vtortola/RedisClient/wiki/Parameter-binding).
 * Transparent connection management [(more about connection management)](//github.com/vtortola/RedisClient/wiki/Connection-management).
 * Basic output binding [(more about output binding)](//github.com/vtortola/RedisClient/wiki/Getting-results).
 * Server-side scripting through procedures [(more about procedures)](//github.com/vtortola/RedisClient/wiki/Procedures).
 * A [debugging tool for procedures](//github.com/vtortola/RedisClient/wiki/Procedure-Debugger) is available in this repository.
 * Support for asynchronous, synchronous and "fire and forget" operations.

#### Simple procedure example
Imagine a catalog application, with products/services defined as different hashes in the Redis database, where each hash contains the properties of each product, like 
name, url, stock, description, picture url, etc... Also you have different zsets containing the keys of all products sorted by a specific properties or just grouped by categories. 
Since there may be a big amount of products, pagination is needed to avoid blowing up the server response with too much data. 

How is this pagination achieved **without server side scripting**? First querying the zset with the desired range to obtain the list of hash keys that need to be retrieved, and then retrieving
each key (usually) one by one. 

This can be expedited and simplified **with server-side scripting**. For example, you can use a procedure to get the list of products directly, without extra round trips:

```
proc ZPaginate(zset, page, itemsPerPage)
	
	local start  = page * itemsPerPage
	local stop = start + itemsPerPage - 1
	local items = redis.call('ZREVRANGE', zset, start, stop)

	local result = {}

	for index, key in ipairs(items) do
	    result[index] = redis.call('HGETALL', key)
	end

	return result
endproc
```   

Using the templated string syntax you can invoke this procedure easily:
```
// Execute procedure
var result = channel.Execute("ZPaginate @key @page @items", 
                              new { key = "products:bydate",  page=3, items=10 });

// Expand result of the first line as a collection of results
var hashes = result[0].AsResults();

// Bind each hash to objects
// Where <Product> is a class with properties that match the hash keys.
var products = hashes.Select(h => h.AsObjectCollation<Product>()).ToArray();
```

Server side scripting has multiple advantages, like preventing multiple round trips to the Redis instance or atomicity. [Continue reading about procedures](//github.com/vtortola/RedisClient#executing-procedures) 

#### Performance
A [performance comparison](//github.com/vtortola/RedisClient/wiki/Performance) shows the that the performance is close to other well known clients.

<a href="//github.com/vtortola/RedisClient/wiki/Performance" style="display:block"><img src="http://vtortola.github.io/rcclientperfTran.png" height="250"></a>
<a href="//github.com/vtortola/RedisClient/wiki/Performance" style="display:block"><img src="http://vtortola.github.io/rcclientperfPipe.png" height="250"></a>


#### Use in a web application
Also in this repository you will find [SimpleQA](//github.com/vtortola/RedisClient/wiki/SimpleQA-:-A-proof-of-concept), a proof of concept of a _Q&A_ application using RedisClient.

<a href="//github.com/vtortola/RedisClient/wiki/SimpleQA-:-A-proof-of-concept"><img src="http://vtortola.github.io/SimpleQA/Mini.png?version=3" /></a>

## Getting started

### Installing
The alpha version is [available in NuGet](https://www.nuget.org/packages/vtortola.RedisClient/).
```
PM> Install-Package vtortola.RedisClient -Pre
```

### Setting it up
The API has two fundamental parts:
 * `RedisClient` class handles the connection management. Usually you have one instance across all your AppDomain (or two instances if you have master/slave). It is a thread safe object, that usually is cached for the extend of your application lifetime.
 
```cs
_client = new RedisClient(endpoint))
await _client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
``` 

 * `IRedisChannel` interface is used to execute commands. Channels are short lived, cheap to create, non-thread safe objects that represent virtual connections to Redis. Channels provide seamless operation for commanding and subscribing.

```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync("incr mykey")
                 .ConfigureAwait(false);
}
``` 
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;It is possible to execute multiple statements per command, splitting them with line breaks. Statements are pipelined to the same connection (but still they may be interpolated with other commands by Redis, use `MULTI` if you want to avoid it).

```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync(@"
                  incr mykey
                  decr otherkey
                  subscribe topic")
                  .ConfigureAwait(false);
}
``` 
[Read more about available options](//github.com/vtortola/RedisClient/wiki/Options).

[Read more about connection management](//github.com/vtortola/RedisClient/wiki/Connection-management).

### Binding parameters
Although is possible to use RedisClient composing strings dynamically, it is unrecommended. Providing command templates will increase the performance since then the command execution plan can be cached.

Parameter binding works passing an object which properties will be bind to the command parameters, identified by a starting '@'. Only [integral types](https://msdn.microsoft.com/en-us/library/exx3b86w(v=vs.80).aspx), `String`, `DateTime` and their `IEnumerable<>` are supported. Commands should always start by a Redis command or a procedure alias.

```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync(@"
                  incr @counterKey
                  set currentDateKey @date",
                  new { counterKey = "mycounter", date = DateTime.Now })
                  .ConfigureAwait(false);
}
``` 

Collections are added to the command as sequences. For example, it is possible to add multiple items with `SADD`:
```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync("sadd @setKey @data",
                  new { setKey = "myset", data = new [] { "a", "b", "c" } })
                 .ConfigureAwait(false);
}
``` 

Object's properties, `IEnumerable<Tuple<,>>` and `IEnumerable<KeyValuePair<,>>` can be sequenced with the `Parameter` helper. This is handy for example saving objects as hashes:
```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync("hmset myObject @data",
                  new { data = Parameter.SequenceProperties(myObjectInstance))
                 .ConfigureAwait(false);
}
```

[Read more about parameter binding](//github.com/vtortola/RedisClient/wiki/Parameter-binding).

### Getting results
A command execution result implements `IRedisResults`, which allows to inspect the return in every single statement of the command through a `IRedisResultInspector` per statement. 
 * Each statement correlates to a position in the `IRedisResults` items. First statement is item 0, and so on.
 * `.RedisType`: indicates the result type.
 * If the result is an error, accessing the statement result will throw a `RedisClientCommandException` with the details of the Redis error. It is possible to get the exception without throwing it using `.GetException()`.
 * `.GetXXX` methods: will try to read the value as `XXX` type, and will throw an `RedisClientCastException` if the data is not in the expected type.
 * `.AsXXX` methods: will try to read the value as `XXX` type, or parse it as `XXX` (there is no `.GetDouble()` because Redis only returns string, integer or error, but there is a `.AsDouble()`.
 * `.AsObjectCollation<T>()` allows to bind the result to an object by parsing a sequence of key-value pairs, and bind it to the object properties. For example `member1 value1 member2 value2` will be bound as `{ member1 = "value1", member2 = "value2" }`.
 * `.AsDictionaryCollation<TKey, TValue>()` allows to bind the result to an object by parsing a sequence of key-value pairs as `KeyValuePair<>`.
 
 ```cs
using (var channel = _client.CreateChannel())
{
        var results = await channel.ExecuteAsync(@"
                                    incr mycounter
                                    hgetall @customer",
                                    new { customer = "customer:" + customerId} )
                                    .ConfigureAwait(false);
        var value = results[0].GetInteger();
        var obj = results[1].AsObjectCollation<Customer>();
}
```

[Read more about getting results](//github.com/vtortola/RedisClient/wiki/Getting-results).

### Subscribing to channels
`IRedisChannel` exposes a `NotificationHandler` property that can be used to get or set a handler for messages received by this channel. The handler will receive `RedisNotification` objects containing the message data.
```cs
using (var channel = Client.CreateChannel())
{
    channel.NotificationHandler = msg => Console.WriteLine(msg.Content); // will print 'whatever'
    channel.Execute("psubscribe h?llo");
    channel.Execute("publish hello whatever");
}
```
**Note:** You may feel tempted to put the `SUBSCRIBE` and `PUBLISH` statements in the same command, however it won't work because they will be executed in parallel in subscriber and commander connections respectively. Although technically possible to do, I considered this a very unlikely scenario, so in alas of better performace parallel execution is used.

Subscriptions are automatically cleared on `IRedisChannel.Dispose()`, so make sure you always dispose your channels.

[Read more about subscribing to topics](//github.com/vtortola/RedisClient/wiki/Subscribing).

### Executing procedures
Rather than executing LUA scripts directly, they need to be wrapped in what is called a procedure:
```
proc Sum(a, b)
    return a + b
endproc
```
Procedures are loaded in the configuration, and they are automatically deployed to Redis when connecting the first time. Multiple procedures can be uploaded from the same reader.
```cs
var options = new RedisClientOptions()
options
  .Procedures
  .Load(new StringReader(@"
     proc Sum(a, b)
       return a + b
     endproc"));
 ```

Then they can be invoked as normal Redis commands:
```cs
using (var channel = _client.CreateChannel())
{
    var result = await channel.ExecuteAsync("Sum 1 2")
                              .ConfigureAwait(false);
    var value = result[0].GetInteger();
}
``` 

Procedures accepts single and collection parameters:
 * `parameterName` will expect a single value'.
 * `parameterName[]` will expect one or more parameters. They are [LUA arrays](https://www.lua.org/pil/11.1.html).

Also, parameters can be passed as keys to the script (important for clustering) using the `$` prefix, either in single or collection parameters. The parameter (or parameters) will be passed in `KEYS` rather than in `ARGV`.

Quick example:
```
-- sums the content of a and stored the content
-- into the key specified in <asum>
proc sumAndStore($asum, a[])
   local function sum(t)
       local sum = 0
       for i=1, table.getn(t), 1 
       do 
          sum = sum + t[i]
       end
       return sum
   end
   local result = sum(a)
   return redis.call('set', asum, result)
endproc
``` 

Invoking:
```cs
using (var channel = _client.CreateChannel())
{
    var result = await channel.ExecuteAsync(@"
                               sumAndStore @key @values",
                               new { key = "mysum", values = new [] { 1, 2, 3} }
                               ).ConfigureAwait(false);
    result[0].AssertOK();
}
``` 
This will store the value `6` as string in the key `mysum` and will return `OK`. The value `mysum` is passed in `KEYS` rather than in `ARGV`.

[Read more about procedures](//github.com/vtortola/RedisClient/wiki/Procedures).

[Read more about procedures management](//github.com/vtortola/RedisClient/wiki/Procedure-management).

[Read more about available options](//github.com/vtortola/RedisClient/wiki/Options).
