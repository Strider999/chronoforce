#region File Description
//-----------------------------------------------------------------------------
// CharacterBase.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForceData.Base;
#endregion

namespace ChronoForceData.Character
{
    #region Structs

    /// <summary>
    /// Contains all essential character stats
    /// </summary>
    public struct CharacterStats
    {
        public int Level;    // Current charcter level
        public int Attack;   // Physical strength and attack power
        public int Defense;  // Defense against physical attacks
        public int Speed;    // How fast the time bar fills and how well they dodge
        public int Accuracy; // Ability to physically hit
        public int Magic;    // Magic power for elemental skills/spells
        public int MDefense; // Magic defense against elemental attacks
        public int Time;     // Chrono affinity, determines character CF and resistance
        public int HitPoints;      // Hit points
        public int MaxHitPoints;
        public int MagicPoints;    // Magic points for skills
        public int MaxMagicPoints;
        public int ChronoForce;    // Chrono Force, time based attacks
        public int MaxChronoForce;

        /// <summary>
        /// Constructor with all the stats except for HP, MP, and CF
        /// </summary>
        /// <param name="level"></param>
        /// <param name="attack"></param>
        /// <param name="defense"></param>
        /// <param name="speed"></param>
        /// <param name="accuracy"></param>
        /// <param name="magic"></param>
        /// <param name="mdefense"></param>
        /// <param name="time"></param>
        public CharacterStats(int level, int attack, int defense, int speed,
            int accuracy, int magic, int mdefense, int time)
        {
            Level = level;
            Attack = attack;
            Defense = defense;
            Speed = speed;
            Accuracy = accuracy;
            Magic = magic;
            MDefense = mdefense;
            Time = time;
            HitPoints = 1;
            MaxHitPoints = 1;
            MagicPoints = 0;
            MaxMagicPoints = 0;
            ChronoForce = 0;
            MaxChronoForce = 0;
        }
    }

    #endregion

    /// <summary>
    /// Defines characters and monsters with customizable skills and spells based on
    /// a loaded file.  The player class will inherit from this with more abilities and
    /// the ability to level.
    /// </summary>
    public class CharacterBase
    {
        #region Constants
        protected const float cTimeScale = 0.25f;
        // Scales how fast to animate the sprites
        protected const float cSpeedScale = 0.07f;
        #endregion

        #region Fields
        Vector2 position;  // Position to draw the character

        // Sprite class for the character
        CharacterSprite sprite;
        // Character name
        string name;
        // Character ID number, used for Parties and scripts
        int id;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the character
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Sets/Gets the player ID used for position in a party and scripts
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The position where the character is rendered
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// The sprite representing the character
        /// </summary>
        public CharacterSprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        /// <summary>
        /// Changes which sprite to render with a string key
        /// </summary>
        public ActionString SpriteAction
        {
            get { return Sprite.Action; }
            set { Sprite.Action = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor loads default values into the attributes
        /// </summary>
        public CharacterBase()
        {
            name = "Unknonwn";
        }

        /// <summary>
        /// Constructor with default name and loads the provided sprite
        /// </summary>
        /// <param name="sprite">Sprite that represents the character</param>
        public CharacterBase(CharacterSprite sprite)
            : this()
        {
            this.sprite = sprite;
            position.X = 200;
            position.Y = 200;
        }

        /// <summary>
        /// Constructor that loads the specific name
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        public CharacterBase(string nameArg)
        {
            name = nameArg;
            id = -1;
        }

        /// <summary>
        /// Constructor that loads a specific name and sprite
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        /// <param name="sprite">Sprite that represents the character</param>
        public CharacterBase(string nameArg, CharacterSprite sprite)
        {
            name = nameArg;
            id = -1;
            this.sprite = sprite;
            position.X = 200;
            position.Y = 200;
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates the timer for the character and any time based restoration.
        /// </summary>
        /// <param name="elapsed">Time passed since last update in milliseconds</param>
        public virtual void Update(int elapsed)
        {
            // Update the sprite, whether in battle or on the overworld map
            sprite.Update(elapsed);
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Draws the character on the screen
        /// </summary>
        /// <param name="batch">Batch to use for drawing</param>
        /// <param name="color">Color tint of the character.  White is normal</param>
        /// <param name="blendMode">Any blendined required</param>
        public virtual void Draw(SpriteBatch batch, Color color, SpriteBlendMode blendMode)
        {
            // Passes it on to the internal sprite class for drawing
            sprite.Draw(batch, color, blendMode, position);
        }

        #endregion
    }
}