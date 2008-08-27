#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
// - Added visibility and active flags
// - Made menu choices into a maxtrix
// - Made input presses read from a settings class
// - Has the ability to reset the selected menu option
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForceData.Base;
#endregion

namespace ChronoForce
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up, down, left, right to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        List<MenuEntry>[] menuEntries = new List<MenuEntry>[ChronoConstants.cMaxMenus];
        // Keeps track of the position of the cursor
        int selectedRowEntry = -1;
        int selectedColEntry = -1;
        // Title of this battle menu, though not really used except for debugging
        string menuTitle;
        // Flag for setting visibilty of the different menus
        bool[] isMenuVisible = new bool[ChronoConstants.cMaxMenus];
        // Start position to render the menu (with default values)
        Vector2 startPosition = new Vector2(426, 150);
        // Offset for each menu to the right
        int menuOffset = 50;
        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.  By default, it returns the first
        /// list of menu entries for cases where only a single line is needed.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries[0]; }
        }

        /// <summary>
        /// Gets the maxtrix of menu entries, so derived classes can add
        /// or change the menu contents.  This returns the full 2D list
        /// of menu entries.
        /// </summary>
        protected IList<MenuEntry>[] MenuEntriesList
        {
            get { return menuEntries; }
        }

        /// <summary>
        /// Offset for the next menu to the right.
        /// </summary>
        protected int MenuOffset
        {
            get { return menuOffset; }
            set { menuOffset = value; }
        }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor with default values and a provided menu title.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            // Initialize the menus
            for (int i = 0; i < ChronoConstants.cMaxMenus; i++)
                menuEntries[i] = new List<MenuEntry>();

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Constructor with menu title and stated starting position
        /// </summary>
        /// <param name="menuTitle">Title of the menu</param>
        /// <param name="startPos">Where to start the menu</param>
        public MenuScreen(string menuTitle, Vector2 startPos)
            : this(menuTitle)
        {
            startPosition = startPos;
        }

        /// <summary>
        /// Constructor with title, start position, and menu offset.
        /// </summary>
        /// <param name="menuTitle">Title of the menu</param>
        /// <param name="startPos">Where to start the menu</param>
        /// <param name="offset">How far the right menu will be from the previous</param>
        public MenuScreen(string menuTitle, Vector2 startPos, int offset)
            : this(menuTitle)
        {
            startPosition = startPos;
            menuOffset = offset;
        }

        /// <summary>
        /// Constructor with all parameters.
        /// </summary>
        /// <param name="menuTitle">Title of the menu</param>
        /// <param name="startPos">Where to start the menu</param>
        /// <param name="offset">How far the right menu will be from the previous</param>
        /// <param name="transitionTime">Transition time when switching or showing the menu</param>
        public MenuScreen(string menuTitle, Vector2 startPos, int offset, float transitionTime)
            : this(menuTitle)
        {
            startPosition = startPos;
            menuOffset = offset;

            // Overrides the default transition times
            TransitionOnTime = TimeSpan.FromSeconds(transitionTime);
            TransitionOffTime = TimeSpan.FromSeconds(transitionTime);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows access to modify visiblity of specific menus
        /// </summary>
        public void SetMenuVisible(int index, bool visible)
        {
            // Safety check
            if (index >= ChronoConstants.cMaxMenus || index < 0)
            {
                Console.WriteLine("MenuScreen::SetMenuVisible: Index of {0} is invalid.", index);
                return;
            }

            isMenuVisible[index] = visible;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input, int elapsed)
        {
            // If the selected rows or column isn't valid, then there's no visible menu
            // to select from, so skip the inputs
            if (selectedColEntry == -1 || selectedRowEntry == -1)
                return;

            // Move up menu entry?
            if (input.UpKey)
            {
                do
                {
                    selectedRowEntry--;

                    if (selectedRowEntry < 0)
                        selectedRowEntry = menuEntries[selectedColEntry].Count - 1;
                } while (menuEntries[selectedColEntry][selectedRowEntry].ShowMe == false ||
                    menuEntries[selectedColEntry][selectedRowEntry].IsActive == false);
            }

            // Move down menu entry?
            if (input.DownKey)
            {
                do
                {
                    selectedRowEntry++;

                    if (selectedRowEntry >= menuEntries[selectedColEntry].Count)
                        selectedRowEntry = 0;
                }while(menuEntries[selectedColEntry][selectedRowEntry].ShowMe == false ||
                    menuEntries[selectedColEntry][selectedRowEntry].IsActive == false);
            }

            // Move to the left menu entry?
            if (input.LeftKey)
            {
                // Do an additional check to see if there's menu entry to select
                // If not, keep advancing left until we hit one
                do
                {
                    selectedColEntry--;

                    if (selectedColEntry < 0)
                        selectedColEntry = ChronoConstants.cMaxMenus - 1;

                } while (menuEntries[selectedColEntry].Count == 0 ||
                    !isMenuVisible[selectedColEntry] ||
                    menuEntries[selectedColEntry][selectedRowEntry].ShowMe == false ||
                    menuEntries[selectedColEntry][selectedRowEntry].IsActive == false);
            }

            // Move to the right menu entry?
            if (input.RightKey)
            {
                // Do an additional check to see if there's menu entry to select
                // If not, keep advancing right until we hit one
                do
                {
                    selectedColEntry++;

                    if (selectedColEntry >= ChronoConstants.cMaxMenus)
                        selectedColEntry = 0;

                } while (menuEntries[selectedColEntry].Count == 0 ||
                    !isMenuVisible[selectedColEntry] ||
                    menuEntries[selectedColEntry][selectedRowEntry].ShowMe == false ||
                    menuEntries[selectedColEntry][selectedRowEntry].IsActive == false);
            }

            // Accept the menu?  Note there's no cancel for this
            if (input.ConfirmKey)
            {
                OnSelectEntry(selectedRowEntry, selectedColEntry);
            }
            else if (input.CancelKey)
            {
                OnCancel();
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryRowIndex, int entryColIndex)
        {
            menuEntries[entryColIndex][entryRowIndex].OnSelectEntry();
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }

        /// <summary>
        /// Resets the selected menu entry.
        /// </summary>
        protected void ResetSelected()
        {
            selectedRowEntry = -1;
            selectedColEntry = -1;
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < ChronoConstants.cMaxMenus; i++)
            {
                for (int j = 0; j < menuEntries[i].Count; j++)
                {
                    // If nothing is selected, select the first available menu entry
                    // if the menu is currently in focus.
                    if (selectedColEntry == -1 && selectedColEntry == -1 &&
                        !otherScreenHasFocus)
                    {
                        if (menuEntries[i][j].IsActive && menuEntries[i][j].ShowMe)
                        {
                            selectedColEntry = i;
                            selectedRowEntry = j;
                        }
                    }

                    bool isSelected = IsActive && (i == selectedColEntry && j == selectedRowEntry);

                    menuEntries[i][j].Update(this, isSelected, gameTime);
                }
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = Fonts.MenuFont;
            Vector2 position = new Vector2(startPosition.X, startPosition.Y);

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < ChronoConstants.cMaxMenus; i++)
            {
                // Reset the position and move the menu to the right
                position.Y = startPosition.Y;
                position.X = startPosition.X + (i * menuOffset);

                if (isMenuVisible[i])
                {
                    for (int j = 0; j < menuEntries[i].Count; j++)
                    {
                        MenuEntry menuEntry = menuEntries[i][j];

                        bool isSelected = IsActive && (i == selectedColEntry && j == selectedRowEntry);

                        menuEntry.Draw(this, position, isSelected, gameTime);

                        position.Y += menuEntry.GetHeight(this);
                    }
                }
            }

            spriteBatch.End();
        }


        #endregion
    }
}
