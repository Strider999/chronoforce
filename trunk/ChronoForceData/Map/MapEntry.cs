#region File Description
//-----------------------------------------------------------------------------
// MapEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ChronoForceData.Base;
#endregion

namespace ChronoForceData.Map
{
    /// <summary>
    /// The description of where an instance of a world object is in the world.
    /// </summary>
    public class MapEntry<T> where T : ContentObject
    {
        #region Map Data


        /// <summary>
        /// The position of this object on the map.
        /// </summary>
        private Point mapPosition;

        /// <summary>
        /// The position of this object on the map.
        /// </summary>
        public Point MapPosition
        {
            get { return mapPosition; }
            set { mapPosition = value; }
        }


        /// <summary>
        /// The orientation of this object on the map.
        /// </summary>
        private MapDirection direction;

        /// <summary>
        /// The orientation of this object on the map.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public MapDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }


        #endregion

        #region Content Data

        /// <summary>
        /// The content referred to by this entry.
        /// </summary>
        /// <remarks>
        /// This will not be automatically loaded, as the content path may be incomplete.
        /// </remarks>
        private T content;

        /// <summary>
        /// The content referred to by this entry.
        /// </summary>
        /// <remarks>
        /// This will not be automatically loaded, as the content path may be incomplete.
        /// </remarks>
        [ContentSerializerIgnore]
        [XmlIgnore]
        public T Content
        {
            get { return content; }
            set { content = value; }
        }

        #endregion

        #region Object Implementation


        /// <summary>
        /// Tests for equality between reference objects.
        /// </summary>
        /// <remarks>
        /// Implemented so that player-removed map entries from save games can be 
        /// compared to the data-driven map entries.
        /// </remarks>
        /// <returns>True if "equal".</returns>
        public override bool Equals(object obj)
        {
            MapEntry<T> mapEntry = obj as MapEntry<T>;
            return ((mapEntry != null) &&
                (mapEntry.Content == Content) &&
                (mapEntry.mapPosition == mapPosition) &&
                (mapEntry.Direction == Direction));
        }


        /// <summary>
        /// Calculates the hash code for comparisons with this reference type.
        /// </summary>
        /// <remarks>Recommended specified overload when Equals is overridden.</remarks>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #endregion

        #region Content Type Reader


        /// <summary>
        /// Read a MapEntry object from the content pipeline.
        /// </summary>
        public class MapEntryReader : ContentTypeReader<MapEntry<T>>
        {
            /// <summary>
            /// Read a MapEntry object from the content pipeline.
            /// </summary>
            protected override MapEntry<T> Read(ContentReader input,
                MapEntry<T> existingInstance)
            {
                MapEntry<T> desc = existingInstance;
                if (desc == null)
                {
                    desc = new MapEntry<T>();
                }

                desc.MapPosition = input.ReadObject<Point>();
                desc.Direction = (MapDirection)input.ReadInt32();

                return desc;
            }
        }


        #endregion
    }
}
