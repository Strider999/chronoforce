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
using ChronoForce.Character;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Main game engine that handles loading sprites, tiles, handling battle sequences,
    /// and keeping track of player data.
    /// </summary>
    public class ChronosEngine
    {
        #region Fields
        // Content manager used by the game
        ContentManager content;
        
        // Map Engine
        MapEngine mapEngine;
        // Battle Engine
        BattleEngine battleEngine;
        // Debug flag
        private bool debugOn;
        #endregion

        #region Public Accessors

        /// <summary>
        /// Returns the map engine
        /// </summary>
        public MapEngine Map
        {
            get { return mapEngine; }
        }

        /// <summary>
        /// Returns the battle engine
        /// </summary>
        public BattleEngine Battle
        {
            get { return battleEngine; }
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
        /// Constructor loads and initializes the class
        /// </summary>
        /// <param name="graphicsComponent">Place where to render the graphics to</param>
        /// <param name="contentManager">Content to load from</param>
        public ChronosEngine(GraphicsDevice graphicsComponent, ContentManager contentManager)
        {
            if (graphicsComponent == null)
            {
                throw new ArgumentNullException("graphicsComponent");
            }

            // TODO:  Load party characters and pass them to the engine if necessary

            // Load the map engine
            mapEngine = new MapEngine(graphicsComponent, contentManager);

            // Load the battle engine
            battleEngine = new BattleEngine(graphicsComponent, contentManager);
        }

        #endregion

        public void Draw(GameTime gameTime)
        {
            //mapEngine.Draw(gameTime);
        }
    }
}
