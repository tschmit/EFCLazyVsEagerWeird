## Illustration of an odd comportment difference between Lazy and Eager Loading in a particular case.

The story starts by using ef on an existing database.

This repository follows an StackOverflow [post](https://stackoverflow.com/questions/77983253/ef-core-8-on-net-8-mismatch-behavior-lazy-vs-eager-loading).

The case is when an SQL column is nullable but the materialized property is not.

```csharp
public class Blog {
    public int Id {get; set;}
    public ICollection<Log> Logs {get; set;}
}
public class Log {
    public int Id {get; set;}
    public bool IsError {get; set;}
}
```

- I had to 'hack' the sql migration to set the IsError column as nullable.
- I had to use raw sql to populate the database as the model and the storage are not aligned (due to the 'hack').

To see the error you have too:
- set the connection string in the code
- run once to create an populate
- run again to see the error

```csharp
using BlogCtx ctx = new BlogCtx(cs);
b = await ctx.Set<Blog>().Include(x => x.Logs).Where(x => x.Id == 1).FirstOrDefaultAsync();
Console.WriteLine(b.Logs.Count()); // <=== this Eager case runs

using BlogCtx ctx2 = new BlogCtx(cs);
b = await ctx2.Set<Blog>().Where(x => x.Id == 1).FirstOrDefaultAsync();
Console.WriteLine(b.Logs.Count()); // <=== this Lasy case will throw an exception

```