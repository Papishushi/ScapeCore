/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * Copyright (c) 2023 Daniel Molinero Lucas
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 * 
 * GZipStreamer.cs
 * Provides compression and decompression utilities to classes 
 * inheriting this class.
 */

using MonoGame.Framework.Utilities.Deflate;
using System.IO;


namespace ScapeCore.Core.Serialization.Streamers
{
    public abstract class GZipStreamer
    {
        protected readonly int _size;

        protected GZipStreamer(int size) => _size = size;

        public virtual byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    using (var bs = new BufferedStream(zipStream, _size))
                    {
                        bs.Write(data, 0, data.Length);
                        bs.Close();
                        zipStream.Close();
                        return compressedStream.ToArray();
                    }
                }
            }
        }

        public virtual byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var bs = new BufferedStream(zipStream, _size))
                    {
                        using (var resultStream = new MemoryStream())
                        {
                            bs.CopyTo(resultStream);
                            bs.Close();
                            zipStream.Close();
                            return resultStream.ToArray();
                        }
                    }
                }
            }
        }
    }
}