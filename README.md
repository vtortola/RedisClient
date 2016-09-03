# RedisClient
####.NET client for Redis
##### Status: ALPHA

**vtortola.RedisClient** is based on templated strings, analyzes the commands and decides which is the best way of executing them. It uses two commander connection pools: one shared, multiplexed and pipelined; and other exclusive and pipelined; and an additional connection pool for subscriber connections. It also uses a special syntax for working with LUA scripts, named [procedures]().

 * Templated strings interface [(more about parameter binding)](//github.com/vtortola/RedisClient/wiki/Parameter-binding).
 * Seamless connection management [(more about connection management)](//github.com/vtortola/RedisClient/wiki/Connection-management).
 * Basic output binding [(more about output binding)](//github.com/vtortola/RedisClient/wiki/Getting-results).
 * Script managememnt through procedures [(more about procedures)](//github.com/vtortola/RedisClient/wiki/Procedures).
 * Support for blocking operations and partial transactions.
 * Support for asynchronous, synchronous and "fire and forget" operations.
 
## Getting started

### Installing

Nuget..

### Setting it up
The API has to main fundamental pieces:
 * `RedisClient` class handles the connection management. Usually you have one instance across all your AppDomain (or two instances if you have master/slave). It is a thread safe object, expected for the extend of your application lifetime.
 
```cs
_client = new RedisClient(endpoint))
await _client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
``` 

 * `IRedisChannel` interface is used to execute commands. Channels are short lived, cheap to create, non-thread safe objects that represent virtual connections to Redis. Channels provide seamless access to commander and subscriber connections, analyze commands and decide how to route them through the three connection pools (multiplexed, exclusive and subscription).

```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync("incr mykey").ConfigureAwait(false);
}
``` 
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; It is possible to execute multiple statements per command, splitting them with line breaks. Statements are pipelined to the same connection (but still they may be interpolated with other commands by Redis, use `MULTI` if you want to avoid it).

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
                                    new { customer = "customer:" + customerId )
                                    .ConfigureAwait(false);
        var value = results[0].GetInteger();
        var obj = results[1].AsObjectCollation<Customer>();
}
```

[Read more about getting results)](//github.com/vtortola/RedisClient/wiki/Getting-results).

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
 
 [Read more about available options](//github.com/vtortola/RedisClient/wiki/Options).
 
Then they can be invoked as normal Redis commands:
```cs
using (var channel = _client.CreateChannel())
{
    var result = await channel.ExecuteAsync("Sum 1 2").ConfigureAwait(false);
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
