using System;

public class Progress
{
    public float PercentageComplete { get; private set; }
    public string Status { get; private set;  }

    public event Action<Progress> OnProgressed;

    public void ReportProgress(float complete, string message)
    {
        PercentageComplete = complete;
        Status = message;

        OnProgressed?.Invoke(this);
    }
}
