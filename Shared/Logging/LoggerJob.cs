﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Logging;
public class LoggerJob
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LogReader _reader;
    private readonly CancellationTokenSource _source;
    private readonly CancellationToken _stoppingToken;

    public LoggerJob(IHttpClientFactory httpClientFactory, LogReader reader)
    {
        _httpClientFactory = httpClientFactory;
        _reader = reader;
        _source = new CancellationTokenSource();
        _stoppingToken = _source.Token;
    }

    // TODO: Send log messages in batches
    public async Task Start()
    {
        await foreach (var message in _reader.Read(_stoppingToken))
        {
            await _httpClientFactory
                .CreateClient("LoggerJob")
                .PostAsJsonAsync("/logs", message, cancellationToken: _stoppingToken);
        }
    }

    public void Stop() =>
        _source.Cancel();
}
