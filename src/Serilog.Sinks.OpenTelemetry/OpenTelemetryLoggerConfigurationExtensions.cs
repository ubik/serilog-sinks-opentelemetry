﻿// Copyright 2022 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog;

/// <summary>
/// Class containing extension methods to <see cref="LoggerConfiguration"/>, configuring sinks
/// to convert Serilog LogEvents to OpenTelemetry LogRecords and to send them to an OTLP/gRPC
/// endpoint.
/// </summary>
public static class OpenTelemetryLoggerConfigurationExtensions
{
    /// <summary>
    /// Adds a non-durable sink that transforms Serilog log events into OpenTelemetry
    /// log records, sending them to an OTLP gRPC endpoint.
    /// </summary>
    /// <param name="sinkConfiguration">
    /// The logger configuration.
    /// </param>
    /// <param name="endpoint">
    /// The full URL of the OTLP/gRPC endpoint.
    /// </param>
    /// <param name="resourceAttributes">
    /// A Dictionary<string, Object> containing attributes of the resource attached
    /// to the logs generated by the sink. The values must be simple primitive 
    /// values: integers, doubles, strings, or booleans. Other values will be 
    /// silently ignored. 
    /// </param>
    /// <param name="formatProvider">
    /// Provider for formatting and rendering log messages.
    /// </param>
    /// <param name="restrictedToMinimumLevel">
    /// The minimum level for events passed through the sink. Default value is
    /// <see cref="LevelAlias.Minimum"/>.
    /// </param>
    /// <param name="batchSizeLimit">
    /// The maximum number of log events to include in a single batch.
    /// </param>
    /// <param name="batchPeriod">
    /// The maximum delay in seconds between batches.
    /// </param>
    /// <param name="batchQueueLimit">
    /// The maximum number of batches to hold in memory.
    /// </param>
    /// <returns>Logger configuration, allowing configuration to continue.</returns>
    public static LoggerConfiguration OpenTelemetry(
        this LoggerSinkConfiguration sinkConfiguration,
        string endpoint = "http://localhost:4317/v1/logs",
        IDictionary<string, Object>? resourceAttributes = null,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        int batchSizeLimit = 100,
        int batchPeriod = 2,
        int batchQueueLimit = 10000)
    {
        if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

        var sink = new OpenTelemetrySink(
            endpoint: endpoint,
            formatProvider: formatProvider,
            resourceAttributes: resourceAttributes);

        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = TimeSpan.FromSeconds(batchPeriod),
            EagerlyEmitFirstEvent = true,
            QueueLimit = batchQueueLimit
        };

        var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);

        return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
    }
}
