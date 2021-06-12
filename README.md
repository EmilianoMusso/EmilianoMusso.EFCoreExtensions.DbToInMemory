# EmilianoMusso.EFCoreExtensions.DbToInMemory

A library to easy load persisted data from database to [InMemory](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/?tabs=dotnet-core-cli)

**Usage**:

Let's suppose we have a Sql Server instance named `MyInstance`, and a database named `MyDatabase`.
The database contains tables, two of them named `MyTableType` and `AnotherTableType`. 
We need to use already persisted data for our integration tests, without tampering the real data, but using it.

EF Core allow us to specify a memory instanced data context using `UseInMemoryDatabase` method, which allow us to replicate our model in an in-memory structure.
This method supports initial seeding through migrations, but this extension has the purpose to make easier the in-memory data seeding.

Please consider the following snippet:

```csharp
var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>()
    .UseInMemoryDatabase("TEST");

var context = new MyDbContext(optionsBuilder.Options);

var connectionString = @"Server=.\MyInstance;Database=MyDatabase;Trusted_Connection=True;";
var toInMem = new DatabaseToInMemory(context, connectionString);

toInMem.LoadTable<MyTableType>()
        .LoadTable<AnotherTableType>(5)
        .PersistToMemory();
```

Here, we have declared an in-memory database modeled on an already existant DbContext.
Then, having instantiated our context, we can simply initialize a new object of `DatabaseToInMemory` type.

That class must be initialized by passing a `DbContext` and a connection string to a physical database.
Afterwards, we can use the `LoadTable<T>` method, where `<T>` is one of the types present in our context to indicate the `DbSet`s representing each table. That method accepts a parameter to specify a number of record to be loaded (`10` by default).

Unless otherwise stated in the `DatabaseToInMemory` constructor, data will be picked randomly from each table.
At the end of load operations, which can be concatenated, we can call `PersistToMemory` or `PersistToMemoryAsync` - which are two wrappers for `SaveChanges` and `SaveChangesAsync` - to persist our data in the in-memory database.