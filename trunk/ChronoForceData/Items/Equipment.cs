#region File Description
//-----------------------------------------------------------------------------
// Equipment.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using ChronoForceData.Base;
using ChronoForceData.Character;
#endregion

namespace ChronoForceData.Items
{
    #region Enums

    /// <summary>
    /// Describes the type of equipment
    /// </summary>
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Accessory
    }

    /// <summary>
    /// Enum to determine the shard type in an equipment
    /// </summary>
    public enum ShardType
    {
        None,
        Time,
        Anti
    }

    /// <summary>
    /// Elemental attack/defense types
    /// </summary>
    public enum ElementalType
    {
        None,
        Lightning,
        Fire,
        Water,
        Dark,
        Time
    }

    #endregion

    /// <summary>
    /// Handles all equipment related items, including weapons, armor, and accessories.
    /// </summary>
    class Equipment
    {
        #region Fields
        // Type of equipment
        EquipmentType type = EquipmentType.Weapon;
        // ID of the weapon, armor, or accessory
        int id = -1;
        // General modifications to the character class
        CharacterStats stats;
        // Determines if the equipment can have shards embedded
        bool ableToEmbed = false;
        // If it can embed, the type of shard currently in the item
        ShardType shard = ShardType.None;
        // Chance for the person to cause critical damage
        int critChance = 0;
        // Elemental type for defense and offense depending on the item
        ElementalType element = ElementalType.None;
        // Status protection or status attacks depending on the item
        StatusEffects statusType = StatusEffects.None;
        // If offense, chance to cause the status effect on enemy
        int statusChance = 0;
        // For all other abilities, the special bitfield will hold
        // additional data
        int special = 0;
        #endregion

        #region Public Accessors

        /// <summary>
        /// Type of equipment
        /// </summary>
        public EquipmentType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Equipment ID number
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Physical strength of the equipment
        /// </summary>
        public int ATK
        {
            get { return stats.Attack; }
            set { stats.Attack = value; }
        }

        /// <summary>
        /// Physical defense of the equipment
        /// </summary>
        public int DEF
        {
            get { return stats.Defense; }
            set { stats.Defense = value; }
        }

        /// <summary>
        /// How much speed the equipment adds
        /// </summary>
        public int SPD
        {
            get { return stats.Speed; }
            set { stats.Speed = value; }
        }

        /// <summary>
        /// How accurate the equipment is
        /// </summary>
        public int ACC
        {
            get { return stats.Accuracy; }
            set { stats.Accuracy = value; }
        }

        /// <summary>
        /// Magic strength of equipment
        /// </summary>
        public int MAG
        {
            get { return stats.Magic; }
            set { stats.Magic = value; }
        }

        /// <summary>
        /// Magic defense of equipment
        /// </summary>
        public int MDEF
        {
            get { return stats.MDefense; }
            set { stats.MDefense = value; }
        }

        /// <summary>
        /// Chrono affinity of the equipment
        /// </summary>
        public int TIME
        {
            get { return stats.Time; }
            set { stats.Time = value; }
        }

        /// <summary>
        /// Whether or not shards can be embedded in equipment
        /// </summary>
        public bool AbleToEmbed
        {
            get { return ableToEmbed; }
            set { ableToEmbed = value; }
        }

        /// <summary>
        /// Type of shard in the equipment
        /// </summary>
        public ShardType Shard
        {
            get { return shard; }
            set { shard = value; }
        }

        /// <summary>
        /// Critical chance of the equipment
        /// </summary>
        public int CritChance
        {
            get { return critChance; }
            set { critChance = value; }
        }

        /// <summary>
        /// Element in the equipment
        /// </summary>
        public ElementalType Element
        {
            get { return element; }
            set { element = value; }
        }

        /// <summary>
        /// Status effect the equipment can cause or protect against
        /// </summary>
        public StatusEffects StatusType
        {
            get { return statusType; }
            set { statusType = value; }
        }

        /// <summary>
        /// Status effect chance the equipment can cause
        /// </summary>
        public int StatusChance
        {
            get { return statusChance; }
            set { statusChance = value; }
        }

        /// <summary>
        /// Extraneous special abilites of the equipment
        /// </summary>
        public int Special
        {
            get { return special; }
            set { special = value; }
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        Equipment()
        {
            // Nothing for now
        }

        #endregion
    }
}
