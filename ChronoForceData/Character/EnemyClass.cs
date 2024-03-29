#region File Description
//-----------------------------------------------------------------------------
// EnemyClass.cs
//
// Copyright (c) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForceData.Character
{
    /// <summary>
    /// Main class for handling enemies and their actions
    /// </summary>
    public class EnemyClass : CombatCharacter
    {
        #region Fields

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public EnemyClass()
            : base()
        {

        }

        /// <summary>
        /// Constructor with a provided name
        /// </summary>
        /// <param name="name">Name of the enemy</param>
        public EnemyClass(string name)
            : base(name)
        {

        }

                /// <summary>
        /// Constructor with provided name and stats
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="stats">Stats of the player</param>
        public EnemyClass(string name, CharacterStats stats)
            : base(name, stats)
        {

        }

        /// <summary>
        /// Constructor with provided name, stats, ID, and sprite
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="stats">Stats of the player</param>
        /// <param name="playerID">ID used in the party position</param>
        public EnemyClass(string name, CharacterStats stats, AnimatingSprite sprite)
            : base(name, stats, sprite)
        {

        }
        #endregion

    }
}
