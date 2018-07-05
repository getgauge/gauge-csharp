// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace Gauge.CSharp.Lib
{
    /// <summary>
    ///     Defines a custom implementation to capture screenshot on failure.
    ///     Deprecated in favour of <see cref="ICustomScreenshotGrabber"/>
    /// </summary>
    [Obsolete("Please use ICustomScreenshotGrabber instead. This interface is likely to be removed in future releases.")]
    public interface IScreenGrabber
    {
        /// <summary>
        ///     Define your own way to take screenshot, that is best applicable to your system-under-test.
        ///     Gauge can take this screenshot and use it for reporting.
        ///     By default, Gauge attempts to capture the active window screenshot, on failure.
        /// </summary>
        /// <returns>A byte array, containing the bitmap equivalent of the image.</returns>
        byte[] TakeScreenShot();
    }

    /// <summary>
    ///     Defines a custom implementation to capture screenshot on failure.
    /// </summary>
    public interface ICustomScreenshotGrabber
    {
        /// <summary>
        ///     Define your own way to take screenshot, that is best applicable to your system-under-test.
        ///     Gauge can take this screenshot and use it for reporting.
        ///     By default, Gauge attempts to capture the active window screenshot, on failure.
        /// </summary>
        /// <returns>A byte array, containing the bitmap equivalent of the image.</returns>
        byte[] TakeScreenShot();
    }
}