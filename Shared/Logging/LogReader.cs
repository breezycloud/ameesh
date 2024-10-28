using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Logging;

namespace Shared.Logging;

public class LogReader
{
     private readonly LogQueue _queue;

    public LogReader(LogQueue queue)
    {
        _queue = queue;
    }

    public IAsyncEnumerable<LogMessage> Read(CancellationToken token = default) =>
        _queue.ReadLogs(token);
}
