using Godot;

[GlobalClass]
public partial class InputManager : Node
{
	NavigationManager _navigationManager;
	Vector2 _lastInput;
	
	public override void _Ready()
	{
		_navigationManager = GetNode<NavigationManager>("../NavigationManager");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			_lastInput += eventMouseMotion.Relative;
		}

		if (Input.IsActionJustPressed("Click"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	PlayerInput GetPlayerInput()
	{
		Vector2 FetchMovementInput()
		{
			Vector2 newInput = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Forward", "Back"));
			if (newInput.Length() > 1f) newInput = newInput.Normalized();
			return newInput;
		}
		
		PlayerInput input = new PlayerInput
		{
			MouseInput = _lastInput,
			ControllerInput = Input.GetVector("Camera-Left", "Camera-Right", "Camera-Up", "Camera-Down"),
			MovementInput = FetchMovementInput(),
			Jumping = Input.IsActionJustPressed("Jump"),
			Sprinting = Input.IsActionPressed("Sprint")
		};

		_lastInput = Vector2.Zero;
		return input;
	}

	public override void _PhysicsProcess(double delta)
	{
		PlayerInput input = GetPlayerInput();
		_navigationManager.HandleInputs((float)delta, input);
		_lastInput = Vector2.Zero;
	}
}

public class PlayerInput
{
	internal Vector2 MouseInput;
	internal Vector2 ControllerInput;
	internal Vector2 MovementInput;
	internal bool Jumping;
	internal bool Sprinting;
}
