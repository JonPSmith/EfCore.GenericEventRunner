// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers.Internal;
using GenericEventRunner.ForSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// This is the class that will be injected into the DbContext to handle events
    /// NOTE: The EventsRunner has the lifetime of the DbContext, i.e. its Scoped
    /// </summary>
    public class EventsRunner : IEventsRunner
    {
        private readonly RunEachTypeOfEvents _eachEventRunner;
        private readonly IGenericEventRunnerConfig _config;

        /// <summary>
        /// This is the class that will manage the events inside your DbContext
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public EventsRunner(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, IGenericEventRunnerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _eachEventRunner = new RunEachTypeOfEvents(serviceProvider, logger, _config);
        }

        /// <summary>
        /// This runs the events before and after the base SaveChanges method is run
        /// </summary>
        /// <param name="context">The current DbContext</param>
        /// <param name="callBaseSaveChanges">A function that is linked to the base SaveChanges in your DbContext</param>
        /// <returns>Returns the status with the numUpdated number from SaveChanges</returns>
        public IStatusGeneric<int> RunEventsBeforeAfterSaveChanges(DbContext context, Func<int> callBaseSaveChanges)
        {
            IStatusGeneric<int> RunTransactionWithDuringSaveChangesEvents()
            {
                var localStatus = new StatusGenericHandler<int>();
                //If there is a current transaction then use that, otherwise 
                using var transaction = context.Database.CurrentTransaction == null 
                    ? context.Database.BeginTransaction()
                    : null;
                var duringPreValueTask = _eachEventRunner.RunDuringSaveChangesEventsAsync(context, false, false);
                if (!duringPreValueTask.IsCompleted)
                    throw new InvalidOperationException("Can only run sync tasks");
                localStatus.CombineStatuses(duringPreValueTask.Result);

                var transactionSaveChanges = CallSaveChangesWithExceptionHandler(context, callBaseSaveChanges);
                if (localStatus.CombineStatuses(transactionSaveChanges).HasErrors)
                    return localStatus;

                localStatus.SetResult(transactionSaveChanges.Result);

                var duringPostValueTask = _eachEventRunner.RunDuringSaveChangesEventsAsync(context, true,false);
                if (!duringPostValueTask.IsCompleted)
                    throw new InvalidOperationException("Can only run sync tasks");
                if (localStatus.CombineStatuses(duringPostValueTask.Result).HasErrors)
                    return localStatus;

                transaction?.Commit();

                return localStatus;
            }

            var status = new StatusGenericHandler<int>();
            var hasDuringEvents = _eachEventRunner.SetupDuringEvents(context);

            var beforeValueTask = _eachEventRunner.RunBeforeSaveChangesEventsAsync(context, false);
            if (!beforeValueTask.IsCompleted)
                throw new InvalidOperationException("Can only run sync tasks");
            status.CombineStatuses(beforeValueTask.Result);
            if (!status.IsValid) 
                return status;

            context.ChangeTracker.DetectChanges();

            //This runs any actions adding to the config that match this DbContext type
            RunAnyAfterDetectChangesActions(context);

            //Call SaveChanges with catch for exception handler
            IStatusGeneric<int> callSaveChangesStatus;
            if (!hasDuringEvents)
            {
                //No need for a transaction as no During event. Therefore just call SaveChanges
                callSaveChangesStatus = CallSaveChangesWithExceptionHandler(context, callBaseSaveChanges);
            }
            else if (context.Database.CurrentTransaction == null && context.Database.CreateExecutionStrategy().RetriesOnFailure)
            {
                //There is no existing transactions AND we have to handle retries, then we need to wrap the transaction in a retry 
                callSaveChangesStatus = context.Database.CreateExecutionStrategy().Execute(RunTransactionWithDuringSaveChangesEvents);
            }
            else
            {
                callSaveChangesStatus = RunTransactionWithDuringSaveChangesEvents();
            }
            
            if (status.CombineStatuses(callSaveChangesStatus).HasErrors)
                return status;

            //Copy over the saveChanges result
            status.SetResult(callSaveChangesStatus.Result);

            var afterValueTask = _eachEventRunner.RunAfterSaveChangesEventsAsync(context, false);
            if (!afterValueTask.IsCompleted && !afterValueTask.IsFaulted)
                throw new InvalidOperationException("Can only run sync tasks");
            if (afterValueTask.IsFaulted)
                throw afterValueTask.Result;

            return status;
        }

        /// <summary>
        /// This runs the events before and after the base SaveChangesAsync method is run
        /// </summary>
        /// <param name="context">The current DbContext</param>
        /// <param name="callBaseSaveChangesAsync">A function that is linked to the base SaveChangesAsync in your DbContext</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the status with the numUpdated number from SaveChanges</returns>
        public async Task<IStatusGeneric<int>> RunEventsBeforeAfterSaveChangesAsync(DbContext context,
            Func<Task<int>> callBaseSaveChangesAsync, CancellationToken cancellationToken)
        {

            async Task<IStatusGeneric<int>> RunTransactionWithDuringSaveChangesEventsAsync()
            {
                var localStatus = new StatusGenericHandler<int>();
                //If there is a current transaction then use that, otherwise 
                using var transaction = context.Database.CurrentTransaction == null
                    ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
                    : null;
                var duringPreStatus = await _eachEventRunner.RunDuringSaveChangesEventsAsync(context, false, true)
                    .ConfigureAwait(false);
                localStatus.CombineStatuses(duringPreStatus);

                var transactionSaveChanges = await CallSaveChangesWithExceptionHandlerAsync(context, callBaseSaveChangesAsync)
                    .ConfigureAwait(false);
                if (localStatus.CombineStatuses(transactionSaveChanges).HasErrors)
                    return localStatus;

                localStatus.SetResult(transactionSaveChanges.Result);

                var duringPostStatus = await _eachEventRunner.RunDuringSaveChangesEventsAsync(context, true, true)
                    .ConfigureAwait(false);
                if (localStatus.CombineStatuses(duringPostStatus).HasErrors)
                    return localStatus;

                if (transaction != null)
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                return localStatus;
            }

            var status = new StatusGenericHandler<int>();
            var hasDuringEvents = _eachEventRunner.SetupDuringEvents(context);
            
            status.CombineStatuses(await _eachEventRunner.RunBeforeSaveChangesEventsAsync(context, true).ConfigureAwait(false));
            if (!status.IsValid)
                return status;

            context.ChangeTracker.DetectChanges();

            //This runs any actions adding to the config that match this DbContext type
            RunAnyAfterDetectChangesActions(context);

            //Call SaveChangesAsync with catch for exception handler
            IStatusGeneric<int> callSaveChangesStatus;
            if (!hasDuringEvents)
            {
                //No need for a transaction as no During event. Therefore just call SaveChanges
                callSaveChangesStatus = await CallSaveChangesWithExceptionHandlerAsync(context, callBaseSaveChangesAsync)
                    .ConfigureAwait(false);
            }
            else if (context.Database.CurrentTransaction == null && context.Database.CreateExecutionStrategy().RetriesOnFailure)
            {
                //There is no existing transactions AND we have to handle retries, then we need to wrap the transaction in a retry 
                callSaveChangesStatus = await context.Database.CreateExecutionStrategy().ExecuteAsync(async x =>
                    await RunTransactionWithDuringSaveChangesEventsAsync().ConfigureAwait(false), cancellationToken);
            }
            else
            {
                callSaveChangesStatus = await RunTransactionWithDuringSaveChangesEventsAsync().ConfigureAwait(false);
            }

            if (status.CombineStatuses(callSaveChangesStatus).HasErrors)
                return status;
            do
            {
                try
                {
                    status.SetResult(await callBaseSaveChangesAsync().ConfigureAwait(false));
                    break; //This breaks out of the do/while
                }
                catch (Exception e)
                {
                    var exceptionStatus = _config.SaveChangesExceptionHandler?.Invoke(e, context);
                    if (exceptionStatus == null)
                        //This means the SaveChangesExceptionHandler doesn't cover this type of Concurrency Exception
                        throw;
                    //SaveChangesExceptionHandler ran, so combine its error into the outer status
                    status.CombineStatuses(exceptionStatus);
                }
                //If the SaveChangesExceptionHandler fixed the problem then we call SaveChanges again, but with the same exception catching.
            } while (status.IsValid);
            await _eachEventRunner.RunAfterSaveChangesEventsAsync(context, true).ConfigureAwait(false);
            return status;
        }

        //------------------------------------------
        // private methods

        private void RunAnyAfterDetectChangesActions(DbContext context)
        {
            _config.ActionsToRunAfterDetectChanges
                .Where(x => context.GetType() == x.dbContextType)
                .ToList().ForEach(x => x.action());
        }

        private IStatusGeneric<int> CallSaveChangesWithExceptionHandler(DbContext context, Func<int> callBaseSaveChanges)
        {
            var status = new StatusGenericHandler<int>();
            do
            {
                try
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;

                    status.SetResult(callBaseSaveChanges());
                    break; //This breaks out of the do/while
                }
                catch (Exception e)
                {
                    var exceptionStatus = _config.SaveChangesExceptionHandler?.Invoke(e, context);
                    if (exceptionStatus == null)
                        //This means the SaveChangesExceptionHandler doesn't cover this type of Concurrency Exception
                        throw;
                    //SaveChangesExceptionHandler ran, so combine its error into the outer status
                    status.CombineStatuses(exceptionStatus);
                }
                finally
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = true;
                }

                //If the SaveChangesExceptionHandler fixed the problem then we call SaveChanges again, but with the same exception catching.
            } while (status.IsValid);

            return status;
        }

        private async Task<IStatusGeneric<int>> CallSaveChangesWithExceptionHandlerAsync(DbContext context, Func<Task<int>> callBaseSaveChangesAsync)
        {
            var status = new StatusGenericHandler<int>();
            do
            {
                try
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;

                    status.SetResult(await callBaseSaveChangesAsync().ConfigureAwait(false));
                    break; //This breaks out of the do/while
                }
                catch (Exception e)
                {
                    var exceptionStatus = _config.SaveChangesExceptionHandler?.Invoke(e, context);
                    if (exceptionStatus == null)
                        //This means the SaveChangesExceptionHandler doesn't cover this type of Concurrency Exception
                        throw;
                    //SaveChangesExceptionHandler ran, so combine its error into the outer status
                    status.CombineStatuses(exceptionStatus);
                }
                finally
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = true;
                }

                //If the SaveChangesExceptionHandler fixed the problem then we call SaveChanges again, but with the same exception catching.
            } while (status.IsValid);

            return status;
        }



    }
}