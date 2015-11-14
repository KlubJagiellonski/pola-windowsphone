using System;

namespace Windows.ApplicationModel
{
    public static class ApplicationModelExtensions
    {
        public static Version ToVersion(this PackageVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
