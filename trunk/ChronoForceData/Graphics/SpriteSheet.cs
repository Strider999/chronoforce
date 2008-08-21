#region File Description
//-----------------------------------------------------------------------------
// SpriteSheet.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified version by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace ChronoForceData.Graphics
{

    /// <summary>
    /// Stores entries for individual sprites on a single texture.
    /// </summary>
    public class SpriteSheet
    {
        #region Fields
        private Texture2D texture;
        private Dictionary<int, Rectangle> spriteDefinitions;
        #endregion

        #region Constants
        private const int noTile = 255;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new Sprite Sheet
        /// </summary>
        public SpriteSheet(Texture2D sheetTexture)
        {
            texture = sheetTexture;
            spriteDefinitions = new Dictionary<int, Rectangle>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a source sprite for fast retrieval
        /// </summary>
        public void AddSourceSprite(int key, Rectangle rect)
        {
            spriteDefinitions.Add(key, rect);
        }

        /// <summary>
        /// Get the source sprite texture
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }

        /// <summary>
        /// Get the rectangle that defines the source sprite
        /// on the sheet.
        /// </summary>
        public Rectangle this[int i]
        {
            get
            {
                return spriteDefinitions[i];
            }
        }

        /// <summary>
        /// A faster lookup using refs to avoid stack copies.
        /// </summary>
        /// <param name="i">Texture key to get</param>
        /// <param name="rect">Returns the rectangle from the spriteSheet where the key is</param>
        /// <returns>True if the key is valid, False if there is no tile here</returns>
        public bool GetRectangle(ref int i, out Rectangle rect)
        {
            if (i == noTile)
            {
                // A rectangle output is needed, so just fill it with the first rectangle
                rect = spriteDefinitions[0];
                return false;
            }

            rect = spriteDefinitions[i];

            return true;
        }
        #endregion
    }

}