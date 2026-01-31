using System;
using System.Collections;
using Godot;
using Godot.Collections;

public partial class QueueManager : Node3D
{
	[Export] public int Difficulty = 1;
	[Export] private PackedScene npc; 
	[Export] private Area3D QueueStart;
	[Export] private Node3D SpawnPoint;
	[Export] private int maxQueueLength = 3;
	[Export] private Node3D WalkAwayPoint;
	[Export] private double minIntervalSpawns = 0.3;
	[Export] private double maxIntervalSpawns = 2.0;
	private Random random = new Random();
	private Timer spawnTimer = new Timer();
	private Queue npcQueue = new Queue();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Array<Node> temp = GetChildren();
		foreach (Node node in temp)
		{
			if (node.IsInGroup("npc")) {
				npcQueue.Enqueue(node);
			}
		}
		spawnTimer.Autostart = true;
		spawnTimer.OneShot = true;
		AddChild(spawnTimer);
		spawnTimer.Timeout += () => OnSpawnTimerTimeout();
		StartSpawnTimer();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("jump"))
		{
			ServeNpc(true);
		}
	}

	private void StartSpawnTimer()
	{
		double interval = random.NextDouble() * (maxIntervalSpawns - minIntervalSpawns) + minIntervalSpawns;
		spawnTimer.WaitTime = interval;
		spawnTimer.Start();
	}

	private void OnSpawnTimerTimeout()
	{
		CreateNpc();
		StartSpawnTimer();
	}

	public void ServeNpc(bool served)
	{
		if (served && npcQueue.Count > 0)
		{
			Npc servedNpc = (Npc)npcQueue.Dequeue();
			GD.Print("Serving NPC: ", servedNpc);
			servedNpc.WalkAway(WalkAwayPoint.GlobalPosition);
		}

		if (npcQueue.Count > 0)
		{
			Npc Next = (Npc)npcQueue.Peek();
			Next.WalkUp(QueueStart.GlobalPosition);
		}
	}

	private void OnNpcReachedTarget(Npc a)
	{
		GD.Print("NPC reached target: ", a);
		GD.Print("Spawning new NPC.");
		if (npcQueue.Count >= maxQueueLength)
		{
			GD.Print("Max queue length reached. Not spawning new NPC.");
			return;
		}

		var queueArray = npcQueue.ToArray();
		int idx = System.Array.IndexOf(queueArray, a);
		if (idx >= 0 && idx + 1 < queueArray.Length)
		{
			Npc Next = (Npc)queueArray[idx + 1];
			Next.WalkUp(a.getBackConnectorPosition());
		}
	}

	private Npc CreateNpc()
	{
		Random r = new Random();
		spawnTimer.WaitTime = r.NextDouble() * minIntervalSpawns;
		Npc newNpc = (Npc) npc.Instantiate();
		AddChild(newNpc);
		npcQueue.Enqueue(newNpc);
		newNpc.GlobalPosition = SpawnPoint.GlobalPosition;
		newNpc.ReachedTarget += () => OnNpcReachedTarget(newNpc);

		var timer = new Timer();
		timer.WaitTime = 0.1f; // 0.1 seconds
		timer.OneShot = true;
		AddChild(timer);
		timer.Timeout += () => newNpc.SetTargetPos(QueueStart.GlobalPosition);
		timer.Start();
		return newNpc;
	}
}
