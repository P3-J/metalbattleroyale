using Godot;

public partial class Player : CharacterBody3D
{
	[Export] public float Speed = 6f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float MouseSensitivity = 0.002f;
	[Export] private Node3D _head;
	[Export] private Camera3D _camera;

	public override void _Ready()
	{

		Input.MouseMode = Input.MouseModeEnum.Captured;

	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (e is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			_head.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

			Vector3 rot = _head.Rotation;
			rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
			_head.Rotation = rot;
		}

		if (e is InputEventKey)
		{

			if (e.IsActionPressed("escape"))
			{
				Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * (float)delta;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
