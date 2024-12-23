
namespace GlobalTypeDefinitions;

public enum ShapeType
{
    Sphere, 
    Torus, 
    Frequency, 
    Pointcloud
}

public enum AnimationType
{
    None, 
    Gravity, 
    AntiGravity, 
    Implode, 
    Lerp, 
    Fusion, 
    Collapse
}

public struct CameraState
{
    public int RotationX { get; set; }
    public int RotationY { get; set; }
    public int Zoom { get; set; }
}
