using System;

[Serializable, Flags]
public enum CrossingType
{
	None = 0,

	Walk = 1 << 0,
	Swim = 1 << 1,
	Fly = 1 << 2,

	Any = Walk | Swim | Fly,
}