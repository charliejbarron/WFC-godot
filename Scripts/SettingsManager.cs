using Godot;

[GlobalClass]
public partial class SettingsManager : Node
{
	[Export] public Vector2 Sensitivity = new(6f, 4f);
	[Export] public Vector2 ControllerSensitivity = new(7f, 4f);
	[Export] public bool SmoothCamera = true; // pauses camera, improves movement next to jagged surface (Remove when fixing camera)
	[Export] public bool DrawDebugging;
}