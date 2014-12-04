using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPServer.Interface
{
    /// <summary>
    /// The interface for an object that abstracts away the clock.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current date and time at UTC.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
