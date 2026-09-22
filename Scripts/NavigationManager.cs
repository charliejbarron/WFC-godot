using Godot;

[GlobalClass]
public partial class NavigationManager : Node
{
	[Export] public NavigationNode StartingNode;
	[Export] public PackedScene PlayerScene;

	readonly Content _content = new();
	
	NavigationNode _currentNode;
	internal bool Editing;

	public override void _EnterTree()
	{
		void GetNodes(Node player)
		{
			string scene = GetTree().CurrentScene.Name;
			string path = "/root/" + scene;
			
			_content.Settings = GetNode<SettingsManager>(path + "/SettingsManager");
			
			_content.Orientator = player.GetNode<Node3D>("Orientation");
			
			_content.Head = player.GetNode<Node3D>("Player/Head");
			_content.CameraOrigin = player.GetNode<Node3D>("CameraOrigin");
			_content.Marker = player.GetNode<Node3D>("CameraOriginMarker");
			_content.PitchMarker = player.GetNode<Node3D>("CameraOriginMarker/PitchMarker");
			
			_content.Camera = player.GetNode<CameraManager>("CameraOrigin/Camera");
			_content.Player = player.GetNode<Player>("Player");
		}
		
		_currentNode = StartingNode;
		
		Node playerHolder = PlayerScene.Instantiate();
		
		AddChild(playerHolder);
		GetNodes(playerHolder);
		
		_content.Camera.OffsetLerped =  _content.Marker.Position = _currentNode.CameraPerspective.CameraOffset;
	}

	public override void _Ready()
	{
		Player player = GetNode<Player>("playerHolder/Player");
		player.GlobalPosition = _currentNode.GlobalPosition;
		
		_currentNode.HandleOrient(_content);
		_currentNode.CameraPerspective.HandleCamera(_content, Vector2.Zero, 1f, Vector2.Zero);
	}

	public void HandleInputs(float delta, PlayerInput input)
	{
		input.MouseInput *= _content.Settings.Sensitivity * 0.1f;
		input.ControllerInput *= _content.Settings.ControllerSensitivity * 3f;
		
		_currentNode.Handle(delta, input, _content);

		NavigationNode node = _currentNode.CalculateCurrentNode(_content);

		if(node != null)
		{
			_currentNode.OnExit(_content, input);
			_currentNode.CameraPerspective.OnSwitch();
			_currentNode = node;
			_currentNode.OnEnter(_content, input);
		}
	}
}

public class Content
{
	public Node3D Orientator;
	public Node3D CameraOrigin;
	public Node3D Marker;
	public Node3D PitchMarker;
	public Node3D Head;
	public SettingsManager Settings;
	public CameraManager Camera;
	public Player Player;
}
