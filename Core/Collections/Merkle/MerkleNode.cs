/*
 * -*- encoding: utf-8 with BOM -*-
 * .▄▄ ·  ▄▄·  ▄▄▄·  ▄▄▄·▄▄▄ .     ▄▄·       ▄▄▄  ▄▄▄ .
 * ▐█ ▀. ▐█ ▌▪▐█ ▀█ ▐█ ▄█▀▄.▀·    ▐█ ▌▪▪     ▀▄ █·▀▄.▀·
 * ▄▀▀▀█▄██ ▄▄▄█▀▀█  ██▀·▐▀▀▪▄    ██ ▄▄ ▄█▀▄ ▐▀▀▄ ▐▀▀▪▄
 * ▐█▄▪▐█▐███▌▐█ ▪▐▌▐█▪·•▐█▄▄▌    ▐███▌▐█▌.▐▌▐█•█▌▐█▄▄▌
 *  ▀▀▀▀ ·▀▀▀  ▀  ▀ .▀    ▀▀▀     ·▀▀▀  ▀█▄▀▪.▀  ▀ ▀▀▀ 
 * https://github.com/Papishushi/ScapeCore
 * 
 * MIT License
 *
 * Copyright (c) 2023 Daniel Molinero Lucas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Security.Cryptography;

namespace ScapeCore.Core.Collections.Merkle
{
    public readonly struct MerkleNode<T>
    {
        private readonly byte[] result;

        public MerkleNode(T unique)
        {
            result = BitConverter.GetBytes(unique.GetHashCode());
        }
        public MerkleNode(MerkleNode<T> merkleNode) : this()
        {
            result = merkleNode;
        }
        public MerkleNode(T left, T right)
        {
            var hashGen = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            hashGen.AppendData(BitConverter.GetBytes(left.GetHashCode()));
            hashGen.AppendData(BitConverter.GetBytes(right.GetHashCode()));

            result = hashGen.GetCurrentHash();
        }
        public MerkleNode(MerkleNode<T> left, MerkleNode<T> right) : this()
        {
            var hashGen = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            hashGen.AppendData(left);
            hashGen.AppendData(right);

            result = hashGen.GetCurrentHash();
        }

        public static explicit operator MerkleNode<T>(T input) => new(input);

        public static implicit operator byte[](MerkleNode<T> node) => node.result;
        public static implicit operator int(MerkleNode<T> node) => BitConverter.ToInt32((byte[])node);
        public static implicit operator string(MerkleNode<T> node) => ((int)node).ToString("x");

        public override bool Equals(object? obj) => obj is MerkleNode<T> node && result == (byte[])node;
        public override int GetHashCode() => BitConverter.ToInt32(result);
        public override string ToString() => GetHashCode().ToString("x");

        public static bool operator ==(MerkleNode<T> left, MerkleNode<T> right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(MerkleNode<T> left, MerkleNode<T> right)
        {
            return !(left==right);
        }
    }
}