using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Models.Logging;
using static Microsoft.Extensions.Logging.LogLevel;


namespace Shared.Logging;

public class DatabaseLogger : ILogger
{
    private readonly LogWriter _logWriter;

        public DatabaseLogger(LogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel is Error or Critical;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Task.Run(async () =>
                await _logWriter.Write(JsonSerializer.Deserialize<LogMessage>(formatter(state, exception))));
        }
}
