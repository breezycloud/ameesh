using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Logging;

namespace Shared.Logging;

public class LogWriter
{
    private readonly LogQueue _queue;

    public LogWriter(LogQueue queue)
    {
        _queue = queue;
    }

    public ValueTask Write(LogMessage message, CancellationToken token = default) =>
        _queue.WriteLog(message, token);
}
