// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestAsyncHandlers
    {
        [Fact]
        public async Task TestValueTask()
        {
            //SETUP
            async ValueTask<int> GetIdSync()
            {
                return 1;
            }

            async ValueTask<int> GetIdAsync()
            {
                await Task.Delay(1);
                return 2;
            }


            //ATTEMPT
            var r1 = GetIdSync().Result;
            var r2 = await GetIdSync();
            var r3 = await GetIdAsync();
            var r4 = GetIdAsync().Result;

            //VERIFY
            r1.ShouldEqual(1);
            r2.ShouldEqual(1);
            r3.ShouldEqual(2);
            r4.ShouldEqual(2);
        }
    }
}