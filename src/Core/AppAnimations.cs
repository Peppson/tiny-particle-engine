
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App
{
    private HashSet<int> _transformedPoints = new(Config.totalShapeSize);
    private float _elapsedAnimationTime = 0;
    private bool _collapseStageClear = true;
    private double _oldTime = 0;
    private float _currentPosY = _posYDefault;
    private const float _posYDefault = 250;


    public void ShapeAnimation(float deltaTime)
    {   
        // Any ongoing animation?
        if (State.CurrentAnimation == AnimationType.None) return;

        switch (State.CurrentAnimation)
        {
            case AnimationType.Fusion:       Fusion();              break;
            case AnimationType.Lerp:         Lerp();                break;
            case AnimationType.Collapse:     Collapse();            break;
            case AnimationType.Implode:      Implode(deltaTime);    break;
        }
    }

    public void ShapeAnimationsReset()
    {   
        State.IsFirstFusion = true;
        State.IsFirstCollapse = true;
        State.IsFirstImplode = true;
        State.IgnoreAsteroids = false;
        State.IsShotMeEnabled = false;
        _currentPosY = _posYDefault;
        _elapsedAnimationTime = 0;
        _collapseStageClear = true;
    }

    private Vector3 GetDirectionFromOrigin(int i)
    {
        return Vector3.Normalize(_origin - _currentShape[i].Position);
    }

    private float GetDistanceFromOrigin(int i)
    {
        return Math.Abs(_currentShape[i].Position.X - _origin.X) + 
               Math.Abs(_currentShape[i].Position.Y - _origin.Y) + 
               Math.Abs(_currentShape[i].Position.Z - _origin.Z);
    }

    private void Lerp()
    {   
        if (LerpShape(State.CurrentShape))
        {
            State.CurrentAnimation = AnimationType.None;
            #if ANIMATION_LOGGING     
                Console.WriteLine("Lerp done!"); 
            #endif
        }
    }

    private bool LerpShape(ShapeType shapeType)
    {   
        const float tolerance = 0.15f;
        float factor = State.GameSpeedFactor * 0.025f;
        bool isAnimationDone = true;

        // Linear interpolation
        Vector3[] target = _shapes[shapeType];
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            if (_isDebris[i]) continue;

            _currentShape[i].Position = Vector3.Lerp(_currentShape[i].Position, target[i], factor);
            _currentShape[i].Color = State.CurrentColor;
        }
        // Animation done?
        for (int i = 0; i < Config.totalShapeSize; i++)
        { 
            if (_isDebris[i]) continue;

            float distanceX = Math.Abs(target[i].X - _currentShape[i].Position.X); // X is enough
            if (distanceX > tolerance)
            {
                isAnimationDone = false;
                break;
            }
        }

        return isAnimationDone;
    }

    private void Fusion()
    {   
        // Timing
        float interval = 10 / State.GameSpeedFactor;
        if (State.CurrentGameTime < _oldTime + interval) return;       
        _oldTime = State.CurrentGameTime;

        if (State.IsFirstFusion)
        {
            FusionInit();
        }

        Vector3[] target = _shapes[State.CurrentShape];
        for (int i = 0; i < 140; i++)
        {   
            int index; // Find random unmodified point, that isn't debris
            do
            {
                index = _random.Next(0, Config.totalShapeSize);
                if (_isDebris[index]) 
                {
                    _transformedPoints.Add(index);
                    continue;
                }

            } while (_transformedPoints.Contains(index));

            _transformedPoints.Add(index);

            // Transform
            _currentShape[index].Position = target[index];
            _currentShape[index].Color = State.CurrentColor;
        }
        // Set the last remaining points and cleanup
        if (_transformedPoints.Count >= Config.totalShapeSize - 200)
        {   
            FusionCleanup(ref target);
        }
    }

    private void FusionInit()
    {
        State.IsFirstFusion = false;
        _transformedPoints.Clear();
    }

    private void FusionCleanup(ref Vector3[] target)
    {
        for (int i = 0; i < Config.totalShapeSize; i++) 
        {   
            if (_isDebris[i]) continue;
            _currentShape[i].Position = target[i];
        }

        State.CurrentAnimation = AnimationType.None;
        State.IsFirstFusion = true;
        #if ANIMATION_LOGGING     
            Console.WriteLine("Fusion done!");
        #endif
    }

    private void Collapse()
    {
        // Timing
        float interval = 10 / State.GameSpeedFactor;
        if (State.CurrentGameTime < _oldTime + interval) return;
        _oldTime = State.CurrentGameTime;

        const int thresholdPosY = 70;
        Vector3[] target = _shapes[State.CurrentShape];

        // Stage 1 (Y pos--)
        if (State.IsFirstCollapse)
        {   
            CollapseStage1();
            _currentPosY -= 2;

            if (_currentPosY <= -thresholdPosY)
            {
                State.IsFirstCollapse = false;
            }
        }
        else // Stage 2 (Y pos++)
        {   
            CollapseStage2(ref target);
            _currentPosY += 2;

            if (_currentPosY == thresholdPosY)
            {
                CollapseCleanup();
            }
        }
    }

    private void CollapseStage1() 
    {   
        var target = _shapes[State.CurrentShape];

        // Hide points inside origin cube
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (_currentShape[i].Position.Y >= _currentPosY)
            {   
                _currentShape[i].Position = target[i];
                _currentShape[i].Color = ColorPalette.None;
                _isDebris[i] = false;
            }
        }
    }

    private void CollapseStage2(ref Vector3[] target)
    {   
        if (_collapseStageClear)
        {
            ShapeAsteroidSoftReset();
            _collapseStageClear = false;
        }

        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (_isDebris[i]) continue;

            if (target[i].Y <= _currentPosY)
            {
                _currentShape[i].Position.X = target[i].X;
                _currentShape[i].Position.Y = target[i].Y;
                _currentShape[i].Position.Z = target[i].Z;
                _currentShape[i].Color = State.CurrentColor;
            }
        }
    }

    private void CollapseCleanup() 
    {
        State.CurrentAnimation = AnimationType.None;
        State.IsFirstCollapse = true;
        _currentPosY = _posYDefault;
        _collapseStageClear = true;

        #if ANIMATION_LOGGING
            Console.WriteLine("Collapse done!"); 
        #endif
    }   

    private void Implode(float deltaTime) 
    {           
        float animationSpeed = State.GameSpeedFactor * 0.004f;
        _elapsedAnimationTime += deltaTime;

        switch (_elapsedAnimationTime)
        {
            case < 2300:    ImplodeStage1(animationSpeed);  break;  // Expand
            case < 3920:    ImplodeStage2(animationSpeed);  break;  // Implode
            case < 4000:    ImplodeStage3(animationSpeed);  break;  // Set at origin
            default:        ImplodeStage4();                break;  // Wait for spacebar press      
        }
    }   

    private void ImplodeStage1(float animationSpeed) 
    {   
        const int colorThreshold = 50;

        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (_isDebris[i]) continue;

            Vector3 direction = GetDirectionFromOrigin(i);
            float distance = GetDistanceFromOrigin(i);
            _currentShape[i].Position += direction * (100 - distance) * animationSpeed;

            if (distance < colorThreshold)
                _currentShape[i].Color = Config.cubeColor; 
        }
    }

    private void ImplodeStage2(float animationSpeed) 
    {
        State.IgnoreAsteroids = true;

        // Start from floor if gravity
        if (State.GravityState != GravityType.None)
        {
            ShapeGravityReset();
            for (int i = 0; i < Config.totalShapeSize; i++)
                _currentShape[i].Position.Y = NewFloorPositionY;
        }

        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            float distance = GetDistanceFromOrigin(i);
            if (distance > 2)
            {
                Vector3 direction = GetDirectionFromOrigin(i);
                _currentShape[i].Position += direction * (500 - distance) * (animationSpeed * 3);
                if (distance < 10) 
                    _currentShape[i].Color = Config.cubeColor; 
            }
        }
    }

    private void ImplodeStage3(float animationSpeed)
    {   
        if (State.IsFirstImplode) 
        {
            // Set all points @ origin, sprinkle in some color
            Color color = State.CurrentColor;
            for (int i = 0; i < Config.totalShapeSize; i++)
            {
                _currentShape[i].Color = (i % 7 == 0) ? ColorPalette.White : color;
                _currentShape[i].Position = _origin;
            }

            ShapeAsteroidSoftReset();
            State.IsFirstImplode = false;
            return;
        }

        float Factor = animationSpeed * 30;
        var target = _shapes[ShapeType.Impact];

        // Lerp to shape
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            _currentShape[i].Position = Vector3.Lerp(_currentShape[i].Position, target[i], Factor);
        }
    }

    private void ImplodeStage4()
    {   
        if (!State.IsFirstImplode)
        {   
            State.IsShotMeEnabled = true;
            State.IgnoreAsteroids = false;
            State.IsShotMeEnabled = true;
            State.IsFirstImplode = true; // Oho reusing bools...
            return;
        }
        
        if (!State.IsShotMeEnabled)
        {
            ImplodeCleanup();
        }
    }

    private void ImplodeCleanup()
    {   
        State.CurrentAnimation = AnimationType.None;
        State.IgnoreAsteroids = false;
        State.IsFirstImplode = true;
        _elapsedAnimationTime = 0;

        #if ANIMATION_LOGGING     
            Console.WriteLine("Implode done!"); 
        #endif
    }
}
