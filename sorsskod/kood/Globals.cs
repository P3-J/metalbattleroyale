using Godot;
using System;

public partial class Globals : Node
{
    [Signal] public delegate void SpawnItemEventHandler(string itemName);
    [Signal] public delegate void SendItemsToRegisterEventHandler(string[] itemNames);
    [Signal] public delegate void OrderInEventHandler();
    [Signal] public delegate void OrderDoneEventHandler(bool Done, int orderValue = 0);
    [Signal] public delegate void PassiveBenchInteractEventHandler(int money);
    [Signal] public delegate void PassiveBenchResponseEventHandler(int moneyChange, int newNeededMoney);

}
