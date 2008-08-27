#region File Description
//-----------------------------------------------------------------------------
// ChronosEngine.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using ChronoForceData.Character;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Main game engine that handles loading sprites, tiles, handling battle sequences,
    /// and keeping track of player data.
    /// </summary>
    public class ChronosEngine
    {
        #region Singleton

        /// <summary>
        /// The single Session instance that can be active at a time.
        /// </summary>
        private static ChronosEngine singleton;

        #endregion

        #region Party

        /// <summary>
        /// The party that is playing the game right now.
        /// </summary>
        private PartyClass party;

        /// <summary>
        /// The party that is playing the game right now.
        /// </summary>
        public static PartyClass Party
        {
            get { return (singleton == null ? null : singleton.party); }
        }

        #endregion

        #region State Data

        /// <summary>
        /// Returns true if there is an active session.
        /// </summary>
        public static bool IsActive
        {
            get { return singleton != null; }
        }

        #endregion

        #region Fields
        // Content manager used by the game
        ContentManager content;
        // Screen manager to load different screens
        ScreenManager screenManager;

        // Debug flag
        private bool debugOn;
        #endregion

        #region Public Accessors

        /// <summary>
        /// The ScreenManager used to manage all UI in the game.
        /// </summary>
        public static ScreenManager ScreenManager
        {
            get { return (singleton == null ? null : singleton.screenManager); }
        }

        /// <summary>
        /// If true, debug mode will be turned on for the game engine
        /// </summary>
        public bool DebugOn
        {
            set { debugOn = value; }
            get { return debugOn; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Private constructor loads and initializes the class
        /// </summary>
        /// <param name="graphicsComponent">Place where to render the graphics to</param>
        private ChronosEngine(ScreenManager screenManager)
        {
            if (screenManager == null)
            {
                throw new ArgumentNullException("screenManager");
            }

            // TODO:  Load party characters and pass them to the engine if necessary

            // Store the screen manager
            this.screenManager = screenManager;

            // Load the map engine
            //mapEngine = new MapEngine(contentManager);
        }

        #endregion

        #region Starting a New Session

        public static void StartNewSession(ScreenManager screenManager)
        {
            if (screenManager == null)
            {
                throw new ArgumentNullException("screenManager");
            }

            // End any existing session
            EndSession();

            // Create a new singleton
            singleton = new ChronosEngine(screenManager);
        }

        #endregion

        #region Ending a Session

        public static void EndSession()
        {
            // End the gameplay
            // -- store the gameplay session, for re-entrance
            if (singleton != null)
            {
                // clear the singleton
                singleton = null;
            }
        }

        #endregion

        public void Draw(GameTime gameTime)
        {
            //mapEngine.Draw(gameTime);
        }
    }
}
