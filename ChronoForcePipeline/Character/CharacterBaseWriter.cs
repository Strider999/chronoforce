#region File Description
//-----------------------------------------------------------------------------
// CharacterBaseWriter.cs
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
using ChronoForceData.Actions;
using ChronoForceData.Character;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the CharacterBase data
    /// </summary>
    [ContentTypeWriter]
    public class CharacterBaseWriter : ChronoForceWriter<CharacterBase>
    {
        ActionObjectWriter actionObjWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            actionObjWriter = compiler.GetTypeWriter(typeof(ActionObject))
                as ActionObjectWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, CharacterBase value)
        {
            // Write the basic action object
            output.WriteRawObject<ActionObject>(value as ActionObject, actionObjWriter);

            // Write the object position
            output.WriteObject<Point>(value.MapPosition);

            output.WriteObject<AnimatingSprite>(value.Sprite);
        }
    }
}
