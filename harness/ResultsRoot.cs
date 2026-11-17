namespace DeclarativeIpmEval;

/// <summary>Where the results of the study are written: data, so OUTSIDE the code repositories. The env variable DECLARATIVEIPM_RESULTS overrides it.</summary>
public static class ResultsRoot
{
    public const string Default = @"<results folder>";

    public static string Get() => Environment.GetEnvironmentVariable("DECLARATIVEIPM_RESULTS") is { Length: > 0 } e ? e : Default;
}
