using Godot;
using Godot.Collections;

public partial class Npc : CharacterBody3D
{
	public const float Speed = 1.0f;
	public const float JumpVelocity = 4.5f;
	private NavigationAgent3D navagent;
	private bool Served = false;
	private bool Connected = false;
	Vector3 velocity;
	Vector3 nextSpot;
	[Signal]
	public delegate void ReachedTargetEventHandler();	

	public override void _Ready()
	{
		base._Ready();
		GD.Print("NPC Added.");
		navagent = (NavigationAgent3D)GetNode("NavigationAgent3D");
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Connected)
		{
			return;
		}
		velocity = Velocity;
		MoveTowardsTarget();
		Velocity = velocity;
		MoveAndSlide();
		if (Served && navagent.IsTargetReached())
		{
			QueueFree();
		}
	}

	private void MoveTowardsTarget()
	{
		nextSpot = navagent.GetNextPathPosition();
		//GD.Print("Next spot for ", this, " is ", nextSpot);
		Vector3 dir = GlobalPosition.DirectionTo(nextSpot).Normalized();
		Vector3 flatTarget = new Vector3(nextSpot.X, GlobalPosition.Y, nextSpot.Z);
		if (!GlobalPosition.IsEqualApprox(flatTarget))
		{
			LookAt(flatTarget, Vector3.Up);
		}
		velocity.X = dir.X * 5;
		velocity.Z = dir.Z * 5;
	}

	public void SetTargetPos(Vector3 pos)
	{
		var map = GetWorld3D().NavigationMap;
		var p = NavigationServer3D.MapGetClosestPoint(map, pos);
		navagent.TargetPosition = p;
		//GD.Print("Target post for" , this, " \n Position:", pos);
		
	}

	public void _on_front_connector_area_entered(Area3D area) {
		GD.Print("Connector entered something.");
		if (area.IsInGroup("npcConnector"))
		{
			EmitSignal(nameof(ReachedTarget));
			Connected = true;
		}
	}

	public void WalkAway(Vector3 WalkAwayPoint)
	{
		var map = GetWorld3D().NavigationMap;
		var p = NavigationServer3D.MapGetClosestPoint(map, WalkAwayPoint);
		navagent.TargetPosition = p;
		// Disable the BackConnector Area3D
		var backConnector = (Area3D)GetNode("BackConnector");
		var frontConnector = (Area3D)GetNode("FrontConnector");
		frontConnector.Monitoring = false;
		frontConnector.Monitorable = false;
		backConnector.Monitoring = false;
		backConnector.Monitorable = false;
		backConnector.Visible = false;
		Connected = false;
		Served = true;
		GD.Print("NPC walking away: ", this);
	}

	public void WalkUp(Vector3 queuePosition)
	{
		var map = GetWorld3D().NavigationMap;
		var p = NavigationServer3D.MapGetClosestPoint(map, queuePosition);
		navagent.TargetPosition = p;
		Connected = false;
		GD.Print("NPC walking up: ", this);
	}

	public Vector3 getBackConnectorPosition() {
		var backConnector = (Area3D)GetNode("BackConnector");
		return backConnector.GlobalPosition;
	}
}
