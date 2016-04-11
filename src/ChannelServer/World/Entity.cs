// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Melia.Shared.World;
namespace Melia.Channel.World
{
	public interface IEntity
	{
		int Handle { get; }
		IntProperty Hp { get; }
		Map Map { get; }
		Position Position { get; }
	}
}
