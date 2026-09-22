using Godot;

[GlobalClass, Tool]
public partial class FreeNavigationNode : NavigationNode
{
    Area3D _regionArea;
    bool entered;

    public override void _Ready()
    {
        _regionArea = GetChild<Area3D>(0);
        _regionArea.BodyEntered += DetectPlayerEnter;
    }

    internal override void HandleOrient(Content content)
    {
        content.Orientator.Rotation = new Vector3(0, content.CameraOrigin.Rotation.Y, 0);
    }

    internal override bool PlayerInside(Player player)
    {
        return _regionArea.OverlapsBody(player);
    }

    void DetectPlayerEnter(Node3D body)
    {
        if (body is Player)
        {
            entered = true;
        }
    }

    void DetectPlayerExit(Node3D body)
    {
        if (body is Player)
        {
            entered = false;
        }
    }
    
    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        base._GetConfigurationWarnings();
        string[] warnings = new string[1];

        int children = GetChildCount();

        switch (children)
        {
            case 0:
                Area3D area = new Area3D();
                area.Name = "RegionArea";
                area.CollisionMask = 2;
                AddChild(area);
                area.SetOwner(EditorInterface.Singleton.GetEditedSceneRoot());
                return null;
            case 1:
                if (GetChildOrNull<Area3D>(0) == null)
                {
                    warnings[0] = "Child node must be of type: Area3D";
                    return warnings;
                }

                return null;
            default:
                warnings[0] = "Navigation Node should contain only 1 Child of type Area3D. Nodes: " + GetChildren();
                return warnings;
        }
    }
    #endif
}