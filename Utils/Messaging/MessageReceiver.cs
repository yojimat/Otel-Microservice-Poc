// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0
// Altered by : Vinícius Costa

using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace Utils.Messaging;

public class MessageReceiver(ILogger<MessageReceiver> logger)
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageReceiver));
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public void ReceiveMessage(BasicDeliverEventArgs ea)
    {
        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
        var activityName = $"{ea.RoutingKey} receive";

        using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            logger.LogInformation($"Message received: [{message}]");

            activity?.SetTag("message", message);

            // Simulate some work
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message processing failed.");
        }
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                if (value is byte[] bytes) return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract trace context.");
        }

        return Enumerable.Empty<string>();
    }
}

public class BasicDeliverEventArgs(string routingKey, IBasicProperties basicProperties, IEnumerable<byte> body)
{
    public string RoutingKey { get; } = routingKey;
    public IBasicProperties BasicProperties { get; } = basicProperties;
    public IEnumerable<byte> Body { get; } = body;
}