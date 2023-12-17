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
using System.Collections.Generic;
using System.Linq;

namespace ScapeCore.Core.Collections.Merkle
{
    public readonly struct MerkleTree<T>
    {
        private readonly byte[] result;
        public readonly IReadOnlyList<IEnumerable<MerkleNode<T>>> Children;

        public MerkleTree(params T[] elements)
        {
            IEnumerable<MerkleNode<T>> initialConfiguration = Array.Empty<MerkleNode<T>>();
            List<IEnumerable<MerkleNode<T>>> nodes = new();

            if (elements.Length == 1) initialConfiguration = initialConfiguration.Append(new(elements[0]));
            for (var i = 0; i < elements.Length - 1; i += 2)
            {
                initialConfiguration = initialConfiguration.Append(new(elements[i], elements[i + 1]));
                if (i == elements.Length - 3) initialConfiguration = initialConfiguration.Append(new(elements[i + 2]));
            }
            while (initialConfiguration.Count() != 1)
            {
                IEnumerable<MerkleNode<T>> tempLayer = Array.Empty<MerkleNode<T>>();
                nodes.Add(initialConfiguration);
                for (var i = 0; i < initialConfiguration.Count(); i += 2)
                {
                    tempLayer = tempLayer.Append(new(initialConfiguration.ElementAt(i), initialConfiguration.ElementAt(i + 1)));
                    if (i == initialConfiguration.Count() - 3) tempLayer = tempLayer.Append(initialConfiguration.ElementAt(i + 2));
                }
                initialConfiguration = tempLayer;
            }
            nodes.Reverse();
            Children = nodes;
            result = initialConfiguration.FirstOrDefault();
        }

        public static implicit operator byte[](MerkleTree<T> tree) => tree.result;
        public static implicit operator int(MerkleTree<T> tree) => BitConverter.ToInt32((byte[])tree);
        public static implicit operator string(MerkleTree<T> tree) => ((int)tree).ToString("x");

        public override bool Equals(object? obj) => obj is MerkleTree<T> tree && result == tree.result;
        public override int GetHashCode() => BitConverter.ToInt32(result);
        public override string ToString() => GetHashCode().ToString("x");

        public static bool operator ==(MerkleTree<T> left, MerkleTree<T> right) => left.Equals(right);
        public static bool operator !=(MerkleTree<T> left, MerkleTree<T> right) => !(left.Equals(right));
    }
}