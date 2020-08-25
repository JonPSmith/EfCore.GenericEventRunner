// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;

namespace Test.EfHelpers
{
    public static class SqlServerWithExecution
    {
        public static DbContextOptions<TContext> CreateOptionsWithRetryExecutions<TContext>(this object callingClass) where TContext : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString();
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());

            return builder.Options; 
        }
    }
}