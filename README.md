# EfCore.GenericEventRunner

This library allows users of Entity Framework Core (EF Core) to add events to their entity classes, i.e. the classes that EF Core maps to a database. It is useful if you have business rules that are tiggered by a property changing, or an event such as receiving an customer order and you need to check some things before you can accept it. 

This is an open source project (MIT license) available on GitHub at https://github.com/JonPSmith/EfCore.GenericServices

## Useful articles

* [Article about this event-driven architecture](https://www.thereformedprogrammer.net/a-robust-event-driven-architecture-for-using-with-entity-framework-core/) - good to get an idea of what the library is trying to do.
* [Detailed article going though the how and why of the EfCore.GenericEventRunner library](https://www.thereformedprogrammer.net/efcore-genericeventrunner-an-event-driven-library-that-works-with-ef-core/)




