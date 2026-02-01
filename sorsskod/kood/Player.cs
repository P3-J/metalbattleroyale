using Godot;

public partial class Player : CharacterBody3D
{
	[Export] public float Speed = 6f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float MouseSensitivity = 0.002f;
	[Export] private Node3D _head;
	[Export] private Camera3D _camera;
	[Export] RayCast3D handRay;
	[Export] Marker3D handMarker;
	[Export] CashRegister reg;
	[Export] Camera3D cashCam;
	[Export] AudioStreamPlayer3D craftingAudio;
	Label toiletLabel;
	Label infoTag;

	bool holdingObj = false;
	bool canHoldItem = true;
	bool tryingToHoldItem = false;
	RigidBody3D objInHand = null;
	/// <summary>
	///  SWITCH START END POINT
	/// </summary>

	public override void _Ready()
	{
		base._Ready();
		toiletLabel = GetNode<Label>("head/Camera3D/Control/Toilet");
		infoTag = GetNode<Label>("head/Camera3D/Control/lmb/InfoTag");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		glob = GetNode<Globals>("/root/Globals");
		glob.Connect("PassiveBenchResponse", new Callable(this, nameof(HandlePassiveBenchResponse)));
		glob.Connect("LabelTextResponse", new Callable(this, nameof(UpdateInfoLabelText)));
		BalanceLabel.Text = "Balance: " + moneyBalance.ToString() + " $";
	}

	public void UpdateInfoLabelText(string text)
	{
		infoTag.Text = text;
		GD.Print("Updated Info Label Text: ", text);
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (inBenchMode)
		{
			HandleBenchInput(e);
			return;
		}

		if (inConsoleMode)
		{
			HandleConsoleInput(e);
			return;
		}

		if (e is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			_head.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

			Vector3 rot = _head.Rotation;
			rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
			_head.Rotation = rot;
		}

		if (e is InputEventKey eventKey)
		{
			if (e.IsActionPressed("escape"))
			{
				Input.MouseMode =
					Input.MouseMode == Input.MouseModeEnum.Captured
						? Input.MouseModeEnum.Visible
						: Input.MouseModeEnum.Captured;
			}

			if (eventKey.Keycode == Key.E && eventKey.Pressed)
			{
				if (canUsePassiveBench)
				{
					HandlePassiveBenchInput(e);
					return;
				}
				if (canClearToilet)
				{
					HandleClearToiletInput(e);
					return;
				}
				if (canUseToilet && !usedToilet)
				{
					HandleToiletInput(e);
					return;
				}
				if (canUseBench)
				{
					inBenchMode = true;
					benchCam.Current = true;
					keysParent.Visible = true;
					lmbParent.Visible = false;
				}
				else if (canUseConsole)
				{
					inConsoleMode = true;
					cashCam.Current = true;
					lmbParent.Visible = false;
				}
			}
		}

		if (e is InputEventMouseButton)
		{
			if (e.IsActionPressed("lmb"))
			{
				tryingToHoldItem = true;
			}
			if (e.IsActionReleased("lmb"))
			{
				tryingToHoldItem = false;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (Input.IsActionJustPressed("poop") && inToiletMode)
		{
			toiletLabel.Visible = false;
			// play sound and then inToiletMode = false after some time
			glob.EmitSignal("PlayerUsedToilet");
			GlobalPosition = resetPos;
			inToiletMode = false;
		}

		if (!IsOnFloor())
			velocity.Y -=
				ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * (float)delta;

		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !inBenchMode && !inConsoleMode && !inToiletMode)
			velocity.Y = JumpVelocity;

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero && !inBenchMode && !inConsoleMode && !inToiletMode)
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

	public override void _Process(double delta)
	{
		base._Process(delta);

		CheckHandCollisionAndHoldItem();
	}
}
