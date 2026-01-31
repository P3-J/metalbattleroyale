using Godot;
using System;

public partial class Globals : Node
{
    [Signal] public delegate void SpawnItemEventHandler(string itemName);
    [Signal] public delegate void SendItemsToRegisterEventHandler(string[] itemNames);
    [Signal] public delegate void OrderInEventHandler();
    [Signal] public delegate void OrderDoneEventHandler(bool Done);

}
