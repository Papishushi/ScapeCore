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
 * MerkleNode.cs
 * MerkleNode represents a node in a Merkle tree,
 * used for efficient verification of data integrity.
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