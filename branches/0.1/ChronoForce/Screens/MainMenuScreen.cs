#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry startMenuEntry = new MenuEntry("Start");
            MenuEntry continueMenuEntry = new MenuEntry("Continue");
            MenuEntry gameOverMenuEntry = new MenuEntry("Show Game Over");
            MenuEntry exitMenuEntry = new MenuEntry("Quit");

            // Hook up menu event handlers.
            startMenuEntry.Selected += StartMenuEntrySelected;
            continueMenuEntry.Selected += ContinueMenuEntrySelected;
            gameOverMenuEntry.Selected += GameOverMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(startMenuEntry);
            MenuEntries.Add(continueMenuEntry);
            MenuEntries.Add(gameOverMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            // Make the first menu visible
            SetMenuVisible(0, true);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Start menu entry is selected.
        /// </summary>
        void StartMenuEntrySelected(object sender, EventArgs e)
        {
            //LoadingScreen.Load(ScreenManager, true, new OverworldScreen());
            ScreenManager.AddScreen(new OverworldScreen());
        }


        /// <summary>
        /// Event handler for when the Continue menu entry is selected.
        /// </summary>
        void ContinueMenuEntrySelected(object sender, EventArgs e)
        {
            //ScreenManager.AddScreen(new ContinueMenuScreen());
        }

        /// <summary>
        /// Event handler for when the Game Over menu entry is selected.
        /// </summary>
        void GameOverMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new GameOverScreen());
        }

        /// <summary>
        /// When the user cancels the main menu, close the program
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }

        #endregion
    }
}
