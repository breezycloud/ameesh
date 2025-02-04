﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Server.Logging;
public class ApplicationLoggerProvider : ILoggerProvider
{
    private readonly IDbContextFactory<LoggingContext> _contextFactory;

    public ApplicationLoggerProvider(IDbContextFactory<LoggingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return new DatabaseLogger(_contextFactory);
    }

    public void Dispose()
    {

    }
}