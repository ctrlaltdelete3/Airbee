namespace Airbee.Shared.Messages;

// Used by both Collector (publish) and Processing (consume), so they always match.
public static class QueueNames
{
    public const string Measurements = "measurements";
}
