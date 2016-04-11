﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Melia.Shared.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melia.Shared.World
{
	public abstract class BaseCharacter : IObject
	{
		/// <summary>
		/// Id of the character's account.
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// Character's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Character's team name.
		/// </summary>
		public string TeamName { get; set; }

		/// <summary>
		/// Character's job.
		/// </summary>
		public Job Job { get; set; }

		/// <summary>
		/// Character's gender.
		/// </summary>
		public Gender Gender { get; set; }

		/// <summary>
		/// Character's hair style.
		/// </summary>
		public byte Hair { get; set; }

		/// <summary>
		/// Character's level.
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		/// The map the character is in.
		/// </summary>
		public int MapId { get; set; }

		/// <summary>
		/// Character's position.
		/// </summary>
		public Position Position { get; set; }

		/// <summary>
		/// Character's direction.
		/// </summary>
		public Direction Direction { get; set; }

		/// <summary>
		/// Character's head's direction.
		/// </summary>
		public Direction HeadDirection { get; set; }

		/// <summary>
		/// Current experience points.
		/// </summary>
		public int Exp { get; set; }

		/// <summary>
		/// Current maximum experience points.
		/// </summary>
		public int MaxExp { get; set; }

		/// <summary>
		/// Health points.
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.HP, PropertyType.INT)]
		public IntProperty Hp { get; set; }

		/// <summary>
		/// Maximum health points.
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.MHP, PropertyType.INT)]
		public IntProperty MaxHp { get; set; }

		/// <summary>
		/// Spell points.
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.SP, PropertyType.INT)]
		public IntProperty Sp { get; set; }

		/// <summary>
		/// Maximum spell points.
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.MSP, PropertyType.INT)]
		public IntProperty MaxSp { get; set; }

		/// <summary>
		/// Stamina points.
		/// </summary>
		public int Stamina { get; set; }

		/// <summary>
		/// Maximum stamina points.
		/// </summary>
		public int MaxStamina { get; set; }

		/// <summary>
		/// Gets or sets character's strength (STR).
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.STR, PropertyType.INT)]
		public IntProperty Str { get; set; }

		/// <summary>
		/// Gets or sets character's vitality (CON).
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.CON, PropertyType.INT)]
		public IntProperty Con { get; set; }

		/// <summary>
		/// Gets or sets character's intelligence (INT).
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.INT, PropertyType.INT)]
		public IntProperty Int { get; set; }

		/// <summary>
		/// Gets or sets character's spirit (SPR/MNA).
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.MNA, PropertyType.INT)]
		public IntProperty Spr { get; set; }

		/// <summary>
		/// Gets or sets character's agility (DEX).
		/// </summary>
		[PropertyAttribute(ObjectProperty.PC.DEX, PropertyType.INT)]
		public IntProperty Dex { get; set; }

		/// <summary>
		/// Returns stance, based on job and other factors.
		/// </summary>
		public int Stance
		{
			get
			{
				var cls = (short)this.Job / 1000;

				switch (cls)
				{
					case 1: return 10000; // Swordsman
					case 2: return 10006; // Wizard
					case 3: return 10008; // Archer
					case 4:
					case 9: return 10004; // Cleric
				}

				throw new Exception("Unknown job class.");
			}
		}

		/// <summary>
		/// Sets character's position.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void SetPosition(float x, float y, float z)
		{
			this.Position = new Position(x, y, z);
		}

		/// <summary>
		/// Sets character's direction.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetDirection(float x, float y)
		{
			this.Direction = new Direction(x, y);
		}

		/// <summary>
		/// Sets character's direction.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetHeadDirection(float x, float y)
		{
			this.HeadDirection = new Direction(x, y);
		}
	}
}
