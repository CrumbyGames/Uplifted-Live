using Godot;
using System.Collections.Generic;

public partial class Camera : Camera2D
{
	/// <summary>
	/// Field of <see cref="Target"/>.
	/// </summary>
	Node2D target = null;

	/// <summary>
	/// A reference to the current <see cref="RemoteTransform2D"/> being used to mirror the position of <see cref="Target"/>.
	/// </summary>
	RemoteTransform2D currentRemote = null;
	
	/// <summary>
	/// The node that the camera is currently following. Upon set, a<see cref="RemoteTransform2D"/>is instanced to mirror the position of the target.
	/// </summary>
	public Node2D Target {
		get { return target; }
		set {
			if(value != target) {
				// Remove previous remote if able
				if (currentRemote != null)
				{
					currentRemote.QueueFree();
				}

				// Update field
				target = value;

				// Mirror initial position
				GlobalPosition = target.GlobalPosition;

				// Instance remote in new target
				var Global = GetNode<Global>("/root/Global");
				currentRemote = Global.AddInstanceAsChild<RemoteTransform2D>(target, new Dictionary<string, Variant>()
					{
						["UpdateRotation"] = false,
						["UpdateScale"] = false,
					}
				);
				currentRemote.RemotePath = currentRemote.GetPathTo(this);
			}
		}
	}
}
