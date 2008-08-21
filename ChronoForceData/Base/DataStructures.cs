#region File Description
//-----------------------------------------------------------------------------
// Enumerations.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Created by David Hsu
//
// This file contains all common enumerations and structures used in this game
// along with constants
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace ChronoForceData.Base
{
    #region Enums

    /// <summary>
    /// Enum describes status effects a character might have
    /// </summary>
    public enum StatusEffects
    {
        None = 0x0000,
        Poison = 0x0001,
        Sleep = 0x0002,
        Confusion = 0x0004,
        Blind = 0x0008,
        Stopped = 0x0010,
        Locked = 0x0020,
        Shattered = 0x0040,
        Death = 0x0080,
        All = 0x00FF // Used for defensive items
    }

    /// <summary>
    /// Abilites that a player possesses and is used for battle menus
    /// and action slots
    /// </summary>
    public enum Ability
    {
        Attack = 0x01,
        Skill = 0x02,
        Time = 0x04,
        Anti = 0x08,
        Item = 0x10
    }

    /// <summary>
    /// Enum to describe position in the menu where each action is
    /// </summary>
    public enum BattleMenuItem : int
    {
        Attack = 0,
        Skill = 1,
        Time = 2,
        Item = 3
    }

    #endregion

    #region Constants

    /// <summary>
    /// Holds all the constants used in the game
    /// </summary>
    public static class ChronoConstants
    {
        /// <summary>
        /// Max limit for number of party members in battle, mainly for enemies.
        /// </summary>
        public static readonly int cMaxPartyMembers = 5;

        /// <summary>
        /// Max limit for number of player party members in battle.
        /// </summary>
        public static readonly int cMaxPlayerParty = 3;

        /// <summary>
        /// Max number of columns the menu screen can have.
        /// </summary>
        public static readonly int cMaxMenus = 3;

        /// <summary>
        /// Max number of choices on the battle menu.
        /// </summary>
        public static readonly int cMaxMenuItems = 4;

        /// <summary>
        /// Number to pass for the target if it targets everything
        /// </summary>
        public static readonly int cAllTargets = 999;

        /// <summary>
        /// Default delay between each frame of animation for sprites in milliseconds.
        /// This makes the animation run at roughly 30fps.
        /// </summary>
        public static readonly int cAnimationDelay = 33;
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Event arguments for any class to pass numbers through an event.  Note that
    /// the ID and target number for action slots is a bit field that allows mulitple 
    /// player and enemy IDs to be compressed in this event argument.
    /// </summary>
    public class NumberEventArgs : EventArgs
    {
        private int id;
        private int target;
        public NumberEventArgs()
        {
            this.id = -1;
            this.target = -1;
        }
        public NumberEventArgs(int id)
        {
            this.id = id;
            this.target = -1;
        }

        public NumberEventArgs(int id, int target)
        {
            this.id = id;
            this.target = target;
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Target
        {
            get { return target; }
        }
    }
    #endregion
}
