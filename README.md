# EfCore.GenericEventRunner

This library allows users of Entity Framework Core (EF Core) to add events to their entity classes, i.e. the classes that EF Core maps to a database. It is useful if you have business rules that are triggered by a property changing, or an event such as receiving an customer order and you need to check some things before you can accept it. 

This is an open source project (MIT license) available [on GitHub](https://github.com/JonPSmith/EfCore.GenericEventRunner) and as a [NuGet package](https://www.nuget.org/packages/EfCore.GenericEventRunner/). See [ReleaseNotes](https://github.com/JonPSmith/EfCore.GenericEventRunner/blob/master/ReleaseNotes.md) for more information. 

Documentation and links to articles can be found via the [Documentation link](https://github.com/JonPSmith/EfCore.GenericEventRunner/wiki).

## Three types of events

The following image shows the three types of events and when they are called.

![Three types of events](https://github.com/JonPSmith/EfCore.GenericEventRunner/blob/master/GenericEventRunnerTypesOfEvents.png)

## Useful articles

* [Article about this event-driven architecture](https://www.thereformedprogrammer.net/a-robust-event-driven-architecture-for-using-with-entity-framework-core/) - good to get an idea of what the library is trying to do.
* [The "how" and "why" of the EfCore.GenericEventRunner library](https://www.thereformedprogrammer.net/efcore-genericeventrunner-an-event-driven-library-that-works-with-ef-core/) - read this for detailed documentation.




