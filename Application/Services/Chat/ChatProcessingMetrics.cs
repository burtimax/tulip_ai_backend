using System.Diagnostics.Metrics;

namespace Application.Services.Chat;

public sealed class ChatProcessingMetrics
{
    private readonly Counter<long> _jobsProcessed;
    private readonly Counter<long> _jobsFailed;
    private readonly Histogram<double> _queueWaitSeconds;
    private readonly Histogram<double> _processingSeconds;
    private readonly Histogram<double> _plantIdLatencySeconds;
    private readonly Histogram<double> _llmLatencySeconds;

    public ChatProcessingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("TulipAI.ChatPipeline");
        _jobsProcessed = meter.CreateCounter<long>("chat_jobs_processed");
        _jobsFailed = meter.CreateCounter<long>("chat_jobs_failed");
        _queueWaitSeconds = meter.CreateHistogram<double>("chat_queue_wait_seconds");
        _processingSeconds = meter.CreateHistogram<double>("chat_processing_seconds");
        _plantIdLatencySeconds = meter.CreateHistogram<double>("chat_plantid_latency_seconds");
        _llmLatencySeconds = meter.CreateHistogram<double>("chat_llm_latency_seconds");
    }

    public void RecordQueueWait(TimeSpan wait) => _queueWaitSeconds.Record(wait.TotalSeconds);
    public void RecordProcessingTime(TimeSpan duration) => _processingSeconds.Record(duration.TotalSeconds);
    public void RecordPlantIdLatency(TimeSpan duration) => _plantIdLatencySeconds.Record(duration.TotalSeconds);
    public void RecordLlmLatency(TimeSpan duration) => _llmLatencySeconds.Record(duration.TotalSeconds);
    public void MarkProcessed() => _jobsProcessed.Add(1);
    public void MarkFailed() => _jobsFailed.Add(1);
}
