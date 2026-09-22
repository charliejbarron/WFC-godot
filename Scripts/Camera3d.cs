using Godot;
using System;

public partial class Camera3d : Node3D
{
    [Export] float _maxSpeed = 0.2f;
    Vector3 _velocity;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        Vector3 input = new Vector3(Input.GetAxis("left", "right"), Input.GetAxis("down", "up"), Input.GetAxis("forward", "back"));
        input = input.Length() > 1f ? input.Normalized() : input;
        Vector3 newForce = _velocity + input * (float)delta;

        float speedCap = Mathf.Min(Mathf.Max(_velocity.Length(), _maxSpeed), newForce.Length());
        
        _velocity = newForce.Normalized() * speedCap * (1 - (float)delta * 4f * (1 - input.Length()));
        GlobalPosition += GlobalBasis * _velocity;
    }
}