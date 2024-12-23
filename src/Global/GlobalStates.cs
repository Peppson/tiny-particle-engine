
using Color = Microsoft.Xna.Framework.Color;
using GlobalTypeDefinitions;
using GlobalConfig;

namespace GlobalStates;

public static class State
{
    public static ShapeType CurrentShape { get; set; }
    public static Color CurrentColor { get; set; }
    public static float GameSpeedFactor { get; set; }
    public static double GameTotalTime { get; set; } 

    // Animation states
    public static double AnimationStartTime { get; set; }
    public static AnimationType CurrentAnimation { get; set; }
    public static AnimationType PreviousAnimation { get; set; }
    public static bool AutoRotation { get; set; }
    public static float AutoRotationEaseInFactor { get; set; }
    public static bool Gravity { get; set; }
    public static bool IsFirstFusion { get; set; } 
    public static bool IsFirstGravity { get; set; }
    public static bool IsFirstImplode { get; set; }
    public static bool IsFirstCollapse { get; set; }
    public static int CollapsePosY { get; set; }


    static State()
    {
        Reset(); // Init values
    }

    public static void Reset()
    {
        CurrentShape = Config.shapeOnStartup;
        CurrentColor = Config.shapeColor;
        GameSpeedFactor = 0;
        GameTotalTime = 0;

        // Animation states
        AnimationStartTime = 0;
        CurrentAnimation = AnimationType.None;
        PreviousAnimation = AnimationType.None;
        AutoRotation = Config.autoRotationOnStartup;
        AutoRotationEaseInFactor = 1;
        Gravity = false;
        IsFirstFusion = true;
        IsFirstGravity = true;
        IsFirstImplode = false;
        IsFirstCollapse = true;
        CollapsePosY = 50;
    }
}
