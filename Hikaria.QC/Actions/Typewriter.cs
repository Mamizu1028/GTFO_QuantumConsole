using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hikaria.QC.Actions
{
    /// <summary>
    /// Gradually types a message to the console.
    /// </summary>
    public class Typewriter : Composite
    {
        /// <summary>
        /// Configuration for the Typewriter action.
        /// </summary>
        public struct Config
        {
            public enum ChunkType
            {
                Character,
                Word,
                Line
            }

            public float PrintInterval;
            public ChunkType Chunks;

            public static readonly Config Default = new Config
            {
                PrintInterval = 0f,
                Chunks = ChunkType.Character
            };
        }

        private static readonly Regex WhiteRegex = new Regex(@"(?<=[\s+])", RegexOptions.Compiled);
        private static readonly Regex LineRegex = new Regex(@"(?<=[\n+])", RegexOptions.Compiled);

        /// <param name="message">The message to display to the console.</param>
        public Typewriter(string message)
            : this(message, Config.Default)
        { }

        /// <param name="message">The message to display to the console.</param>
        /// <param name="config">The configuration to be used.</param>
        public Typewriter(string message, Config config)
            : base(Generate(message, config))
        { }

        private static IEnumerator<ICommandAction> Generate(string message, Config config)
        {
            QuantumConsole console = null;
            LogHandle streamId = LogHandle.Invalid;
            bool completed = false;
            string[] chunks;
            switch (config.Chunks)
            {
                case Config.ChunkType.Character: chunks = message.Select(c => c.ToString()).ToArray(); break;
                case Config.ChunkType.Word: chunks = WhiteRegex.Split(message); break;
                case Config.ChunkType.Line: chunks = LineRegex.Split(message); break;
                default: throw new ArgumentException($"Chunk type {config.Chunks} is not supported.");
            }

            yield return new GetContext(ctx => console = ctx.Console);

            try
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    yield return new WaitRealtime(config.PrintInterval);

                    if (i == 0)
                        streamId = console.BeginStream(chunks[i]);
                    else
                        console.AppendLog(streamId, chunks[i]);
                }

                console.CompleteLog(streamId);
                completed = true;
            }
            finally
            {
                if (!completed && console != null && streamId.IsValid)
                    console.RemoveLog(streamId);
            }
        }
    }
}
