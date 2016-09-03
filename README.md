# RedisClient
####.NET client for Redis
##### Status: ALPHA

**vtortola.RedisClient** is based on templated strings, analyzes the commands and decides which is the best way of executing them. It uses two connection pools: one shared, multiplexed and pipelined; and other exclusive and pipelined. It also uses a special syntax for working with LUA scripts, named [procedures]().

 * Templated strings interface [(more about parameter binding)]().
 * Seamless connection management [(more about connection management)]().
 * Basic output binding [(more about output binding)]().
 * Script managememnt through procedures [(more about procedures)]().
 * Support for asynchronous, synchronous and "fire and forget" operations.
 * Support for blocking operations and partial transactions.
 
 ## Getting started

### Set up
The API has to main fundamental pieces:
 * `RedisClient` class handles the connection management. Usually you have one instance across all your AppDomain (or two instances if you have master/slave). It is a thread safe object, expected for the extend of your application lifetime.
 
```cs
_client = new RedisClient(endpoint))
await _client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
``` 

 * `IRedisChannel` interface is used to execute commands. Channels are short lived, cheap to create, non-thread safe objects that represent virtual connections to Redis. Channels provide seamless access to command and subscription connections, analyze commands and decide how to route them through the three connection pools (multiplexed, exclusive and subscription).

```cs
using (var channel = _client.CreateChannel())
{
    await channel.ExecuteAsync("incr mykey").ConfigureAwait(false);
}
``` 
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; It is possible to execute multiple statements per command, splitting them with line breaks. Statements are pipelined to the same connection (but still they may be interpolated with other commands by Redis, using `MULTI` if you want to avoid it).

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
                               new { data = Parameter.SequenceProperties(my))
                               .ConfigureAwait(false);
}
```

### Getting results
A command execution result implements `IRedisResults`, which allows to inspect the return in every single statement of the command through a `IRedisResultInspector` per statement. 
 * Each statement correlates to a position in the `IRedisResults` items. First statement is item 0, and so on.
 * `.RedisType`: indicates the result type.
 * If the result is an error, accessing the statement result will throw a `RedisClientCommandException` with the details of the Redis error. It is possible to get the exception without throwing it using `.GetException()`.
 * `.GetXXX` methods: will try to read the value as `XXX` type, and will throw an `RedisClientCastException` if the data is not in the expected type.
 * `.AsXXX` methods: will try to read the value as `XXX` type, or parse it as `XXX` (there is no `.GetDouble()` because Redis only returns string, integer or error, but there is a `.AsDouble()`.
 * `.AsObjectCollation<T>()` allows to bind the result to an object by parsing a sequence of key-value pairs, and bind it to the object properties. For example `member1 value1 member2 value2` will be bound as `{ member1 = "value1", member2 = "value2" }`.
 * `.AsDictionaryCollation` allows to bind the result to an object by parsing a sequence of key-value pairs as `KeyValuePair<>`.
 


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

### Subscribing
