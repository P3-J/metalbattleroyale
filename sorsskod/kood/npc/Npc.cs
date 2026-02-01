using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Npc : CharacterBody3D
{
	public const float Speed = 1.0f;
	public const float JumpVelocity = 4.5f;
	private NavigationAgent3D navagent;
	private AudioStreamPlayer3D voiceLines;
	private bool Served = false;
	private bool Connected = false;
	Vector3 velocity;
	Vector3 nextSpot;
	List<Node3D> Skins = new List<Node3D>();
	private AnimationPlayer animationPlayer;
	[Signal]
	public delegate void ReachedTargetEventHandler();	
	[Export] private float VoiceLineInterval = 10.0f;

	public override void _Ready()
	{
		Skins.Add((Node3D)GetNode("Node3DWalking"));
		Skins.Add((Node3D)GetNode("Node3DWalking2"));
		voiceLines = (AudioStreamPlayer3D)GetNode("AudioStreamPlayer3D");
		base._Ready();
		//GD.Print("NPC Added.");
		navagent = (NavigationAgent3D)GetNode("NavigationAgent3D");
		Random rnd = new Random();
		int skinIndex = rnd.Next(0, Skins.Count);
		for (int i = 0; i < Skins.Count; i++)
		{
			if (i == skinIndex)
			{
				Skins[i].Visible = true;
				animationPlayer = (AnimationPlayer)Skins[i].GetNode("AnimationPlayer");
			}
		}

		Timer voiceline = new Timer();
		voiceline.WaitTime = VoiceLineInterval;
		voiceline.Autostart = true;
		voiceline.OneShot = false;
		AddChild(voiceline);
		voiceline.Timeout += () => voiceLines.Playing = true;
	}


	public override void _PhysicsProcess(double delta)
	{
		if (IsOnFloor() == false)
		{
			velocity.Y -= 9.8f * (float)delta;
		}
		else
		{
			velocity.Y = 0;
		}
		if (Connected)
		{
			animationPlayer.Play("RESET");
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

		animationPlayer.Play("LegMovement");
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
		//GD.Print("NPC walking away: ", this);
		animationPlayer.Play("LegMovement");
	}

	public void WalkUp(Vector3 queuePosition)
	{
		var map = GetWorld3D().NavigationMap;
		var p = NavigationServer3D.MapGetClosestPoint(map, queuePosition);
		navagent.TargetPosition = p;
		Connected = false;
		//GD.Print("NPC walking up: ", this);
		animationPlayer.Play("LegMovement");
	}

	public Vector3 getBackConnectorPosition() {
		var backConnector = (Area3D)GetNode("BackConnector");
		return backConnector.GlobalPosition;
	}
}
