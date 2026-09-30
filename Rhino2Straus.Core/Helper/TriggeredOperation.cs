using System;

namespace Rhino2Straus.Core.Helper;

/// <summary>Runs expensive work only on a false-to-true transition and invalidates stale output when inputs change.</summary>
public sealed class TriggeredOperation<T> where T : class
{
    private bool previousRun;
    private string previousKey;
    public T Value { get; private set; }
    public string Status { get; private set; } = "Idle";

    public T Update(string key, bool run, Func<T> operation, Func<T,string> completed, string waiting="Run required")
    {
        bool fire=run&&!previousRun;
        previousRun=run;
        if(key!=previousKey)
        {
            previousKey=key;
            Value=null;
            Status=waiting;
        }
        if(fire)
        {
            Value=null;
            Status="Failed";
            var result=operation();
            string status=completed(result);
            Value=result;
            Status=status;
        }
        return Value;
    }
}
