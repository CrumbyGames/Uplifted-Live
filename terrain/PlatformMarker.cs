using Godot;
using System;
[Tool]
public partial class PlatformMarker : Node2D
{
	[Export(PropertyHint.Range,"0,10,or_greater")] public float PauseLength = 0f;
	[Export] bool visibleGizmo;    

	MovingPlatform platform;
	Vector2 difference;
	public override void _Ready()
	{
		platform = GetParent().GetParent<MovingPlatform>();
		Node2D hitbox = platform.GetNode<Node2D>("Collider");
		difference = hitbox.GlobalPosition - platform.GlobalPosition;
	}
	
	public override void _Draw()
	{
		// Shows outline of platform in editor to show where the platform stops.
		if (Engine.IsEditorHint())
		{
			DrawRect(new Rect2(difference.X - platform.Dimensions.X / 2, difference.Y - platform.Dimensions.Y / 2, platform.Dimensions.X, platform.Dimensions.Y), new Color(0.4f, 0.5f, 0.8f, 0.7f), false, 1);
		}
	}
}
