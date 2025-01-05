using System.Xml.Schema;
using Godot;
using System;
namespace Player {

    
    /// <summary>
    /// Character states for animation and behaviours (only for reference currently as there was no real need for a state machine while prototyping before complex animation)
    /// </summary>
    public enum State
	{
        /// <summary>
        ///     Conditions:
        ///     <list type="number">
        ///         <item>Grounded</item>
        ///         <item>Negligible velocity</item>
        ///         <item>Horizontal input not pressed</item>
        ///     </list>
        /// </summary>
		Idle = 1,

        /// <summary>
        ///     Conditions:
        ///     <list type="number">
        ///         <item>Grounded</item>
        ///         <item>Horizontal input pressed</item>
        ///     </list>
        /// </summary>
		Move = 2,

        /// <summary>
        ///     Conditions:
        ///     <list type="number">
        ///         <item>Airborne</item>
        ///         <item>Upwards velocity</item>
        ///     </list>
        /// </summary>
		Jump = 16,

        /// <summary>
        ///     Conditions:
        ///     <list type="number">
        ///         <item>Airborne</item>
        ///         <item>Downwards velocity</item>
        ///     </list>
        /// </summary>
		Fall = 32,

        /// <summary>
        ///     Conditions:
        ///     <list type="number">
        ///         <item>Airborne</item>
        ///         <item>Downwards velocity</item>
        ///         <item>Glide input pressed</item>
        ///     </list>
        /// </summary>
        Gliding = 64,
        
        Grounded = Idle | Move,
        Airborne = Jump | Fall | Gliding,
	}

    public partial class @Controller : CharacterBody2D
    {
        // Initialise export variables that relate to grounded physics

        [ExportGroup("Grounded Physics")]
        [Export(PropertyHint.Range, "0,1")] float accelerationCoefficient = 0.15f;
        [Export(PropertyHint.Range, "0,1")] float decelerationCoefficient = 0.3f;
        [Export(PropertyHint.Range, "0,500,or_greater")] float moveSpeed = 300.0f;
        [Export(PropertyHint.Range, "0,0.5,or_greater")] float coyoteTime = 0.0f;

        // Initialise export variables that relate to airborne physics

        [ExportGroup("Airborne Physics")]
        [Export(PropertyHint.Range, "0,1")] float airborneAccelerationCoefficient = 0.25f;
        [Export(PropertyHint.Range, "0,500,or_greater")] float jumpSpeed = 400.0f;
        [Export(PropertyHint.Range, "0,150,or_greater")] float glideSpeed = 25.0f;
        [Export(PropertyHint.Range, "0,1")] float slowDownCoefficient = 0.15f;

        // Initialise node variables

        /// <summary>
        /// Contains references to all<see cref="AnimationPlayer"/>and<see cref="AnimationTree"/>instances used by this node.<para>Expand manually.</para>
        /// </summary>
        (AnimationTree Walk, AnimationPlayer Direction) animators;

        /// <summary>
        /// A reference to a<see cref="Camera2D"/>that is globally reused.
        /// </summary>
        Camera camera;

        // Initialise miscellaneous variables

        /// <summary>
        /// The gravitational constant, derived from project settings.
        /// </summary>
        float gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
    
        /// <summary>
        /// Stores the last direction the player moved in
        /// </summary>
        bool facingRight = true;

        public override void _Ready()
        {
            // Get node references
            camera = Owner.GetNode<Camera>("%Camera");
            animators.Walk = GetNode<AnimationTree>("%Animators/Walk");
            animators.Direction = GetNode<AnimationPlayer>("%Animators/Direction");
            
            // Update camera to follow the player
            camera.Target = this;

        }

        public override void _PhysicsProcess(double delta)
        {
            // Initialise a desynced local copy of velocity
            Vector2 velocity = Velocity;

            // Dynamic friction
            float surfaceFriction = 1.0f; 

            // AIRBORNE BEHAVIOUR
            if (!IsOnFloor())
            {
                surfaceFriction = airborneAccelerationCoefficient;
                
                // Gliding
                
                if (velocity.Y > glideSpeed && Input.IsActionPressed("ui_accept"))
                {
                    velocity.Y = Mathf.Lerp(velocity.Y, glideSpeed, slowDownCoefficient);
                }

                else

                // Falling
                
                {
                    velocity.Y += gravity * (float)delta;
                }
            }

            // GROUNDED BEHAVIOUR
            if (IsOnFloor())
            {

                // Jumping

                if(Input.IsActionJustPressed("ui_accept")) {
                    velocity.Y = -jumpSpeed;
                }

                // Varying floor frictions
                
                // Get collision that matches floor
                KinematicCollision2D floorCollision = null;
                for(int i=0; i<GetSlideCollisionCount(); i++) {
                    KinematicCollision2D collision = GetSlideCollision(i);
                    if(collision.GetNormal() == GetFloorNormal()) {
                        floorCollision = collision;
                        break;
                    }
                }

                // Get friction from collider if static (only StaticBody2D can have a PhysicsMaterialOverride)
                if(floorCollision is not null) {
                    PhysicsBody2D floorCollider = (PhysicsBody2D)floorCollision.GetCollider();
                    if(floorCollider is StaticBody2D) {
                        StaticBody2D staticFloorCollider = (StaticBody2D)floorCollider;
                        if(staticFloorCollider.PhysicsMaterialOverride != null) {
                            surfaceFriction = staticFloorCollider.PhysicsMaterialOverride.Friction;
                        }
                    }
                }
            }

            // HORIZONTAL MOVEMENT
            // Get speed proportional to input
            float targetSpeed = Input.GetAxis("move_left", "move_right") * moveSpeed;


            float velocityInterpolationWeight = surfaceFriction;
            if (targetSpeed != 0 && Mathf.Sign(targetSpeed) == Mathf.Sign(velocity.X) && Mathf.Abs(velocity.X) < Mathf.Abs(targetSpeed)) {
                velocityInterpolationWeight *= accelerationCoefficient;
            } else {
                velocityInterpolationWeight *= decelerationCoefficient;
            }

            // Update local velocity using linear interpolation. Consider the weight based on varying floor friction and whether accelerating or decelerating.
            velocity.X = Mathf.Lerp(velocity.X, targetSpeed, velocityInterpolationWeight);

            // ANIMATION (only walking animation has been implemented as a proof of concept, there are no other animations ready)

            // Determine and set walk blend factor (based on momentum, either leaning/walking backwards or running forwards)
            float blendFactor = 0f;
            if (IsOnFloor()) {
                blendFactor = Mathf.Clamp(Mathf.Abs(velocity.X) / moveSpeed, 0, 1);
                if (Mathf.Abs(Velocity.X) > 0f && Mathf.Sign(targetSpeed) != Mathf.Sign(Velocity.X)) {
                    blendFactor *= -1;
                }
            }
            
            animators.Walk.Set("parameters/Momentum/blend_amount", Mathf.Lerp((float)animators.Walk.Get("parameters/Momentum/blend_amount"),blendFactor,0.15f));

            // Update sprite direction smoothly
            if(facingRight!=Velocity.X>0) {
                animators.Direction.Play("direction_changer/"+(facingRight ? "TurnLeft" : "TurnRight"));
                facingRight = !facingRight;
            }

            // ENGINE PHYSICS
            Velocity = velocity;
            MoveAndSlide();
        }
    }
}