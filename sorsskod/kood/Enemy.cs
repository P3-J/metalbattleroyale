using Godot;

public partial class Enemy : CharacterBody3D
{
	[Export] private NavigationAgent3D navagent;
	bool hasTarget = false;
	bool isDisabled = true;
	Player player;
	Vector3 velocity;
	Vector3 nextSpot;

	public override void _Ready()
	{
		base._Ready();
		navagent.Connect("velocity_computed", new Callable(this, nameof(OnNavigationAgentVelocityComputed)));
		navagent.Connect("link_reached", new Callable(this, nameof(OnNavigationAgentLinkReached))); 

		SceneTreeTimer tr = GetTree().CreateTimer(1.0);
		tr.Timeout += OnTimeOut;
	}
	public override void _PhysicsProcess(double delta)
	{
	
		//velocity = velocity.MoveToward(new Vector3(0, velocity.Y, 0), 1f * (float)delta);
		MoveTowardsTarget();
		navagent.Velocity = velocity;
		MoveAndSlide();
		velocity = Velocity;
		//GD.Print(Velocity, groundcheck.IsColliding(), IsOnFloor(), canMove);
	}

	private void MoveTowardsTarget()
	{
		Vector3 dir = GlobalPosition.DirectionTo(nextSpot).Normalized();
		velocity.X = dir.X * 5;
		velocity.Z = dir.Z * 5;
	}


	private void OnTimeOut()
	{
		hasTarget = true;
		Node p = GetTree().CurrentScene.FindChild("Player");

		if (p.IsInGroup("Player"))
		{
			isDisabled = false;
			player = (Player)p;
			SetTargetPos(player.GlobalPosition);
		} else
		{
			GD.PushWarning("no player"); 
		}

	}

	public void SetTargetPos(Vector3 pos)
	{
		var map = GetWorld3D().NavigationMap;
		var p = NavigationServer3D.MapGetClosestPoint(map, pos);
		navagent.TargetPosition = p;				
	}



	private void OnNavigationAgentVelocityComputed(Vector3 safevelo)
	{
		Velocity = safevelo;
	}


	private void OnNavigationAgentLinkReached(Godot.Collections.Dictionary data)
	{
		// do link action
	} 
	private void _on_recalc_timeout()
	{
	   SetTargetPos(player.GlobalPosition);
	   nextSpot = navagent.GetNextPathPosition();
	}






}
