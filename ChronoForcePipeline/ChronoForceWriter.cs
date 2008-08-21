#region File Description
//-----------------------------------------------------------------------------
// ChronoForceWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace ChronoForcePipline
{
    /// <summary>
    /// Custom pipeline writer that automatically returns the correct type for the 
    /// reader.  Overloads GetRuntimeReader and GetRuntimeType to return the custom
    /// classes for reading.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ChronoForceWriter<T> : ContentTypeWriter<T>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            Type type = typeof(T);

            string readerText = type.FullName;
            string shortTypeName = type.Name;
            if (shortTypeName.EndsWith("`1"))
            {
                // build the name of a templated type
                shortTypeName = shortTypeName.Substring(0, shortTypeName.Length - 2);
                readerText = readerText.Insert(readerText.IndexOf("`1") + 2, "+" +
                    shortTypeName + "Reader");
            }
            else
            {
                // build the name of a non-templated type
                readerText += "+" + shortTypeName + "Reader";
            }
            readerText += ", ChronoForce";

            // replace the suffix name on the Xbox 360
            // -- since the processor runs on Windows, it needs to reference 
            //    RolePlayingGameDataWindows.  However, this means that type.FullName
            //    will specify RolePlayingGameWindows in the interior type of templates
            // NOTE:  Note using this for now.  Will have to change this later when I finish
            // the Windows version and can import to Xbox
            //if (targetPlatform == TargetPlatform.Xbox360)
            //{
            //    readerText = readerText.Replace("Windows", "Xbox");
            //}

            System.Diagnostics.Debug.WriteLine("Reader:  " + readerText);

            return readerText;
        }


        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            Type type = typeof(T);

            string typeText = type.FullName + ", RolePlayingGameDataWindows";

            // replace the suffix name on the Xbox 360
            // -- since the processor runs on Windows, it needs to reference 
            //    RolePlayingGameDataWindows.  However, this means that type.FullName
            //    will specify RolePlayingGameWindows in the interior type of templates
            // NOTE:  Note using this for now.  Will have to change this later when I finish
            // the Windows version and can import to Xbox
            //if (targetPlatform == TargetPlatform.Xbox360)
            //{
            //    typeText = typeText.Replace("Windows", "Xbox");
            //}

            System.Diagnostics.Debug.WriteLine("Type:  " + typeText);

            return typeText;
        }
    }
}
