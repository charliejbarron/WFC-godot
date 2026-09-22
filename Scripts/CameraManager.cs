using Godot;

public partial class CameraManager : Camera3D
{
	internal Vector3 OffsetLerped;
	ShapeCast3D _softShape;
	Node3D _marker;

	float _fraction = 1;
	float _fractionTimer;

	public override void _Ready()
	{
		_marker = GetNode<Node3D>("../CameraMarker");
		_softShape = GetNode<ShapeCast3D>("../SoftShape");
	}

	internal void HandleOffset(Vector3 offset, float transitionSpeed, bool ignoreCollisions, bool smooth ,float delta)
	{
		OffsetLerped = OffsetLerped.Lerp(offset, delta * transitionSpeed);
		
		if(ignoreCollisions)
		{
			_marker.Position = OffsetLerped;
		}
		else
		{
			_softShape.TargetPosition = OffsetLerped;
			_softShape.ForceShapecastUpdate();

			if (_softShape.IsColliding())
			{
				float newFraction = _softShape.GetClosestCollisionSafeFraction();

				if (newFraction < _fraction && smooth)
				{
					_fraction = newFraction;
					_fractionTimer = 0.3f;
				}

				_marker.Position = OffsetLerped * (smooth ? _fraction : newFraction);
			}
			else
			{
				_marker.Position = OffsetLerped * (smooth ? _fraction : 1f);

				if (smooth)
				{
					if (_fractionTimer <= 0)
						_fraction = float.Lerp(_fraction, 1, delta * -_fractionTimer);
					_fractionTimer -= delta;
				}
			}
		}		
		
		Transform = _marker.GetGlobalTransformInterpolated();
	}
}
