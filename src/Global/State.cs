
using Color = Microsoft.Xna.Framework.Color;

namespace Particle.Global;

public static class State
{
    public static ShapeType CurrentShape { get; set; }
    public static Color CurrentColor { get; set; }
    public static RenderType RenderTarget { get; set; }
    public static double CurrentGameTime { get; set; }
    public static float GameSpeedFactor { get; set; }
    public static bool IsStartup { get; set; } = Config.showStartupMenu;

    // Toggle buttons
    public static bool IsPaused { get; set; }
    public static bool IsEffect { get; set; }
    public static bool IsGravity { get; set; }

    // Animations
    public static AnimationType CurrentAnimation { get; set; }
    public static ColorAnimationType CurrentColorAnimation { get; set; }
    public static GravityType GravityState { get; set; }
    public static float RotationEaseInFactor { get; set; }
    public static bool IgnoreAsteroids { get; set; }
    public static bool IsFirstGravity { get; set; }
    public static bool IsFirstFusion { get; set; } 
    public static bool IsFirstImplode { get; set; }
    public static bool IsFirstCollapse { get; set; }
    public static bool IsShotMeEnabled { get; set; }
    public static bool IsMemeEnabled { get; set; }


    static State() // Init
    {
        Reset(); 
    }

    public static void Reset()
    {
        CurrentShape = Config.shapeOnStartup;
        CurrentColor = Config.shapeColor;
        CurrentGameTime = 0;
        GameSpeedFactor = 0;
        RenderTarget = RenderType.Normal;

        // Buttons
        IsPaused = false;
        IsEffect = false;
        IsGravity = false;

        // Animation states
        CurrentAnimation = AnimationType.None;
        CurrentColorAnimation = ColorAnimationType.None;
        GravityState = GravityType.None;
        RotationEaseInFactor = 1;
        IgnoreAsteroids = false;
        IsFirstFusion = true;
        IsFirstGravity = true;
        IsFirstImplode = true;
        IsFirstCollapse = true;
        IsShotMeEnabled = false;
        IsMemeEnabled = false;
    }
}
