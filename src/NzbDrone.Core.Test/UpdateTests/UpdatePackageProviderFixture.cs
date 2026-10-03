using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [Test]
        public void should_not_find_update()
        {
            Subject.GetLatestUpdate("main", new Version(3, 0)).Should().BeNull();
        }

        [Test]
        public void should_not_get_recent_updates()
        {
            Subject.GetRecentUpdates("main", new Version(4, 0), null).Should().BeEmpty();
        }

        [Test]
        public void should_not_call_update_service()
        {
            Subject.GetLatestUpdate("main", new Version(3, 0));
            Subject.GetRecentUpdates("main", new Version(4, 0), null);

            Mocker.GetMock<IHttpClient>().VerifyNoOtherCalls();
        }
    }
}
