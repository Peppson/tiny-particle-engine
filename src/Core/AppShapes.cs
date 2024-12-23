
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalConfig;
using GlobalStates;
using ColorPalette;

namespace monogame;

public partial class App : Game
{   
    // Shapes
    private Dictionary<State.ShapeType, Vector3[]> _shapes = [];
    private VertexPositionColor[] _currentShape = new VertexPositionColor[Config.totalShapeSize];
    private VertexPositionColor[] _centerCube;
    private readonly Vector3 _origin = new(0, 0, 0);


    public void LoadAllShapes() 
    {   
        _shapes.Add(State.ShapeType.Sphere,     CreateSphere());
        _shapes.Add(State.ShapeType.Torus,      CreateTorus());
        _shapes.Add(State.ShapeType.Frequency,  CreateFrequency());
        _shapes.Add(State.ShapeType.Pointcloud, CreatePointCloud());
        _centerCube =                           CreateCube();

        SetCurrentShape(Config.shapeOnStartup);
        InitAnimationComet(); // TODO
    }

    public void SetCurrentShape(State.ShapeType shapeType)
    {   
        Vector3[] shape = _shapes[shapeType];

        // LinQ here was about 80x slower than the for-loop ¯\_(ツ)_/¯
        for (int i = 0; i < _currentShape.Length; i++)
        {
            _currentShape[i].Position = shape[i];
            _currentShape[i].Color = Config.shapeColor;
        }
    }

    public void SetShapeColor(Color color)
    {   
        for (int i = 0; i < _currentShape.Length; i++)
        {   
            _currentShape[i].Color = color;
        }
    }

    public void SetCubeColor(Color color)
    {   
        for (int i = 0; i < _centerCube.Length; i++)
        {
            _centerCube[i].Color = color;
        }
    }

    private static Vector3[] CreatePointCloud(ushort size = Config.shapeSize)
    {
        var shape = new Vector3[size * size * size];
        int index = 0; 
        
        // Points in space!
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {   
                    // Map from -50 to 50 XYZ
                    float pointX = Map(x, 0, (size - 1), -50, 50);
                    float pointY = Map(y, 0, (size - 1), -50, 50);
                    float pointZ = Map(z, 0, (size - 1), -50, 50);
                                                    
                    shape[index++] = new Vector3(pointX, pointY, pointZ);                     
                } 
            }
        }

        return shape;
    }

    private static Vector3[] CreateTorus(ushort size = Config.shapeSize) // CreateGalaxy? Flat Torus? Ring?
    {
        const float radius = 75f;
        int totalSize = size * size * size;
        var shape = new Vector3[totalSize];
        Random random = new();

        // Draw a ring with some extra randomness sprinkled on top
        for (int i = 0; i < totalSize; i++)
        {
            float angle = MathHelper.TwoPi * i / totalSize;
            float X = radius * (float)Math.Cos(angle);
            float Z = radius * (float)Math.Sin(angle);

            shape[i] = new Vector3(
                X + random.Next(-15, 15), 
                0 + random.Next(-2, 2), 
                Z + random.Next(-15, 15)
            ); 
        }

        return shape;
    }

    private static Vector3[] CreateFrequency(ushort size = Config.shapeSize) 
    {
        int totalSize = size * size * size;
        var shape = new Vector3[totalSize];

        // Values randomized until it looked good
        const float amplitude = 50f;
        const float freqX = 182.08478f;   
        const float freqY = 357.16693f;

        for (int i = 0; i < totalSize; i++) 
        {
            float t = i / (float)totalSize * MathHelper.TwoPi;

            float X = amplitude * (float)Math.Sin(freqX * t);
            float Y = amplitude * (float)Math.Sin(freqY * t);
            float Z = amplitude * (float)Math.Sin((freqX + freqY) * t);

            shape[i] = new Vector3(X, Y, Z);
        }

        return shape;
    }

    private static Vector3[] CreateSphere(float radius = 66f, ushort size = Config.shapeSize)
    {
        const float goldenRatio = 1.618034f;
        int totalSize = size * size * size;
        var shape = new Vector3[totalSize];        

        for (int i = 0; i < totalSize; i++) 
        {
            // Polar coordinates
            float theta = 2 * MathHelper.Pi * i / goldenRatio;
            float phi = (float)Math.Acos(1 - 2 * (i + 0.5f) / totalSize);

            // Polar to Cartesian coords (vocabulary++)
            float X = radius * (float)(Math.Sin(phi) * Math.Cos(theta));
            float Y = radius * (float)(Math.Sin(phi) * Math.Sin(theta));
            float Z = radius * (float)Math.Cos(phi);
            
            shape[i] = new Vector3(X, Y, Z);
        }

        return shape;
    }

    public static VertexPositionColor[] CreateComet()
    {
        const float phi = 1.618034f; // Golden ratio
        Color color = NordColors.Yellow;

        VertexPositionColor[] vertices =
        [
            new VertexPositionColor(new Vector3(-1,  phi, 0), color),
            new VertexPositionColor(new Vector3( 1,  phi, 0), color),
            new VertexPositionColor(new Vector3(-1, -phi, 0), color),
            new VertexPositionColor(new Vector3( 1, -phi, 0), color),

            new VertexPositionColor(new Vector3(0, -1, phi), color),
            new VertexPositionColor(new Vector3(0,  1, phi), color),
            new VertexPositionColor(new Vector3(0, -1, -phi), color),
            new VertexPositionColor(new Vector3(0,  1, -phi), color),

            new VertexPositionColor(new Vector3(phi, 0, -1), color),
            new VertexPositionColor(new Vector3(phi, 0,  1), color),
            new VertexPositionColor(new Vector3(-phi, 0, -1), color),
            new VertexPositionColor(new Vector3(-phi, 0,  1), color),

            new VertexPositionColor(new Vector3(-phi / 2, phi / 2, 0), color),
            new VertexPositionColor(new Vector3(phi / 2, -phi / 2, 0), color),
            new VertexPositionColor(new Vector3(-1.5f,  0.5f * phi, -0.5f), color),
            new VertexPositionColor(new Vector3(1.5f, -0.5f * phi, 0.5f), color),

            new VertexPositionColor(new Vector3(0.5f, 1.5f, -phi / 2), color),
            new VertexPositionColor(new Vector3(-0.5f, -1.5f, phi / 2), color),
            new VertexPositionColor(new Vector3(0.8f * phi, 0.8f, -0.5f), color),
            new VertexPositionColor(new Vector3(-0.8f * phi, -0.8f, 0.5f), color),

            new VertexPositionColor(new Vector3(0.9f, phi * 0.9f, -0.3f), color),
            new VertexPositionColor(new Vector3(-0.9f, -phi * 0.9f, 0.3f), color),
            new VertexPositionColor(new Vector3(phi * 0.7f, -0.6f, 0.8f), color),
            new VertexPositionColor(new Vector3(-phi * 0.7f, 0.6f, -0.8f), color),
        ];

        return vertices;
    }

    private static VertexPositionColor[] CreateCube(float size = Config.cubeScale)
    {   
        Color color = Config.cubeColor;
        
        VertexPositionColor[] cube =
        [
            new VertexPositionColor(new Vector3(-size, -size,  size), color),
            new VertexPositionColor(new Vector3( size, -size,  size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3(-size, -size,  size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3(-size,  size,  size), color),

            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3(-size,  size, -size), color),
            new VertexPositionColor(new Vector3( size,  size, -size), color),
            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3( size,  size, -size), color),
            new VertexPositionColor(new Vector3( size, -size, -size), color),

            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3(-size, -size,  size), color),
            new VertexPositionColor(new Vector3(-size,  size,  size), color),
            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3(-size,  size,  size), color),
            new VertexPositionColor(new Vector3(-size,  size, -size), color),

            new VertexPositionColor(new Vector3( size, -size, -size), color),
            new VertexPositionColor(new Vector3( size,  size, -size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3( size, -size, -size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3( size, -size,  size), color),

            new VertexPositionColor(new Vector3(-size,  size, -size), color),
            new VertexPositionColor(new Vector3(-size,  size,  size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3(-size,  size, -size), color),
            new VertexPositionColor(new Vector3( size,  size,  size), color),
            new VertexPositionColor(new Vector3( size,  size, -size), color),

            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3( size, -size, -size), color),
            new VertexPositionColor(new Vector3( size, -size,  size), color),
            new VertexPositionColor(new Vector3(-size, -size, -size), color),
            new VertexPositionColor(new Vector3( size, -size,  size), color),
            new VertexPositionColor(new Vector3(-size, -size,  size), color),
        ];

        return cube;
    }

    // C++ copy pasta
    private static float Map(float input, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (toMax - toMin) * ((input - fromMin) / (fromMax - fromMin));
    }
}
