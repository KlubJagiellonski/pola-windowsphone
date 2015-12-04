using System;

namespace Windows.ApplicationModel
{
    public static class ApplicationModelExtensions
    {
        /// <summary>
        /// Transforms a PackageVersion object to an instance of Version class.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Version ToVersion(this PackageVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
