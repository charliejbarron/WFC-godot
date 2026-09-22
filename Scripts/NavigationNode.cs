using System;
using Godot;

[GlobalClass]
public partial class NavigationNode : Node3D
{
    [Export] public Perspective CameraPerspective;
    [Export] internal NavigationNode[] ConnectedNavigationNodes;

    [ExportSubgroup("InputLocks")] [Export]
    internal InputLock InputLockX;

    [Export] internal InputLock InputLockZ;
    [Export] internal bool InputLockJump;

    internal enum InputLock
    {
        None,
        Both,
        Far,
        Near
    }

    public virtual int Priority => 0;

    internal void Handle(float delta, PlayerInput input, Content content)
    {
        void CameraControls()
        {
            if (CameraPerspective == null)
                return;

            CameraPerspective.HandleCamera(content, input.ControllerInput + input.MouseInput, delta, input.MovementInput);
            content.Camera.HandleOffset(CameraPerspective.CameraOffset, CameraPerspective.OffsetTransitionSpeed,
                CameraPerspective.IgnoreCollisions, content.Settings.SmoothCamera, delta);
        }

        Vector2 LockedInput(Content content, Vector2 movementInput)
        {
            bool LockInputAxis(InputLock lockAxis, float offset, float axisInput)
            {
                switch (lockAxis)
                {
                    case InputLock.None:
                        return false;
                    case InputLock.Both:
                        return true;
                    case InputLock.Far:
                        switch (offset)
                        {
                            case > -0.25f when axisInput <= 0:
                                return true;
                            case > -2f when axisInput < 0:
                                input.Sprinting = false;
                                return true;
                            default:
                                return false;
                        }
                    case InputLock.Near:
                        switch (offset)
                        {
                            case < 0.25f when axisInput >= 0:
                                return true;
                            case < 2f when axisInput > 0:
                                input.Sprinting = false;
                                return true;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }

            Vector2 toCenter = ToLocal(content);

            float offsetX = Mathf.Abs(toCenter.X) > 0.25f ? Mathf.Clamp(toCenter.X, -2, 2) * 0.2f : 0;
            float offsetY = Mathf.Abs(toCenter.Y) > 0.25f ? Mathf.Clamp(toCenter.Y, -2, 2) * 0.2f : 0;

            bool lockX = LockInputAxis(InputLockX, toCenter.X, movementInput.X);
            bool lockY = LockInputAxis(InputLockZ, toCenter.Y, movementInput.Y);

            movementInput = new Vector2(lockX ? offsetX : movementInput.X, lockY ? offsetY : movementInput.Y);

            return movementInput;
        }

        HandleOrient(content);
        input.MovementInput = LockedInput(content, input.MovementInput);
        content.Player.HandleMovement(delta, content.Orientator.Basis, input, InputLockJump);
        CameraControls();
    }

    internal virtual Vector2 ToLocal(Content content)
    {
        Vector3 local = content.Orientator.ToLocal(GlobalPosition - content.Player.GlobalPosition);
        Vector2 toCenter = new Vector2(local.X, local.Z);

        return toCenter;
    }


    internal virtual void HandleOrient(Content content)
    {
    }

    internal virtual void OnExit(Content content, PlayerInput input)
    {
    }

    internal virtual void OnEnter(Content content, PlayerInput input)
    {
    }

    internal virtual NavigationNode CalculateCurrentNode(Content content)
    {
        bool inside = PlayerInside(content.Player);
        for (int i = 0; i < ConnectedNavigationNodes.Length; i++)
        {
            if (!ConnectedNavigationNodes[i].PlayerInside(content.Player))
                continue;

            if (inside && ConnectedNavigationNodes[i].Priority <= Priority) continue;

            return ConnectedNavigationNodes[i];
        }

        return null;
    }

    internal virtual bool PlayerInside(Player player)
    {
        return false;
    }
}