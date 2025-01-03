
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Utility;

public static class Utils
{
    private static readonly Stopwatch _timer = new();
    private static bool _isTimerRunning = false;


    public static void PrintArrayInfo<T>(T[] array, string name)
    {
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        IntPtr address = handle.AddrOfPinnedObject();

        Console.WriteLine($"{array} Address: {address}");
        Console.WriteLine($"{name} Total Size: {Marshal.SizeOf(typeof(T)) * array.Length} bytes");

        handle.Free();
    }

    public static void BenchmarkStart()
    {   
        Debug.Assert(!_isTimerRunning, "Timer is already running!");

        _isTimerRunning = true;
        Console.Write("> Timer ON\n");
        _timer.Restart();
    }

    public static void BenchmarkStop(string message = "")
    {   
        Debug.Assert(_isTimerRunning, "Timer is not running. Please start the timer before stopping it!");

        _timer.Stop();
        _isTimerRunning = false;

        // Format result
        var elapsedTime = _timer.Elapsed;
        string result = FormatTimeBenchmark(elapsedTime);

        // Output
        Console.Write($"> Timer OFF ");
        if (message != "") 
        {
            PrintColor($"\"{message}\" ", ConsoleColor.Green);
        }

        PrintColor($"{result}\n", ConsoleColor.Blue);
    }

    public static void PrintColor(string txt, ConsoleColor color) 
    {
        Console.ForegroundColor = color;
        Console.Write(txt);
        Console.ResetColor();
    }

    public static void STOP()
    {   
        #if DEBUG
            PrintColor("\n> STOP\n", ConsoleColor.Red);
            Console.ReadKey(intercept: true);
            PrintColor("> Continue...\n", ConsoleColor.Green);
        #endif
    }

    private static string FormatTimeBenchmark(TimeSpan elapsedTime)
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
