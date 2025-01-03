
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App
{   
    private class Asteroid(VertexPositionColor[] vertexData,
                           Vector3 oldPosition, 
                           Vector3 startPoint, 
                           Vector3 endPoint, 
                           float lerpFactor)
    {
        public VertexPositionColor[] vertexData  = vertexData;
        public Vector3 oldPosition = oldPosition;
        public Vector3 startPoint = startPoint;
        public Vector3 endPoint = endPoint;
        public float lerpFactor = lerpFactor;
    }
    
    public static bool IsAsteroidActive { get; set; } = false;
    public static bool MakeNewAsteroid { get; set; } = false;
    public static int AsteroidCount { get; private set; } = 0;
    private static List<Asteroid> _asteroids = [];
    private static Vector3[] _debrisDirection = new Vector3[Config.totalShapeSize];
    private static bool[] _isDebris = new bool[Config.totalShapeSize];
    private static Matrix _rotationMatrix;
    private const float _asteroidVelocity = 0.0018f;

    #if DEBUG_COMET
        public VertexPositionColor[] _debugAsteroid = GetCube(1.1f);
    #endif


    public void ShapeAsteroid(MouseState mouseState)
    {   
        // Enabled?
        if (!IsAsteroidActive) return;

        if (CreateNewAsteroid())
        {
            Asteroid asteroid = GetNewAsteroid(mouseState);
            _asteroids.Add(asteroid);
        }
        // Movement and collision detection
        if (AsteroidCount > 0)
        {
            UpdateAndProcessAsteroids();
        }
        // Keep moving debris even if there is 0 active Asteroids
        MoveDebris();
    }

    private Asteroid GetNewAsteroid(MouseState mouseState)
    {   
        var (startPoint, endPoint) = GetTrajectory(mouseState);

        return new(
            vertexData:     (VertexPositionColor[])_miscShapes[ShapeType.Asteroid].Clone(),
            oldPosition:    _origin,
            startPoint:     startPoint,
            endPoint:       endPoint,
            lerpFactor:     0
        );
    }

    private (Vector3, Vector3) GetTrajectory(MouseState mouseState)
    {   
        Vector3 mouseScreenPos = new(mouseState.X, mouseState.Y, 0f);
        Vector3 mouseFarScreenPos = new(mouseState.X, mouseState.Y, 1f);

        // Viewport, projection, and view matrices
        Viewport viewport = _cam.mainViewport.viewport;
        Matrix projection = _cam.mainViewport.projection;
        Matrix view = _cam.mainViewport.view;

        // 2D screenspace to 3D worldspace
        Vector3 nearPoint = viewport.Unproject(mouseScreenPos, projection, view, Matrix.Identity);
        Vector3 farPoint = viewport.Unproject(mouseFarScreenPos, projection, view, Matrix.Identity);

        // While rotating "camera" we are actually changing the whole worldview...
        Matrix worldRotation = 
            Matrix.CreateRotationX(- _cam.worldMatrixRotation.Y) * 
            Matrix.CreateRotationY(- _cam.worldMatrixRotation.X);

        nearPoint = Vector3.Transform(nearPoint, worldRotation);
        farPoint = Vector3.Transform(farPoint, worldRotation);

        // Start and end vecs
        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
        Vector3 startPos = nearPoint + direction * 10f; // Z near
        Vector3 endPos = nearPoint + direction * 400f;  // Z far

        return (startPos, endPos);
    } 

    private static void UpdateAndProcessAsteroids()
    {           
        int indexToRemove = int.MaxValue;

        // Calculate!
        for (int i = 0; i < _asteroids.Count; i++)
        {
            Vector3 pos = GetPosition(_asteroids[i]);

            if (IsOutOfBounds(pos))
            {
                indexToRemove = i;
                continue;
            }

            MoveAsteroid(_asteroids[i], pos);
            RotateAsteroid(_asteroids[i], pos);
            CheckCollisions(pos);
        }
        // Remove finished asteroids
        if (indexToRemove != int.MaxValue)
        {
            _asteroids.RemoveAt(indexToRemove);
            SetAsteroidCount(AsteroidCount - 1);
        }
    }

    private static Vector3 GetPosition(Asteroid asteroid)
    {
        // Update asteroid position along its trajectory
        asteroid.lerpFactor += _asteroidVelocity * State.GameSpeedFactor;

        return Vector3.Lerp(
            asteroid.startPoint, 
            asteroid.endPoint, 
            asteroid.lerpFactor
        );  
    }

    private static bool IsOutOfBounds(Vector3 Position)
    {   
        const float threshold = 700;

        if (Math.Abs(Position.Z) > threshold ||
            Math.Abs(Position.Y) > threshold || 
            Math.Abs(Position.X) > threshold)
        {
            return true;
        }

        return false;
    }

    private static void MoveAsteroid(Asteroid asteroid, Vector3 curPos)
    {   
        // Position delta 
        Vector3 movementDelta = curPos - asteroid.oldPosition;
        asteroid.oldPosition = curPos;

        for (int i = 0; i < asteroid.vertexData.Length; i++)
        {
            asteroid.vertexData[i].Position += movementDelta;
        }
    }

    private static void RotateAsteroid(Asteroid asteroid, Vector3 curPos)
    {   
        // Rotate each point around center of mass
        // Pos = Vector3.Transform(oldPos - curPos, _rotationMatrix) + curPos;
        for (int i = 0; i < asteroid.vertexData.Length; i++)
        {
            asteroid.vertexData[i].Position = Vector3.Transform(
                asteroid.vertexData[i].Position - curPos, 
                _rotationMatrix) + curPos;
        }
    }

    private static void CheckCollisions(Vector3 curPos)
    {   
        if (State.IgnoreAsteroids) return;

        const float threshold = 30f;
        float animationSpeed = State.GameSpeedFactor;
        bool gravityAtFloor = State.GravityState == GravityType.AtFloor;
        var shape = (State.RenderTarget == RenderType.Normal) ? _currentShape : _currentShapeCopy;

        // Crunch it!
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            float distance = Vector3.Distance(curPos, shape[i].Position);
            if (distance >= threshold) continue;
            
            // Debris direction
            Vector3 directionAway = shape[i].Position - curPos;
            directionAway.Normalize();

            // Closer points gets more velocity
            float velocity = Map(
                distance, 
                threshold - (0.2f * animationSpeed), 
                threshold, 
                1f * animationSpeed, 
                1.5f * animationSpeed
            ); 
            // Move!
            _currentShape[i].Position += directionAway * velocity;

            // Collapse explosions to 2D if gravity
            if (gravityAtFloor) _currentShapeCopy[i].Position.Y = NewFloorPositionY;

            // Store impacted points as "debris" for future frames
            _debrisDirection[i] = directionAway;
            _isDebris[i] = true;
        }
    }

    private static void MoveDebris()
    {   if (State.IgnoreAsteroids) return;
    
        float velocity = 0.05f * State.GameSpeedFactor;

        for (int i = 0; i < Config.totalShapeSize; i++) 
        {
            if (_isDebris[i])
            {
                _currentShape[i].Position += _debrisDirection[i] * velocity;
                _currentShape[i].Color = Config.asteroidColor;
            }
        }
    }

    private static bool CreateNewAsteroid()
    {   
        if (MakeNewAsteroid && AsteroidCount < Config.asteroidMaxCount)
        {
            SetAsteroidCount(AsteroidCount + 1);
            MakeNewAsteroid = false;
            return true;
        }

        MakeNewAsteroid = false;
        return false;
    }
    
    public static void SetAsteroidCount(int num) 
    {
        AsteroidCount = Math.Clamp(num, 0, Config.asteroidMaxCount); 
    }

    public void ShapeAsteroidReset()
    {   
        IsAsteroidActive = false;
        MakeNewAsteroid = false;
        AsteroidCount = 0;
        _asteroids.Clear();
        Array.Clear(_isDebris);
    }

    public static void ShapeAsteroidSoftReset()
    {   
        if (State.CurrentAnimation == AnimationType.Collapse &&
            State.IsFirstCollapse)
        {
            return;
        }
        Array.Clear(_isDebris);
    }


    #if DEBUG_COMET
        private void DebugPrintAsteroid(Asteroid asteroid)
        {   
            Console.WriteLine($"\nPosition:\t{asteroid.vertexData[0].Position}");
            Console.WriteLine($"oldPosition:\t{asteroid.oldPosition}");
            Console.WriteLine($"startPoint:\t{asteroid.startPoint}");
            Console.WriteLine($"endPoint:\t{asteroid.endPoint}");
            Console.WriteLine($"lerpFactor:\t{asteroid.lerpFactor}");
        }

        private static int[] DebugLoadAsteroidLines()
        {
            var lines = File.ReadAllLines("Lines.txt");
            List<int> indices = [];

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (parts.Length == 2)
                {
                    indices.Add(int.Parse(parts[0]));
                    indices.Add(int.Parse(parts[1]));
                }
            }

            return indices.ToArray();
        }
    #endif
}
