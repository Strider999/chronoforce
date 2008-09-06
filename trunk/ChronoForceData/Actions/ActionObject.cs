#region File Description
//-----------------------------------------------------------------------------
// ActionObject.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace ChronoForceData.Actions
{
    /// <summary>
    /// Enumeration for easier reading of the action object.  Represents and integer
    /// </summary>
    public enum ActionObjectType : int
    {
        Camera = 1,
        Character = 2,
        Effect = 3
    }

    /// <summary>
    /// Base class for any object manipulated by the ActionSlot.  The only purpose is to 
    /// label the object as to what type.  Characters, effect sprites, and cameras will be
    /// moveable objects, but not all can have the same type of action applied to it.
    /// </summary>
    public class ActionObject
    {
        #region Properties

        /// <summary>
        /// Name of the object
        /// </summary>
        private string name;

        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Type of object the action is applying to
        /// </summary>
        private ActionObjectType objectType;

        /// <summary>
        /// Type of object the action is applying to
        /// </summary>
        public ActionObjectType ObjectType
        {
            get { return objectType; }
            set { objectType = value; }
        }

        /// <summary>
        /// Position of the object
        /// </summary>
        protected Vector2 positionValue;

        /// <summary>
        /// Position of the object
        /// </summary>
        [ContentSerializer(Optional = true)]
        public virtual Vector2 Position
        {
            get { return positionValue; }
            set { positionValue = value; }
        }

        #endregion
    }
}
