using Godot;

[GlobalClass, Tool]
public partial class TP_Perspective : Perspective
{
    [Export] public float CameraAutoRotation = 1f;
    
    internal override void HandleCamera(Content content, Vector2 input, float delta, Vector2 movementInput)
    {
        float xCameraInput = Mathf.Clamp(content.CameraOrigin.RotationDegrees.X - input.Y * 0.1f, -85f, 90);
        float yCameraInput = content.CameraOrigin.RotationDegrees.Y - input.X * 0.1f;
        float autoRotation = movementInput.X * 3.6f * CameraAutoRotation * delta;
		
        content.Marker.RotationDegrees = new Vector3(0, yCameraInput - autoRotation, 0);
        content.PitchMarker.RotationDegrees = new Vector3(xCameraInput, 0, 0);
        
        base.HandleCamera(content, input, delta, movementInput);
    }
}
