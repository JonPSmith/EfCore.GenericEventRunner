# EfCore.GenericEventRunner

This library allows users of Entity Framework Core (EF Core) to add events to their entity classes, i.e. the classes that EF Core maps to a database. It is useful if you have business rules that are tiggered by a property changing, or an event such as receiving an customer order and you need to check some things before you can accept it. 

Version 2 how supports async event handlers and, via the [EfCore.GenericEventRunner.DomainParts](https://www.nuget.org/packages/EfCore.GenericEventRunner.DomainParts) library, it can support a Clean Code architecture. 

Preview 2.1 supports more sorts of events, including ones that run withing a transaction where SaveChanges is called. This allows you to implement Integration events across multiple parts of your application.

This is an open source project (MIT license) available [on GitHub](https://github.com/JonPSmith/EfCore.GenericEventRunner) and as a [NuGet package](https://www.nuget.org/packages/EfCore.GenericEventRunner/).

## Useful articles

* [Article about this event-driven architecture](https://www.thereformedprogrammer.net/a-robust-event-driven-architecture-for-using-with-entity-framework-core/) - good to get an idea of what the library is trying to do.
* [The "how" and "why" of the EfCore.GenericEventRunner library](https://www.thereformedprogrammer.net/efcore-genericeventrunner-an-event-driven-library-that-works-with-ef-core/) - read this for detailed documentation.




