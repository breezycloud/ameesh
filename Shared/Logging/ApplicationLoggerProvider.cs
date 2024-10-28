using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Logging;
public sealed class ApplicationLoggerProvider : ILoggerProvider
{
    private readonly LogWriter _logWriter;

    public ApplicationLoggerProvider(LogWriter logWriter)
    {
        _logWriter = logWriter;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return new DatabaseLogger(_logWriter);
    }

    public void Dispose()
    {
    }
}
