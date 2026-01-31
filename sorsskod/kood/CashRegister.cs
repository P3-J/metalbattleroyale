using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

public class Mask : IEquatable<Mask>
{
    public string Name { get; set; }
    public int Price { get; set; }

    public bool IsFulfilled { get; set; }

    // TODO: Add some sort of value to separate effects of masks

    public bool Equals(Mask mask) => mask.Name == Name;
}

public partial class CashRegister : PanelContainer
{
    private string sum = "0";

    private RichTextLabel sumAmountLabel;

    private VBoxContainer lineItemContainer;

    const int maxSumLength = 10;

    private List<Mask> masksInCurrentOrder = [];

    private MarginContainer marginContainer;

    static Random random = new();

    Globals glob;

    private readonly Mask[] maskList =
    [
        new Mask
        {
            Name = "Protective Mask",
            Price = 75,
            IsFulfilled = false,
        },
        new Mask
        {
            Name = "Ninja Mask",
            Price = 175,
            IsFulfilled = false,
        },
    ];

    const int maxAllowedMasksPerOrder = 5;

    const int orderCountWeightedTowards = 2;

    public bool orderInProgress = false;

    bool orderIsFulfilled = false;

    RichTextLabel fulfillOrderLabel;

    RichTextLabel waitForCustomersLabel;

    HBoxContainer sumLineContainer;

    public override void _Ready()
    {
        sumAmountLabel = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/SumLineContainer/SumAmountLabel"
        );

        glob = GetNode<Globals>("/root/Globals");
        glob.Connect("SendItemsToRegister", new Callable(this, nameof(AttemptOrderFulfillment)));
        glob.Connect("OrderIn", new Callable(this, nameof(OrderCameIn)));

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

        fulfillOrderLabel = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/FulfillOrderLabel"
        );

        waitForCustomersLabel = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/WaitForCustomersLabel"
        );

        sumLineContainer = GetNode<HBoxContainer>(
            "MarginContainer/CashRegisterContainer/SumLineContainer"
        );

        HandleRegisterUIState();
    }

    public void AddLineItem(Mask mask)
    {
        PackedScene scene = GD.Load<PackedScene>("res://sorsskod/scenes/RegisterItem.tscn");

        HBoxContainer registerItem = scene.Instantiate<HBoxContainer>();

        RichTextLabel itemNameNode = registerItem.GetNode<RichTextLabel>("ItemName");
        RichTextLabel itemPriceNode = registerItem.GetNode<RichTextLabel>("ItemPrice");

        itemNameNode.Text = $"{mask.Name} : ";
        itemPriceNode.Text = $"${mask.Price}";

        lineItemContainer.AddChild(registerItem);

        masksInCurrentOrder.Add(mask);
    }

    public void UseKeyInput(InputEventKey eventKey)
    {
        if (!eventKey.Pressed || !orderIsFulfilled)
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

        if (eventKey.Keycode == Key.Enter && orderInProgress)
        {
            HandleSumSubmit();
        }

        /* if (!orderInProgress && eventKey.Keycode == Key.Space)
        {
            orderInProgress = true;
            GenerateOrder();
        } */
    }

    private void OrderCameIn()
    {
        orderInProgress = true;
        orderIsFulfilled = false;
        HandleRegisterUIState();
        GenerateOrder();
    }

    private void ClearState()
    {
        Array<Node> lineItems = lineItemContainer.GetChildren();
        int childCount = lineItems.Count;

        for (int i = 0; i < childCount; i++)
        {
            lineItems[i].QueueFree();
        }

        masksInCurrentOrder = [];
        orderInProgress = false;
        orderIsFulfilled = false;
        HandleRegisterUIState();
    }

    private void HandleSumSubmit()
    {
        bool entryValidity = CheckSumEntryValidity();

        if (entryValidity == false || !masksInCurrentOrder.All(mask => mask.IsFulfilled == true))
        {
            MakeResultMessageLabel("FAIL!");
            glob.EmitSignal("OrderDone", true); // fix this later
        }
        else
        {
            MakeResultMessageLabel("SOLD!");
            glob.EmitSignal("OrderDone", true);
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
        int realSum = masksInCurrentOrder.Sum(mask => mask.Price);
        long userSum = Convert.ToInt64(sum);

        return realSum == userSum;
    }

    private void MakeResultMessageLabel(string message)
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
            AddLineItem(randomMask);
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

    public bool AttemptOrderFulfillment(string[] areaMaskNames)
    {
        foreach (string m in areaMaskNames)
        {
            GD.Print(m);
        }

        int masksInCurrentOrderCount = masksInCurrentOrder.Count;

        List<string> areaMaskNamesList = [.. areaMaskNames];

        if (masksInCurrentOrderCount == 0)
        {
            return false;
        }

        bool orderCompleted = true;

        for (int i = 0; i < masksInCurrentOrderCount; i++)
        {
            int indexOfAreaMask = areaMaskNamesList.IndexOf(masksInCurrentOrder[i].Name);

            HBoxContainer orderMaskRegisterItem = lineItemContainer.GetChild<HBoxContainer>(i);

            if (indexOfAreaMask == -1)
            {
                orderCompleted = false;
                masksInCurrentOrder[i].IsFulfilled = false;
                HandleRegisterItemFulfillment(orderMaskRegisterItem, false);
            }
            else
            {
                areaMaskNamesList.RemoveAt(indexOfAreaMask);
                masksInCurrentOrder[i].IsFulfilled = true;
                HandleRegisterItemFulfillment(orderMaskRegisterItem, true);
            }
        }

        orderIsFulfilled = orderCompleted;

        HandleRegisterUIState();

        return orderCompleted;
    }

    private void HandleRegisterItemFulfillment(HBoxContainer registerItem, bool isFulfilled)
    {
        RichTextLabel checkBox = registerItem.GetNode<RichTextLabel>("CheckBox");
        if (isFulfilled)
        {
            checkBox.Text = "[✓]";
        }
        else
        {
            checkBox.Text = "[ ]";
        }
    }

    private void HandleRegisterUIState()
    {
        sumLineContainer.Visible = orderIsFulfilled;
        fulfillOrderLabel.Visible = orderInProgress && !orderIsFulfilled;
        waitForCustomersLabel.Visible = !orderInProgress;
    }
}
