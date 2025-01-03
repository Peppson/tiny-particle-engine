
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App 
{   
    private static float NewFloorPositionY { get; set; } = 0f;
    private static float[] _snapshotPosY = new float[Config.totalShapeSize];
    private float _gravityAcceleration = 1f;
    private float _elapsedGravityTime = 0;
    private bool _isfirstGravityUp = true;


    public void ShapeGravity(float deltaTime)
    {   
        // Enabled?
        if (State.GravityState == GravityType.None) return;
       
        // In Gravity mode, transform a copy of _currentShape instead.
        // 3D always happens, gravity just slowly morphs the copy into 2D
        EnableGravityRendering();

        switch (State.GravityState)
        {
            case GravityType.Gravity:       GravityDown(deltaTime); break;
            case GravityType.AtFloor:       GravityAtFloor();       break;
            case GravityType.AntiGravity:   GravityUp(deltaTime);   break;
        }

        #if DEBUG_GRAVITY
            DebugGravity();
        #endif
    }   

    public void ShapeGravityReset()
    {   
        State.RenderTarget = RenderType.Normal;
        State.GravityState = GravityType.None;
        State.IsFirstGravity = true;
        State.IsGravity = false;
        _gravityAcceleration = 1f;
        _elapsedGravityTime = 0;
    }

    private void GravityDown(float deltaTime)
    {   
        _elapsedGravityTime += deltaTime;
        _gravityAcceleration += 0.016f;

        // Take snapshot of "_currentShape" Y-pos and set floor level
        if (State.IsFirstGravity)
        {
            TakeSnapshot(_currentShape);
            SetFloorFromCamZoom();
            _isfirstGravityUp = true;
            _gravityAcceleration = 1f;
            _elapsedGravityTime = 0;
        }
        // Animation done?
        if (GravityDownUpdate() || _elapsedGravityTime > 1600f)
        {
            State.GravityState = GravityType.AtFloor; 
            #if ANIMATION_LOGGING
                Console.WriteLine("Gravity down, done!");
            #endif
        }
    }

    private bool GravityDownUpdate()
    {   
        float speed = _gravityAcceleration * State.GameSpeedFactor * 0.4f;
        bool isAnimationDone = true;

        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            // At floor?
            if (_snapshotPosY[i] > NewFloorPositionY - 1f &&
                _snapshotPosY[i] < NewFloorPositionY + 1f)
            {
                _currentShapeCopy[i].Position.Y = _snapshotPosY[i] = NewFloorPositionY;
                continue;
            }
            // Down/up
            float oldPosition = 0;
            if (_snapshotPosY[i] > NewFloorPositionY) 
            {
                oldPosition = _snapshotPosY[i];
                _snapshotPosY[i] -= speed * speed;
            }
            else 
            {
                oldPosition = _snapshotPosY[i];
                _snapshotPosY[i] += speed * speed * 1.5f;
            }

            // Capture outliers with way to high velocity
            if (oldPosition > _snapshotPosY[i] && _snapshotPosY[i] < NewFloorPositionY)
            {
                _snapshotPosY[i] = NewFloorPositionY;
            }
            // Set new Y positions
            _currentShapeCopy[i].Position.Y = _snapshotPosY[i];
            isAnimationDone = false;
        }

        return isAnimationDone;
    }

    private void GravityAtFloor()
    {   
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            _currentShapeCopy[i].Position.Y = NewFloorPositionY;
        }
    }

    private void GravityUp(float deltaTime) 
    {   
        float speed = _gravityAcceleration * State.GameSpeedFactor * 0.03f;
        _gravityAcceleration += 0.016f;
        _elapsedGravityTime += deltaTime;

        if (_isfirstGravityUp)
        {   
            _isfirstGravityUp = false;
            _gravityAcceleration = 1f;
            _elapsedGravityTime = 0;
        }
        // Linear interpolation 
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            _currentShapeCopy[i].Position = Vector3.Lerp(_currentShapeCopy[i].Position, _currentShape[i].Position, speed);
        }
        // Animation done?
        if (_elapsedGravityTime > 700f)
        {   
            ShapeGravityReset();
            #if ANIMATION_LOGGING
                Console.WriteLine("Gravity up, done!");
            #endif
        }
    }

    private void EnableGravityRendering()
    {
        State.RenderTarget = RenderType.Gravity;
        
        // Workaround...
        if (State.GravityState != GravityType.AntiGravity)
        {
            _currentShape.CopyTo(_currentShapeCopy, 0);
        } 
    }

    private void SetFloorFromCamZoom()
    {   
        NewFloorPositionY = _cam.cameraPosition.Z / 4;
        if (NewFloorPositionY > -20f) NewFloorPositionY = -20f; 
    }

    private void TakeSnapshot(VertexPositionColor[] target)
    {   
        for (int i = 0; i < Config.totalShapeSize; i++)
            _snapshotPosY[i] = target[i].Position.Y;

        State.IsFirstGravity = false;
    }

    private void TakeSnapshot(float target)
    {   
        for (int i = 0; i < Config.totalShapeSize; i++)
            _snapshotPosY[i] = target;
        
        State.IsFirstGravity = false;
    }

    public void AnimationBeginFromGravity()
    {   
        if (State.GravityState == GravityType.None) return;
        
        _currentShapeCopy.CopyTo(_currentShape, 0);
        ShapeGravityReset();

        #if ANIMATION_LOGGING
            Console.WriteLine("Animation started from gravity!");
        #endif
    }

    #if DEBUG_GRAVITY
        private void DebugGravity()
        {
            State.RenderTarget = RenderType.Gravity;
            
            for (int i = 0; i < Config.totalShapeSize; i++)
            {   
                _currentShapeCopy[i].Color = ColorPalette.Yellow;
            }
        }
    #endif
}