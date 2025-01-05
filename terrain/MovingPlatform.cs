using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Schema;
using Godot;
using System;

// Simply implemented because this was for prototyping
[Tool]
public partial class MovingPlatform : AnimatableBody2D
{
	
	[Export(PropertyHint.Range, "0,200,or_greater")] float speed = 100;

	public Vector2 Dimensions;
	public Vector2 Velocity = Vector2.Zero;
	

	public override void _Ready() {
		
		Dimensions = ((RectangleShape2D)GetNode<CollisionShape2D>("Collider").Shape).Size;

		if(!Engine.IsEditorHint()) {
			updateGoal();
		}
	}

	public override void _PhysicsProcess(double delta) {
		if(!Engine.IsEditorHint()) {
			Vector2 difference = goal.GlobalPosition - GlobalPosition;
			Velocity = difference.Normalized() * speed;
			if (difference.Length() < speed*delta)
			{
				GlobalPosition = goal.GlobalPosition;
				nextGoal(); // Automatically locked out of doing it repeatedly
			}

			Position += Velocity * (float)delta;
		}

	}

	int point_idx = 0;
	PlatformMarker goal;
	bool incrementingGoal = false;
	async void nextGoal() {
		if (!incrementingGoal) {
			incrementingGoal = true;
			await Task.Delay((int)goal.PauseLength*1000);
			incrementingGoal = false;
			point_idx = (point_idx + 1) % GetNode("Positions").GetChildCount();
			updateGoal();
		}
	}
	
	void updateGoal() {
		goal = GetNode("Positions").GetChild<PlatformMarker>(point_idx);
		
	}
}
