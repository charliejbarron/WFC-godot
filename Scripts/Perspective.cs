using Godot;

[GlobalClass, Tool]
public partial class Perspective : Resource
{
	[Export] public Vector3 CameraOffset = new (0, -0.1f, 2.3f);
	[Export] public float OffsetTransitionSpeed = 1f;
    [Export] public float CameraWeight = 15f;
	
	internal virtual bool IgnoreCollisions => false;
	
	internal virtual void HandleCamera(Content content, Vector2 input, float delta, Vector2 movementInput)
	{
		content.Marker.GlobalPosition = content.Marker.GlobalPosition.Lerp(content.Player.GlobalPosition + Vector3.Up * 1.62f, Mathf.Clamp(delta * CameraWeight, 0, 1));
		
		content.CameraOrigin.Transform = content.PitchMarker.GetGlobalTransformInterpolated();
		
		HandleHead(content.Player, content.Head, delta);
	}
	
	internal virtual void HandleHead(Player player, Node3D head, float delta)
	{
		Vector2 forwardVector = new Vector2(-player.Velocity.Z, player.Velocity.X);
		
        if (forwardVector.Length() <= 1e-07)
            return;
		
        Quaternion forward = new Quaternion(Vector3.Up, -forwardVector.Angle()).Normalized();

        head.Rotation = Quaternion.FromEuler(head.Rotation).Slerp(forward, Mathf.Clamp(delta * 15f, 0, 1)).GetEuler();
	}

	internal virtual void OnSwitch()
	{
	}
}
