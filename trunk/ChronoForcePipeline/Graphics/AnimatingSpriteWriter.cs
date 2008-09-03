#region File Description
//-----------------------------------------------------------------------------
// CharacterSpriteWriter.cs
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
using ChronoForceData.Graphics;
#endregion

namespace ChronoForcePipeline.Graphics
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the CharacterSprite data
    /// </summary>
    [ContentTypeWriter]
    public class AnimatingSpriteWriter : ChronoForceWriter<AnimatingSprite>
    {
        protected override void Write(ContentWriter output, AnimatingSprite value)
        {
            output.Write(value.TextureName);
            output.Write(value.TextureType);
            output.WriteObject<Point>(value.FrameDimension);
            output.Write(value.FramesPerRow);
            output.Write(value.Padding);

            // Optional values for sprite type, motion, and direction
            output.Write(value.Type);
            output.Write(value.Motion);
            output.Write(value.Direction);

            // Dictionary of all the animations
            output.WriteObject<Dictionary<string, Animation>>(value.Sprites);
        }
    }
}
