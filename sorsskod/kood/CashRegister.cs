using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

public class Mask
{
    public string Name { get; set; }
    public int Price { get; set; }
    // TODO: Add some sort of value to separate effects of masks
}

public partial class CashRegister : PanelContainer
{
    private string sum = "0";

    private RichTextLabel sumAmountLabel;

    private VBoxContainer lineItemContainer;

    const int maxSumLength = 10;

    private List<int> itemPrices = [];

    private MarginContainer marginContainer;

    static Random random = new();

    private readonly Mask[] maskList =
    [
        new Mask { Name = "Fire Mask", Price = 75 },
        new Mask { Name = "Water Mask", Price = 175 },
        new Mask { Name = "Earth Mask", Price = 225 },
        new Mask { Name = "Air Mask", Price = 300 },
    ];

    const int maxAllowedMasksPerOrder = 5;

    const int orderCountWeightedTowards = 2;

    public bool orderInProgress = false;

    public override void _Ready()
    {
        sumAmountLabel = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/SumLineContainer/SumAmountLabel"
        );

        ChangeSumAmount(sum);

        lineItemContainer = GetNode<VBoxContainer>(
            "MarginContainer/CashRegisterContainer/LineItemContainer"
        );

        marginContainer = GetNode<MarginContainer>("MarginContainer");

        RichTextLabel cursor = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/SumLineContainer/Cursor"
        );

        Tween tween = GetTree().CreateTween();

        tween.SetLoops();
        tween.TweenProperty(cursor, "modulate:a", 1.0, 0);
        tween.TweenInterval(0.6f);
        tween.TweenProperty(cursor, "modulate:a", 0.0, 0);
        tween.TweenInterval(0.6f);
    }

    public void AddLineItem(string itemName, int itemPrice)
    {
        PackedScene scene = GD.Load<PackedScene>("res://sorsskod/scenes/RegisterItem.tscn");

        HBoxContainer registerItem = scene.Instantiate<HBoxContainer>();

        RichTextLabel itemNameNode = registerItem.GetNode<RichTextLabel>("ItemName");
        RichTextLabel itemPriceNode = registerItem.GetNode<RichTextLabel>("ItemPrice");

        itemNameNode.Text = $"{itemName} : ";
        itemPriceNode.Text = $"${itemPrice}";

        lineItemContainer.AddChild(registerItem);

        itemPrices.Add(itemPrice);
    }

    public void UseKeyInput(InputEventKey eventKey)
    {
        if (!eventKey.Pressed)
            return;

        if (
            eventKey.Keycode >= Key.Key0 && eventKey.Keycode <= Key.Key9
            || eventKey.Keycode >= Key.Kp0 && eventKey.Keycode <= Key.Kp9
        )
        {
            if (sum.Length > maxSumLength)
            {
                return;
            }

            int pressedNumber = (char)eventKey.Unicode - '0';

            if (sum == "0")
            {
                ChangeSumAmount(pressedNumber.ToString());
            }
            else
            {
                ChangeSumAmount(sum + pressedNumber.ToString());
            }
        }

        if (eventKey.Keycode == Key.Backspace && sum.Length > 0)
        {
            ChangeSumAmount(sum[..^1]);
        }

        if (eventKey.Keycode == Key.Enter)
        {
            HandleSumSubmit();
        }

        if (!orderInProgress && eventKey.Keycode == Key.Space)
        {
            orderInProgress = true;
            GenerateOrder();
        }
    }

    private void ClearState()
    {
        Array<Node> lineItems = lineItemContainer.GetChildren();
        int childCount = lineItems.Count;

        for (int i = 0; i < childCount; i++)
        {
            lineItems[i].QueueFree();
        }

        itemPrices = [];
    }

    private void HandleSumSubmit()
    {
        bool entryValidity = CheckSumEntryValidity();

        if (entryValidity == false)
        {
            MakeResultMessageLabel("FAIL!");
        }
        else
        {
            orderInProgress = false;
            MakeResultMessageLabel("SOLD!");
            ClearState();
        }

        ChangeSumAmount("0");
    }

    private void ChangeSumAmount(string sumInput)
    {
        sum = sumInput;

        sumAmountLabel.Text = $"${sum}";
    }

    private bool CheckSumEntryValidity()
    {
        if (!(sum.Length > 0))
        {
            return false;
        }
        int realSum = itemPrices.Sum();
        long userSum = Convert.ToInt64(sum);

        return realSum == userSum;
    }

    private async void MakeResultMessageLabel(String message)
    {
        RichTextLabel resultLabel = new();

        resultLabel.AddThemeFontSizeOverride("normal_font_size", 50);

        resultLabel.Text = message;

        resultLabel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        resultLabel.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        resultLabel.AutowrapMode = TextServer.AutowrapMode.Off;
        resultLabel.FitContent = true;

        marginContainer.AddChild(resultLabel);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(resultLabel, "modulate", Colors.Green, 1.0f);
        tween.TweenProperty(resultLabel, "scale", Vector2.Zero, 1.0f);
        tween.TweenCallback(Callable.From(resultLabel.QueueFree));
    }

    public void GenerateOrder()
    {
        int amountOfMasks = GenerateOrderMaskCount();

        for (int i = 0; i < amountOfMasks; i++)
        {
            Mask randomMask = maskList[random.Next(maskList.Length)];
            AddLineItem(randomMask.Name, randomMask.Price);
        }
    }

    public static int GenerateOrderMaskCount()
    {
        List<int> possibleMaskCounts = Enumerable.Range(1, maxAllowedMasksPerOrder).ToList<int>();

        possibleMaskCounts.Remove(orderCountWeightedTowards);

        int randomInt = random.Next(101);

        if (randomInt < 50) // Bad weighted random
        {
            return orderCountWeightedTowards;
        }
        else
        {
            return possibleMaskCounts[random.Next(possibleMaskCounts.Count)];
        }
    }
}
