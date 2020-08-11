# Release notes

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