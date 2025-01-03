
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App
{   
    private static readonly Dictionary<ShapeType, Vector3[]> _shapes = [];
    private static readonly Dictionary<ShapeType, VertexPositionColor[]> _miscShapes = [];    
    private static VertexPositionColor[] _currentShape = new VertexPositionColor[Config.totalShapeSize];
    private static VertexPositionColor[] _currentShapeCopy = new VertexPositionColor[Config.totalShapeSize];
    private static readonly Vector3 _origin = new(0, 0, 0);
    private static Model _model;
    private static Matrix _modelRotation;
  

    public void LoadAllShapes()
    {   
        _shapes.Add(ShapeType.Frequency,        GetFrequency());
        _shapes.Add(ShapeType.Sphere,           GetSphere());
        _shapes.Add(ShapeType.Impact,           GetSphereSmall());
        _shapes.Add(ShapeType.EvenPointcloud,   GetEvenPointCloud());
        _shapes.Add(ShapeType.Pointcloud,       GetPointCloud());

        _miscShapes.Add(ShapeType.Asteroid,     GetAsteroid());
        _miscShapes.Add(ShapeType.CenterCube,   GetCube());
        
        SetCurrentShape(Config.shapeOnStartup);
    }

    public static void SetCurrentShape(ShapeType shapeType)
    {   
        Vector3[] shape = _shapes[shapeType];

        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            _currentShape[i].Position = shape[i];
            _currentShape[i].Color = Config.shapeColor;
        }
    }
    
    private static Vector3[] GetSphere(float offset = 1f)
    {   
        const int totalSize = Config.totalShapeSize;
        const float radius = 66f;

        float spiralOffset = 700f * offset;
        var noise = _shapes[ShapeType.Frequency];
        var sphere = new Vector3[totalSize];
             
        Parallel.For(0, totalSize, i => {

            // Polar coordinates
            float theta = 2 * MathHelper.Pi * i / spiralOffset;
            float phi = (float)Math.Acos(1 - 2 * (i + 0.5f) / totalSize);

            // Polar to Cartesian coords (vocabulary++)
            float X = radius * (float)(Math.Sin(phi) * Math.Cos(theta));
            float Y = radius * (float)(Math.Sin(phi) * Math.Sin(theta));
            float Z = radius * (float)Math.Cos(phi);
            
            // Set sphere and add some noise (Lerp) 
            sphere[i] = Vector3.Lerp(new Vector3(X, Y, Z), noise[i], 0.016f);
        });

        return sphere;
    }

    private static Vector3[] GetSphereSmall()
    {   
        const int totalSize = Config.totalShapeSize;
        const float goldenRatio = 1.618034f;
        const float radius = 14f;
        var sphere = new Vector3[totalSize];
             
        Parallel.For(0, totalSize, i => {

            // Polar coordinates
            float theta = 2 * MathHelper.Pi * i / goldenRatio;
            float phi = (float)Math.Acos(1 - 2 * (i + 0.5f) / totalSize);

            // Polar to Cartesian coords (vocabulary++)
            float X = radius * (float)(Math.Sin(phi) * Math.Cos(theta));
            float Y = radius * (float)(Math.Sin(phi) * Math.Sin(theta));
            float Z = radius * (float)Math.Cos(phi);
            
            // Set sphere
            sphere[i] = new Vector3(X, Y, Z);
        });

        return sphere;
    }

    private static Vector3[] GetFrequency(float offset = 1f)
    {
        // Values randomized until it looked good
        const int totalSize = Config.totalShapeSize;
        const float amplitude = 50f;
        float freqX = 182.08478f + offset;
        float freqY = 657.16693f + offset;
        var frequency = new Vector3[totalSize];

        Parallel.For(0, totalSize, i => {
            float t = i / (float)totalSize * MathHelper.TwoPi;

            float X = amplitude * (float)Math.Sin(freqX * t);
            float Y = amplitude * (float)Math.Sin(freqY * t);
            float Z = amplitude * (float)Math.Sin((freqX + freqY) * t);

            frequency[i] = new Vector3(X, Y, Z);
        });

        return frequency;
    }

    private static Vector3[] GetPointCloud(float effectOffset = 0f)
    {   
        const int limit = Config.shapeSize1D * 2;
        const int cubeGap = 4;
        const float offset = 4.3f;
        var pointcloud = new Vector3[Config.totalShapeSize];
        var lerpTarget = _shapes[ShapeType.EvenPointcloud];
        int index = 0;

        // Space out small cubes inside a "larger" cube
        for (int x = 0; x < limit; x++)
        { 
            if ((x / cubeGap) % 2 != 0) continue;

            for (int y = 0; y < limit; y++)
            { 
                if ((y / cubeGap) % 2 != 0) continue;

                for (int z = 0; z < limit; z++)
                { 
                    if ((z / cubeGap) % 2 != 0) continue;

                    // Map XYZ to -48, 48
                    float pointX = Map(x, 0, (limit - 1), -limit - cubeGap, limit + cubeGap);   
                    float pointY = Map(y, 0, (limit - 1), -limit - cubeGap, limit + cubeGap);
                    float pointZ = Map(z, 0, (limit - 1), -limit - cubeGap, limit + cubeGap);
                    
                    // Set and add noise (Lerp to another shape)
                    pointcloud[index] = Vector3.Lerp(
                        new Vector3(
                            pointX + offset, 
                            pointY + offset, 
                            pointZ + offset
                        ),
                        lerpTarget[index],
                        effectOffset
                    );
                    index++;
                }
            }
        }

        return pointcloud;
    }

    private static Vector3[] GetEvenPointCloud()
    {
        const int size1D = Config.shapeSize1D;
        const int limit = 48;
        int index = 0; 
        var pointcloud = new Vector3[Config.totalShapeSize];

        // Points in space!
        for (int x = 0; x < size1D; x++)
        {
            for (int y = 0; y < size1D; y++)
            {
                for (int z = 0; z < size1D; z++)
                {   
                    // Map XYZ to -48, 48 
                    float X = Map(x, 0, (size1D - 1), -limit, limit);
                    float Y = Map(y, 0, (size1D - 1), -limit, limit);
                    float Z = Map(z, 0, (size1D - 1), -limit, limit);   
                                            
                    pointcloud[index] = new Vector3(X, Y, Z);
                    index++;                  
                } 
            }
        }
        
        return pointcloud;
    }

    private static Vector3[] GetTorus() // CreateGalaxy?
    {
        const float radius = 72f;
        const int totalSize = Config.totalShapeSize;
        const int ringWidth = 15;
        var torus = new Vector3[totalSize];
        var random = new Random();
 
        // Draw a ring with some extra randomness sprinkled on top
        for (int i = 0; i < totalSize; i++)
        {
            float angle = MathHelper.TwoPi * i / totalSize;
            float X = radius * (float)Math.Cos(angle);
            float Z = radius * (float)Math.Sin(angle);

            torus[i] = new Vector3(
                X + random.Next(-ringWidth, ringWidth), 
                0 + random.Next(-2, 2), 
                Z + random.Next(-ringWidth, ringWidth)
            );
        }

        return torus;
    }

    private static VertexPositionColor[] GetAsteroid()
    {
        const float phi = 1.618034f; // Golden ratio
        Color color = Config.asteroidColor;

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

    private static VertexPositionColor[] GetCube(float size = Config.cubeScale)
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

    private static float Map(float input, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (toMax - toMin) * ((input - fromMin) / (fromMax - fromMin));
    }

    private void BenchmarkCopyToFunc() 
    {   
        const int times = 10000;
        Utils.BenchmarkStart();

        // 10000 = 53ms, 1 = 15Ms
        for (int i = 0; i < times; i++) 
        { 
            _currentShape.CopyTo(_currentShapeCopy, 0);
        } 

        // 10000 = 10ms, 1 = 40Ms
        Parallel.For(0, times, i => 
        { 
            _currentShape.CopyTo(_currentShapeCopy, 0); 
        });

        Utils.BenchmarkStop(); 
        Utils.STOP();
    }
}
