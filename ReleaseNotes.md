# Release notes


## 2.2.0 

- BREAKING CHAGE - New feature to check that there is a handler for each event. Consider Async/Sync problem.

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