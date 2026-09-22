using System;
using Godot;

[GlobalClass]
public partial class Player : CharacterBody3D
{
    [Export] private Vector2 _moveSpeeds = new(4f, 8f);
    [Export] private float _friction = 10f;
    [Export] private float _movementAccel = 0.3f;

    Vector2 _lastInput = new(0, 0);
    Vector3 _movementLock = new(0, 0, 0);

    float _moveSpeed = 5f;
    Vector3 _lastSlope;

    internal void HandleMovement(float delta, Basis orientation, PlayerInput input, bool canJump)
    {
        Move(delta, orientation, input.MovementInput, input.Sprinting);
        Jump(input.Jumping, canJump);
        MoveAndSlide();
    }

    void Jump(bool jumping, bool cantJump)
    {
        if (!jumping || cantJump)
            return;

        if (IsOnFloor())
        {
            Velocity += Vector3.Up * 8;
            return;
        }

        if (!(Velocity.Y < 0) || !IsOnWallOnly())
            return;

        Vector3 normal = GetWallNormal();

        if (Mathf.Abs(normal.Y) < 0.01f)
            return;

        Vector3 dir = new Vector3(Velocity.X, normal.Y, Velocity.Z).Normalized();

        Vector3 jumpDir = normal.Lerp(dir, 0.5f * Mathf.Clamp(Velocity.Length() * 0.2f, 0, 1)).Normalized();
        Velocity = jumpDir * Mathf.Max(Velocity.Length(), 10);
        _movementLock = new Vector3(1f - Mathf.Abs(normal.X), 0, 1f - Mathf.Abs(normal.Z));
    }

    void Move(float delta, Basis orientator, Vector2 input, bool sprinting)
    {
        float CalcSpeed()
        {
            int ind = 0;
            if (sprinting && IsOnFloor())
                ind = 1;
            _moveSpeed = Mathf.Lerp(_moveSpeed, _moveSpeeds[ind], _friction * delta);
            return _moveSpeed;
        }

        float CalcYVel()
        {
            return Velocity.Y - 9.8f * 2 * delta;
        }

        Vector3 CalcLocks(Vector3 movement)
        {
            if (_movementLock != Vector3.Zero)
                movement *= _movementLock * (1 / Velocity.Length());
            
            Vector3 normal = GetWallNormal();

            if (normal.Y < Mathf.Sin(Mathf.DegToRad(90 - FloorMaxAngle)) && normal.Y != 0)
            {
                Vector3 slope = (new Vector3(1, 0, 1) * normal).Normalized();
                movement *= 1 - Mathf.Clamp(Mathf.Abs(movement.Dot(slope)), 0, 1);
            }
            
            return movement;
        }

        Vector3 CalcVelocity(Vector3 velocity)
        {
            Vector3 normal = GetWallNormal();
            float yVelocity = CalcYVel();

            if (normal.Y < Mathf.Sin(Mathf.DegToRad(90 - FloorMaxAngle)) && normal.Y != 0)
            {
                velocity -= normal * Velocity.Y * delta;
                yVelocity *= Mathf.Lerp(normal.Y, 1, 0.85f);
            }

            return new Vector3(velocity.X, yVelocity, velocity.Z);
        }

        Vector3 movement = orientator * new Vector3(input.X, 0, input.Y) * CalcSpeed();
        Vector3 oldVelocity = Velocity * new Vector3(1, 0, 1);

        movement = CalcLocks(movement);

        bool grounded = IsOnFloor();
        Vector3 newVelocity = movement * (grounded ? _movementAccel : 0.2f) + oldVelocity;
        float friction = grounded ? 1 - delta * 0.5f * _friction : 1f;
        
        if (grounded)
            _movementLock = Vector3.Zero;

        float maxSpeed = Mathf.Min(Mathf.Max(_moveSpeed, oldVelocity.Length() * friction), newVelocity.Length() * friction);
        Vector3 newVelocityClamped = CalcVelocity(newVelocity.Normalized() * maxSpeed);
        SetVelocity(newVelocityClamped);
    }
}