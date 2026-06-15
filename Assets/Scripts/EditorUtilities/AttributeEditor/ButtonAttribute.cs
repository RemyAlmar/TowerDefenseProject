using System;

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute
{
    public string Name { get; private set; }

    public ButtonAttribute(string name = null)
    {
        Name = name;
    }
}