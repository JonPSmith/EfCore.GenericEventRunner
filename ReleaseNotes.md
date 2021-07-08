# Release notes

## 2.3.3

- Bug Fix - calling `SaveChangesAsync` does not return the number of database updates - see issue #6

## 2.3.2

- Bug Fix - better handling of exceptions in sync methods (added better stacktrace)

## 2.3.1

- Bug Fix - better handling of exceptions in sync methods

## 2.3.0 
- New feature - Doesn't register an event handler if it is already registered with the DI provider. This stops multiple registering of event handlers.
- New feature - Add [RemoveDuplicateEvents] attribute to event class to ensure only one event per type, per entity. Useful if you want an event to only trigger once.

## 2.2.2

- Feature/bug fix: Before events can now create During events that will be found and run. 

## 2.2.1

- Bug Fix: If scans multple assemblies it double registered some event handlers
- Feature: The `RegisterGenericEventRunner` method returns a log of what it resgistered - useful for debugging startup. 

## 2.2.0 

- Feature: GenericEventRunner is guaranteed to work with DbContext pooling - issue #3 (see note in Release Notes).
- Minor change: If configration `NotUsingDuringSaveHandlers` is true, then it supressess the running of the DuringEvents part.

NOTE: The EventRunner creates new instances of all the code every time it is called. This stops any potential issues when using DbContext pooling. PS. It most likely would have worked but this makes sure it does.

## 2.1.0

- BREAKING CHANGE - IDomainEvent renamed to IEntityEvent
- BREAKING CHANGE - the definitions of the events handler has changed. The callingEntity parameter is now an object. 
- BREAKING CHANGE - You now need to register exception handlers against the DbContext type. This allows different handlers for multiple DbContexts
- New Feature: Added during event and event handlers. The event is called within a transaction.
- New feature: DuringSaveStage attribute. Allows you move the during event from running after SaveChanges in the transaction to before the call to SaveChanges (but after DetectChanges has been called).
- New Feature: You can now pick what types of events an entity supports by applying the approriate interfaces: IEntityWithBeforeSaveEvents, IEntityWithDuringSaveEvents, and IEntityWithAfterSaveEvents
- New feature - You can inject a action to be run after DetectChanges is called. Useful for adding last created/updated times.


## 2.0.0

- Make GenericEventRunner support a clean archtecture
   - Add IEventType to GenericEventRunner class and registering
   - Config has GetBeforeSaveEventsThenClear/GetAfterSaveEventsThenClear funcs
- Add Async event handlers

## 1.2.0

- Added IStatusFromLastSaveChanges interface to make it easier to get to the Status if you capture the GenericEventRunnerStatusException

## 1.1.0

- Added SaveChangesExceptionHandler to configuration. Used for handling concurrency issues etc.   
*See article [Entity Framework Core – validating data and catching SQL errors](https://www.thereformedprogrammer.net/entity-framework-core-validating-data-and-catching-sql-errors/) on this feature.**

## 1.0.0

- Initial release.