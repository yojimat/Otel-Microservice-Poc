// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Utils.Messaging;

public class MessageSender(ILogger<MessageSender> logger)
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageSender));
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public string SendMessage(string activityName)
    {
        try
        {
            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
            var activityNameMessage = $"{activityName} send";

            using var activity = ActivitySource.StartActivity(activityNameMessage, ActivityKind.Producer);

            // Depending on Sampling (and whether a listener is registered or not), the
            // activity above may not be created.
            // If it is created, then propagate its context.
            // If it is not created, the propagate the Current context,
            // if any.
            ActivityContext contextToInject = default;
            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            var props = new BasicProperties();

            // Inject the ActivityContext into the message headers to propagate trace context to the receiving service.
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), props, InjectTraceContextIntoBasicProperties);

            // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
            var body = $"Published message: DateTime.Now = {DateTime.Now}.";

            logger.LogInformation($"Message sent: [{body}]");

            return body;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message publishing failed.");
            throw;
        }
    }

    private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
    {
        try
        {
            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}

public interface IBasicProperties
{
    Dictionary<string, object> Headers { get; set; }
}

public class BasicProperties : IBasicProperties
{
    public Dictionary<string, object> Headers { get; set; } = [];
}