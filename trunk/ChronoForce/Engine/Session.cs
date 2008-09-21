#region File Description
//-----------------------------------------------------------------------------
// Session.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// - Keeps track of all game states and
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using ChronoForceData.Character;
using ChronoForce.Screens;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Class that stores all the information about the current play session of the game
    /// </summary>
    class Session
    {
        #region Singleton

        /// <summary>
        /// The single Session instance that can be active at a time.
        /// </summary>
        private static Session singleton;

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

        #region Fields

        // Contains the manager for loading dialog boxes and party screens
        ScreenManager screenManager;

        /// <summary>
        /// Returns the screen manager used to add screens
        /// </summary>
        public static ScreenManager Manager
        {
            get { return singleton.screenManager; }
        }

        // Overlay for map transitions
        MapTitleScreen mapTitle = new MapTitleScreen();

        /// <summary>
        /// Returns the map title overlay screen for map transitions
        /// </summary>
        public static MapTitleScreen MapTitle
        {
            get { return singleton.mapTitle; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads the session
        /// </summary>
        /// <param name="sManager">ScreenManager that handles the screens</param>
        public static void LoadSession(ScreenManager sManager)
        {
            // TODO:  Clear any previous session

            // Create a new session
            singleton = new Session();

            // Store the screen manager
            singleton.screenManager = sManager;

            // TODO:  Load map engine here or in the world screen?
        }

        #endregion

        #region Screen Handling

        public static void CreateDialog()
        {

        }

        /// <summary>
        /// Creates a map title above the map for map transitions
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        public static void CreateMapTitle(string title, string subtitle)
        {
            Manager.RemoveScreen(MapTitle);

            MapTitle.ResetScreen();
            MapTitle.Title = title;
            MapTitle.Subtitle = subtitle;

            Manager.AddScreen(MapTitle);
        }

        #endregion
    }
}
