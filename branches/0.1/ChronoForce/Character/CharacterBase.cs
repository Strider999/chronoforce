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
using ChronoForce.Base;
#endregion

namespace ChronoForce.Character
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
        const float cTimeScale = 0.25f;
        const float cBattleTimerScale = 0.02f;
        const float cSpeedScale = 0.07f;
        #endregion

        #region Fields

        #region Character Attributes
        
        // Character name
        string name;
        // Character ID number, used for Parties
        int id;
        // Basic character traits for both players and monsters
        CharacterStats stats;
        int Exp = 1;      // Total experience points
        int NextExp = 1;  // Experienced needed for next level
        StatusEffects status = StatusEffects.None; // Any status effects on the character
        float battleTimer = 0; // Timer for battle to determine when the character will act   
        Random rnd = new Random(); // Randomness
        Vector2 position;  // Position to draw the character for battle

        // Sprite class for the character
        CharacterSprite sprite;
        #endregion

        // Flag for whether or not the battle is going on (used to halt the timer for scripts)
        bool battleGoing = false;
        // Flag to signal that the timer event has fired so
        // it doesn't get called repeatedly
        bool timerCalled = false;

        #endregion

        #region Event Triggers

        // Timer trigger to tell the battle engine the character is ready
        public event EventHandler<NumberEventArgs> Ready;

        /// <summary>
        /// Method for raising the Ready event.
        /// </summary>
        protected internal virtual void OnReady(int playerID)
        {
            if (Ready != null)
                Ready(this, new NumberEventArgs(playerID));
        }

        #endregion

        #region Properties

        #region Character Stat Accessors

        /// <summary>
        /// Name of the character
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Level of the character
        /// </summary>
        public int LVL
        {
            get { return stats.Level; }
            set { stats.Level = value; }
        }

        /// <summary>
        /// Physical attack strength of character
        /// </summary>
        public int ATK
        {
            get { return stats.Attack; }
            set { stats.Attack = value; }
        }

        /// <summary>
        /// Physical defense of character
        /// </summary>
        public int DEF
        {
            get { return stats.Defense; }
            set { stats.Defense = value; }
        }

        /// <summary>
        /// How fast the character is
        /// </summary>
        public int SPD
        {
            get { return stats.Speed; }
            set { stats.Speed = value; }
        }

        /// <summary>
        /// How accurate the character is
        /// </summary>
        public int ACC
        {
            get { return stats.Accuracy; }
            set { stats.Accuracy = value; }
        }

        /// <summary>
        /// Magic strength of character
        /// </summary>
        public int MAG
        {
            get { return stats.Magic; }
        }

        /// <summary>
        /// Magic defense of character
        /// </summary>
        public int MDEF
        {
            get { return stats.MDefense; }
        }

        /// <summary>
        /// Chrono affinity of character
        /// </summary>
        public int TIME
        {
            get { return stats.Time; }
        }

        /// <summary>
        /// Experience points of character
        /// </summary>
        public int EXP
        {
            get { return Exp; }
            set { Exp = value; }
        }

        /// <summary>
        /// Experience points need to next level
        /// </summary>
        public int NextEXP
        {
            get { return NextExp; }
            set { NextExp = value; }
        }

        /// <summary>
        /// Current hit points character has
        /// </summary>
        public int HP
        {
            get { return stats.HitPoints; }
            set { stats.HitPoints = value; }
        }

        /// <summary>
        /// Maximum hit points the character can have
        /// </summary>
        public int MaxHP
        {
            get { return stats.MaxHitPoints; }
            set { stats.MaxHitPoints = value; }
        }

        /// <summary>
        /// Current magic points character has
        /// </summary>
        public int MP
        {
            get { return stats.MagicPoints; }
            set { stats.MagicPoints = value; }
        }

        /// <summary>
        /// Maximum magic points the character can have
        /// </summary>
        public int MaxMP
        {
            get { return stats.MaxMagicPoints; }
            set { stats.MaxMagicPoints = value; }
        }

        /// <summary>
        /// Current chrono force character has
        /// </summary>
        public int CF
        {
            get { return stats.ChronoForce; }
            set { stats.ChronoForce = value; }
        }
    
        /// <summary>
        /// Maximum chrono force the character can have
        /// </summary>
        public int MaxCF
        {
            get { return stats.MaxChronoForce; }
            set { stats.MaxChronoForce = value; }
        }

        /// <summary>
        /// Returns the percentage filled by the battle timer
        /// </summary>
        public float TimerPercent
        {
            get { return (battleTimer / 100); }
        }
        #endregion

        #region Status Effect Accessors

        /// <summary>
        /// Returns true if poisoned
        /// </summary>
        public bool Poisoned
        {
            get { return (status & StatusEffects.Poison) == StatusEffects.Poison; }
        }

        /// <summary>
        /// Returns true if character is sleeping
        /// </summary>
        public bool Sleeping
        {
            get { return (status & StatusEffects.Sleep) == StatusEffects.Sleep; }
        }

        /// <summary>
        /// Returns true if character is confused
        /// </summary>
        public bool Confused
        {
            get { return (status & StatusEffects.Confusion) == StatusEffects.Confusion; }
        }

        /// <summary>
        /// Returns true if character can't use skills
        /// </summary>
        public bool Locked
        {
            get { return (status & StatusEffects.Locked) == StatusEffects.Locked; }
        }

        /// <summary>
        /// Returns true if blind
        /// </summary>
        public bool Blind
        {
            get { return (status & StatusEffects.Blind) == StatusEffects.Blind; }
        }

        /// <summary>
        /// Returns true if character is stopped
        /// </summary>
        public bool Stopped
        {
            get { return (status & StatusEffects.Stopped) == StatusEffects.Stopped; }
        }

        /// <summary>
        /// Returns true if character is denied CF abilities
        /// </summary>
        public bool Shattered
        {
            get { return (status & StatusEffects.Shattered) == StatusEffects.Shattered; }
        }

        /// <summary>
        /// Returns true if the character has no HP
        /// </summary>
        public bool Dead
        {
            get { return (status & StatusEffects.Death) == StatusEffects.Death; }
        }

        #endregion

        #region Other Accessors

        /// <summary>
        /// Battle flag as to whether or not the battle started
        /// </summary>
        public bool BattleGoing
        {
            get { return battleGoing; }
            set { battleGoing = value; }
        }

        /// <summary>
        /// Sets/Gets the player ID used for position in a party.
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
        /// Sprite that represents the character
        /// </summary>
        public Texture2D Sprite
        {
            get { return sprite.CharacterTexture; }
        }

        /// <summary>
        /// Returns true if the character is in battle.  Used to determine
        /// which sprite to render on the screen.
        /// </summary>
        public bool InBattle
        {
            get { return sprite.InBattle; }
            set { sprite.InBattle = value; }
        }

        /// <summary>
        /// Changes which sprite to render with a string key
        /// </summary>
        public string SpriteType
        {
            get
            {
                if (sprite.InBattle)
                    return sprite.BattleType;
                else
                    return sprite.WorldType;
            }
            set
            {
                if (sprite.InBattle)
                    sprite.BattleType = value;
                else
                    sprite.WorldType = value;
            }
        }
        #endregion

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
        /// Constructor that loads the specific name, character stats.
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        /// <param name="stats">Starting stats for the character</param>
        public CharacterBase(string nameArg, CharacterStats stats)
            : this(nameArg)
        {
            this.stats = stats;
        }

        /// <summary>
        /// Constructor that loads the specific name, character stats, and ID in party
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        /// <param name="stats">Starting stats for the character</param>
        /// <param name="playerID">ID used for position in party</param>
        /// <param name="sprite">Textures that represent the character</param>
        public CharacterBase(string nameArg, CharacterStats stats, CharacterSprite sprite)
            : this(nameArg, stats)
        {
            this.sprite = sprite;
        }

        #endregion

        #region Battle Controls

        /// <summary>
        /// Set the battle timer in battle
        /// </summary>
        /// <param name="time">Starting time</param>
        public void SetTime(int time)
        {
            // Sets the time
            battleTimer = time;

            // Reset the flags
            timerCalled = false;
        }

        /// <summary>
        /// Starts the battle timer with a randomized time based on character speed
        /// </summary>
        public void InitializeTime()
        {
            // Randomize based on the percent of SPD
            battleTimer = (int)(SPD * rnd.NextDouble());
        }

        /// <summary>
        /// Calculates the attack damage from this character.  Classes that inherit should
        /// overwrite this function.
        /// </summary>
        /// <returns>The damage amount</returns>
        public virtual int PerformAttack() 
        {
            return 0;
        }

        /// <summary>
        /// Calculates the skill damage from this character.
        /// </summary>
        /// <param name="skillID">ID of the skill used</param>
        /// <returns>The damage amount</returns>
        public virtual int PerformSkill(int skillID)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the time strength from this character
        /// </summary>
        /// <param name="timeID">ID of the time skill used</param>
        /// <returns>The damage/healing amount</returns>
        public virtual int PerformTime(int timeID)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the anti-time strength from this character
        /// </summary>
        /// <param name="antiID">ID of anti-time skill used</param>
        /// <returns>The damage/healing amount</returns>
        public virtual int PerformAnti(int antiID)
        {
            return 0;
        }

        /// <summary>
        /// Uses the specified item.
        /// </summary>
        /// <param name="itemID">ID of the item used</param>
        /// <returns>Amount the item heals if applicable</returns>
        public virtual int UseItem(int itemID)
        {
            return 0;
        }

        /// <summary>
        /// Adds the status effect onto the character
        /// </summary>
        /// <param name="effect">Specified status effect to add</param>
        public void AddStatusEffect(StatusEffects effect)
        {
            status |= effect;
        }

        /// <summary>
        /// Removes a status effect from the character
        /// </summary>
        /// <param name="effect">Specified status effect to remove</param>
        public void RemoveStatusEffect(StatusEffects effect)
        {
            status &= ~effect;
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates the timer for the character and any time based restoration.
        /// </summary>
        /// <param name="elapsed">Time passed since last update in milliseconds</param>
        public void Update(int elapsed)
        {
            // If the timer is >=100, then the character is ready
            if (battleTimer >= 100)
            {
                if (!timerCalled)
                {
                    timerCalled = true;
                    if (Ready != null)
                        OnReady(id);
                }
            }
            else
            {
                if (battleGoing && !Stopped && !Sleeping)
                {
                    // If the character isn't sleeping or stopped, add to the battle timer
                    battleTimer += elapsed * SPD * cBattleTimerScale * cSpeedScale;

                    // Make sure the timer is within limits
                    Math.Max(battleTimer, 100);
                }
            }

            // Increase the amount of CF if not max already or not CF disabled
            // The amount added is determined by Time stat
            if (CF < MaxCF && !Stopped && !Shattered)
                CF += (int)(TIME * elapsed * cTimeScale);
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Draws the character on the screen
        /// </summary>
        /// <param name="batch">Batch to use for drawing</param>
        /// <param name="color">Color tint of the character.  White is normal</param>
        /// <param name="blendMode">Any blendined required</param>
        public void Draw(SpriteBatch batch, Color color, SpriteBlendMode blendMode)
        {
            // Passes it on to the internal sprite class for drawing
            sprite.Draw(batch, color, blendMode, position);
        }

        #endregion
    }
}