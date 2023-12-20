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
 * MerkleTree.cs
 * Represents a Merkle tree, a hash tree structure used for
 * efficient verification of data integrity.
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