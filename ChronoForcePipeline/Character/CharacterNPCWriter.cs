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
using ChronoForceData.Character;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the CharacterNPC data
    /// </summary>
    [ContentTypeWriter]
    public class CharacterNPCWriter : ChronoForceWriter<CharacterNPC>
    {
        CharacterBaseWriter charBaseWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            charBaseWriter = compiler.GetTypeWriter(typeof(CharacterBase))
                as CharacterBaseWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, CharacterNPC value)
        {
            // Write the basic character object
            output.WriteRawObject<CharacterBase>(value as CharacterBase, charBaseWriter);

            // Write the NPC specific information
            output.Write(value.Moves);
            output.Write(value.MaxRadius);
            output.WriteObject<List<string>>(value.DialogText);
            output.WriteObject<List<Point>>(value.RestrictedPositions);
            output.Write(value.IsMerchant);
        }
    }
}
