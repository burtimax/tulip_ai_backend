namespace Infrastructure.Db.App.Entities;

public enum ChatStatus
{
    Idle = 0,
    Processing = 1,
    Error = 2
}

public enum MessageRole
{
    User = 0,
    Assistant = 1,
    System = 2
}

public enum MessageStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public enum JobStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
