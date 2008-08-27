#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
// - Added greying out entry if disabled
// - Added hiding/showing entries
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class MenuEntry
    {
        #region Fields

        /// <summary>
        /// The text rendered for this entry.
        /// </summary>
        string text;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float selectionFade;

        /// <summary>
        /// If true, the menu entry is shown
        /// </summary>
        bool showMe;

        /// <summary>
        /// If true, the menu entry is active.  Else, grey out the entry.
        /// </summary>
        bool isActive;
        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the text of this menu entry.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Gets or sets the visibility of the menu entry.
        /// </summary>
        public bool ShowMe
        {
            get { return showMe; }
            set { showMe = value; }
        }

        /// <summary>
        /// Gets or sets whether or not the menu is active.
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        #endregion

        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, EventArgs.Empty);
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public MenuEntry(string text)
        {
            this.text = text;
            showMe = true;
            isActive = true;
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(MenuScreen screen, bool isSelected,
                                                      GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreen screen, Vector2 position,
                                 bool isSelected, GameTime gameTime)
        {
            // If the entry is visible, draw it
            if (showMe)
            {
                float scale = 1;

                // If the entry is active, draw the selected entry in yellow, otherwise white.
                // If the entry is inactive, draw it in gray.
                Color color = isActive ? (isSelected ? Color.Yellow : Color.White) : Color.Gray;

                // Modify the alpha to fade text out during transitions.
                color = new Color(color.R, color.G, color.B, screen.TransitionAlpha);

                // Draw text, centered on the middle of each line.
                ScreenManager screenManager = screen.ScreenManager;
                SpriteBatch spriteBatch = screenManager.SpriteBatch;
                SpriteFont font = screenManager.Font;

                Vector2 origin = new Vector2(0, font.LineSpacing / 2);

                spriteBatch.DrawString(font, text, position, color, 0,
                                       origin, scale, SpriteEffects.None, 0);
            }
        }


        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(MenuScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }


        #endregion
    }
}
