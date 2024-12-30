using UnityEngine;

public enum NpcEnum
{
    Player,
    Cashier,
    VegetableAisle,
    MeatCounter,
    FruitStand
}

public enum ItemEnum
{
    Vegetable,
    Beef,
    Apple,
    Salad,
    Steak
}

[System.Serializable]
public struct ItemAmountPair
{
    public ItemEnum item;
    public int amount;

    public ItemAmountPair(ItemEnum item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

[System.Serializable]
public struct ActiveDialogue
{
    public NpcEnum npc;
    public int dialogueIndex;
}