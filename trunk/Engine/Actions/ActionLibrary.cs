#region File Description
//-----------------------------------------------------------------------------
// ActionLibrary.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Character;
using ChronoForce.Base;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine.Actions
{
    /// <summary>
    /// Contains all the scripts for different actions loaded from a file.
    /// </summary>
    public class ActionLibrary
    {
        #region Constants

        readonly Vector2 cOffset = new Vector2(-10, -10);
        #endregion

        #region Fields
        // Stores the raw text form of the action script in a dictionary for retrieval
        // All keys will be concatinations of the skill, so Cross Attack will be "CrossAttack"
        Dictionary<string, List<string>> actionDictionary = new Dictionary<string, List<string>>();

        // Debugger with no graphics for displaying messages in the console
        Debugger LibraryDebug;
        #endregion

        #region Initialization

        /// <summary>
        /// Creates the library
        /// </summary>
        public ActionLibrary()
        {
            LibraryDebug = new Debugger("ActionLibrary", true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads all the actions from the specified file
        /// </summary>
        /// <param name="filename">File containing the actions</param>
        public bool LoadFile(string filename)
        {
            // First, check to see if the filename has the right format
            if (!filename.Contains(".act"))
            {
                LibraryDebug.debugPrint("Filename doesn't end in .act");
                return false;
            }

            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                string msg = "File " + filename + " doesn't exist";
                LibraryDebug.debugPrint(msg);
                return false;
            }

            // TODO:  Read in a file and parse it
            return true;
        }

        /// <summary>
        /// Creates a script for commands that only involve one player and one enemy.
        /// </summary>
        /// <param name="action">String that designates the action type</param>
        /// <param name="attacker">Character that is attacking</param>
        /// <param name="defender">Character that is defending</param>
        /// <param name="startPos">Starting position of attacker</param>
        /// <param name="endPos">Position of the defender</param>
        /// <param name="battleText">Text for damage numbers</param>
        /// <returns>A full action script that animates this action in battle</returns>
        public ActionScript createActionScript(string action, CharacterBase attacker, CharacterBase defender, 
                                               int damage, BattleText battleText)
        {
            Queue<ActionSlot> script = new Queue<ActionSlot>();

            Vector2 startPos = attacker.Position;
            Vector2 endPos = defender.Position - cOffset;

            // DEBUG:  For now, just return the actions for a regular attack
            script.Enqueue(new ActionSlot(ActionCommand.MoveTo, attacker, endPos, 20));
            script.Enqueue(new ActionSlot(ActionCommand.Begin));
            script.Enqueue(new ActionSlot(ActionCommand.ShowAttack, attacker));
            script.Enqueue(new ActionSlot(ActionCommand.ShowText, battleText));
            script.Enqueue(new ActionSlot(ActionCommand.End));
            script.Enqueue(new ActionSlot(ActionCommand.TakeDamage, defender, damage));
            script.Enqueue(new ActionSlot(ActionCommand.MoveTo, attacker, startPos, 20));

            return new ActionScript(script);
        }

        #endregion
    }
}