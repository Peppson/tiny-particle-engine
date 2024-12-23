
using System;
using System.Diagnostics;

namespace UtilityClass;

public static class Utility
{
    private static readonly Stopwatch _timer = new Stopwatch();
    private static bool _isTimerRunning = false;

    
    public static void BenchmarkStart()
    {   
        Debug.Assert(!_isTimerRunning, "Timer is already running!");

        _isTimerRunning = true;
        Console.Write("---- Timer started ----\n");
        _timer.Restart();
    }

    public static void BenchmarkStop(string message = "")
    {   
        Debug.Assert(_isTimerRunning, "Timer is not running. Please start the timer before stopping it!");

        _timer.Stop();
        _isTimerRunning = false;

        // Format result to string
        var elapsedTime = _timer.Elapsed;
        string result = BenchmarkFormatTime(elapsedTime);

        // Print
        Console.Write($"\n---- Timer stopped ----\n");
        if (message != "")
        {
            Console.Write($"> {message}\n");
        }
        Console.Write($"> {result}\n\n");
    }

    private static string BenchmarkFormatTime(TimeSpan elapsedTime)
    {
        // Microseconds
        if (elapsedTime.TotalMilliseconds < 1)
        {
            return $"{elapsedTime.TotalMicroseconds:0.#} Microseconds";
        }
        // Milliseconds
        else if (elapsedTime.TotalSeconds < 1)
        {
            return $"{elapsedTime.TotalMilliseconds:0.#} Milliseconds";
        }
        // Format as 00:00:00
        else
        {
            return $"{elapsedTime:hh\\:mm\\:ss},{elapsedTime.Milliseconds}";
        }
    }
}
