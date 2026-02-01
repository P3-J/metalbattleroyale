using System;
using System.Collections.Generic;
using Godot;

public partial class Globals : Node
{
	[Signal] public delegate void PassiveBenchInteractEventHandler(int money);
	[Signal] public delegate void PassiveBenchResponseEventHandler(int moneyChange, int newNeededMoney);
	[Signal]
	public delegate void SpawnItemEventHandler(string itemName);

	[Signal]
	public delegate void SendItemsToRegisterEventHandler(string[] itemNames);

	[Signal]
	public delegate void OrderInEventHandler();

	[Signal]
	public delegate void OrderDoneEventHandler(
		bool Done,
		int orderValue = 0,
		string[] soldItems = null
	);
}
