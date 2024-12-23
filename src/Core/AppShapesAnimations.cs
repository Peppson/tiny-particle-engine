
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GlobalTypeDefinitions;
using GlobalColorPalette;
using GlobalConfig;
using GlobalStates;
using UtilityClass;

namespace monogame;

public partial class App : Game
{
    private Vector3[] _currentShapeCopy = new Vector3[Config.totalShapeSize]; // Animation snapshot
    private HashSet<int> _transformedPoints = []; // AnimationFusion() counter
    private Random _random = new(); // Noise
    private double _oldTime = 0;


    private void ShapeTransitionAnimation()
    {   
        if (State.CurrentAnimation == AnimationType.None) return;

        HandleAnimationReset();

        switch (State.CurrentAnimation)
        {
            case AnimationType.Gravity:
                AnimationGravity();
                break;
            case AnimationType.AntiGravity:
                AnimationAntiGravity();
                break;
            case AnimationType.Implode:
                AnimationImplode();
                State.PreviousAnimation = State.CurrentAnimation;
                break;
            case AnimationType.Lerp:
                AnimationLerp();
                State.PreviousAnimation = State.CurrentAnimation;
                break;
            case AnimationType.Fusion:
                AnimationFusion();
                State.PreviousAnimation = State.CurrentAnimation;
                break;
            case AnimationType.Collapse:
                AnimationCollapse();
                State.PreviousAnimation = State.CurrentAnimation;
                break;
            default:
                break;
        }       
    }

    private void AnimationGravity()
    {   
        const float floorPos = -50f;
        const double animationDuration = 800D;
        float animationSpeed = State.GameSpeedFactor * 0.00025f;

        // Copy _currentshape if transitioning to gravity from another animation
        // use _currentShapeCopy later to resume previous animation
        if (State.IsFirstGravity && State.PreviousAnimation != AnimationType.None)
        {   
            TakeAnimationSnapshot();
        }

        // Drop all points in _currentShape to the floor
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            float distance = _currentShape[i].Position.Y - floorPos;
            float velocity = 101 - (distance - _random.Next(0, 100));
            _currentShape[i].Position.Y -= distance * velocity * animationSpeed;
        }

        // Animation done?
        if (State.GameTotalTime - State.AnimationStartTime > animationDuration)
        {
            State.CurrentAnimation = AnimationType.None;
            State.AnimationStartTime = 0;
        }
    }

    private void AnimationAntiGravity()
    {   
        const double animationDuration = 450D;
        Vector3[] shape = new Vector3[Config.totalShapeSize];

        // Use _currentShapeCopy to resume previous animation, if any ongoing before gravity
        shape = (State.PreviousAnimation == AnimationType.None) ? 
            _shapes[State.CurrentShape] :
            _currentShapeCopy;

        // Animation done?
        if (Lerp(shape, animationDuration))
        {   
            // Continue with previous animation if there was one
            State.CurrentAnimation = State.PreviousAnimation;
            State.PreviousAnimation = AnimationType.None;
            State.AnimationStartTime = State.GameTotalTime;
            State.IsFirstGravity = true;
        }
    }

    private void AnimationLerp()
    {   
        const double animationDuration = 1200D;

        // Animation done?
        if (Lerp(State.CurrentShape, animationDuration))
        {
            State.CurrentAnimation = AnimationType.None;
        }
    }

    public void AnimationFusion()
    {   
        // Timing
        float animationInterval = 10 / State.GameSpeedFactor;
        if (State.GameTotalTime < _oldTime + animationInterval) return;
        _oldTime = State.GameTotalTime;

        Vector3[] shape = _shapes[State.CurrentShape]; // Transform into
        const int threshold = Config.totalShapeSize - 200;
        State.IsFirstFusion = true;

        // Animation
        for (int i = 0; i < 140; i++)
        { 
            int index; // Find unmodified point
            do
            {
                index = _random.Next(0, Config.totalShapeSize);
            } while (_transformedPoints.Contains(index));

            // Save point
            _transformedPoints.Add(index);

            // Transform
            _currentShape[index].Position.X = shape[index].X;
            _currentShape[index].Position.Y = shape[index].Y;
            _currentShape[index].Position.Z = shape[index].Z;
        }

        // Set the remaining points and cleanup
        if (_transformedPoints.Count >= threshold)
        {   
            // Dirty, just iterate through the whole thing ;)
            for (int i = 0; i < Config.totalShapeSize; i++) 
            {   
                _currentShape[i].Position.X = shape[i].X;
                _currentShape[i].Position.Y = shape[i].Y;
                _currentShape[i].Position.Z = shape[i].Z;
            }
            State.CurrentAnimation = AnimationType.None;
            State.AnimationStartTime = 0;
            State.IsFirstFusion = false;
            _transformedPoints.Clear();
        }
    }

    public void AnimationCollapse()
    {
        // Timing
        float animationInterval = 10 / State.GameSpeedFactor;
        if (State.GameTotalTime < _oldTime + animationInterval) return;
        _oldTime = State.GameTotalTime;

        const int thresholdPosY = 70;
        Vector3[] shape = _shapes[State.CurrentShape]; // Transform into
        
        // Stage 1 collapse (Y pos--)
        if (State.IsFirstCollapse)
        {   
            CollapseShape();
            State.CollapsePosY -= 2;

            if (State.CollapsePosY == -thresholdPosY)
            {
                State.IsFirstCollapse = false;
            }
        }

        // Stage 2 rebuild (Y pos++)
        else
        {   
            RebuildShape(ref shape);
            State.CollapsePosY += 2;

            if (State.CollapsePosY == thresholdPosY)
            {
                State.CurrentAnimation = AnimationType.None;
                State.AnimationStartTime = 0;
                State.IsFirstCollapse = true;
            }
        }
    }

    public void AnimationImplode()
    {
        const int colorThreshold = 50;
        float animationSpeed = State.GameSpeedFactor * 0.003f;
        double animationTimer = State.GameTotalTime - State.AnimationStartTime;
        Vector3[] shape = _shapes[State.CurrentShape]; // Transform into

        // Animation
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            Vector3 direction = GetDirectionFromOrigin(i);
            float distance = GetDistanceFromOrigin(i);
                             
            // Stage 1 (expand)
            if (animationTimer < 2800)
            {   
                _currentShape[i].Position += direction * (100 - distance) * animationSpeed;
                if (distance < colorThreshold) 
                    _currentShape[i].Color = NordColors.Orange;
            }

            // Stage 2 (implode)
            else if (animationTimer < 3900)
            {   
                State.IsFirstImplode = true;
                if (distance > 6)
                    _currentShape[i].Position += direction * (150 - distance) * (animationSpeed * 7);
            }

            // Stage 3 (remodel)
            else if (State.IsFirstImplode)
            {   
                State.IsFirstImplode = false;
                _cam.RotateViewForTorus();

                // Set all points at origin, sprinkle in some color
                for (int j = 0; j < Config.totalShapeSize; j++)
                {
                    _currentShape[j].Position = _origin;
                    _currentShape[j].Color = (j % 7 == 0) ? NordColors.White : State.CurrentColor;
                }
            }

            // Stage 4 (explode)
            else if (animationTimer < 4200)
            {   
                float lerpFactor = animationSpeed * 30;
                _currentShape[i].Position = Vector3.Lerp(_currentShape[i].Position, shape[i], lerpFactor);
            }  

            // Cleanup
            else
            {   
                State.CurrentAnimation = AnimationType.None;
                State.AnimationStartTime = 0;
                break;
            }
        }
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

    private void TakeAnimationSnapshot()
    {
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            _currentShapeCopy[i] = _currentShape[i].Position;
        }   
        State.IsFirstGravity = false;
    }

    private bool Lerp(ShapeType shapeType, double animationDuration)
    {   
        Vector3[] shape = _shapes[shapeType];

        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            float lerpFactor = State.GameSpeedFactor * 0.025f;
            _currentShape[i].Position = Vector3.Lerp(_currentShape[i].Position, shape[i], lerpFactor);
        }

        // Animation done?
        if (State.GameTotalTime - State.AnimationStartTime > animationDuration)
        {
            State.AnimationStartTime = 0;
            return true;
        }

        return false;
    }

    private bool Lerp(Vector3[] shape, double animationDuration)
    {   
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            float lerpFactor = State.GameSpeedFactor * 0.03f;
            _currentShape[i].Position = Vector3.Lerp(_currentShape[i].Position, shape[i], lerpFactor);
        }

        // Animation done?
        if (State.GameTotalTime - State.AnimationStartTime > animationDuration)
        {
            State.AnimationStartTime = 0;
            return true;
        }

        return false;
    }

    private void CollapseShape() 
    {
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (_currentShape[i].Position.Y >= State.CollapsePosY)
            {
                _currentShape[i].Position.X = 0;
                _currentShape[i].Position.Y = 0;
                _currentShape[i].Position.Z = 0;
            }
        }
    }

    private void RebuildShape(ref Vector3[] shape)
    {
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (shape[i].Y <= State.CollapsePosY)
            {
                _currentShape[i].Position.X = shape[i].X;
                _currentShape[i].Position.Y = shape[i].Y;
                _currentShape[i].Position.Z = shape[i].Z;
            }
        }
    }

    private void HandleAnimationReset() 
    {   
        // Spaghetti reset control
        AnimationCollapseReset();
        AnimationFusionReset();
    }

    private void AnimationCollapseReset()
    {   
        if (State.PreviousAnimation == AnimationType.Collapse &&
            State.CurrentAnimation != AnimationType.Collapse)
        {
            State.IsFirstCollapse = true;
            State.CollapsePosY = 50;
        }
    }

    private void AnimationFusionReset()
    {
        if (State.IsFirstFusion && State.PreviousAnimation != AnimationType.Fusion)
        {
            State.IsFirstFusion = false;
            _transformedPoints.Clear();
        }
    }
}
