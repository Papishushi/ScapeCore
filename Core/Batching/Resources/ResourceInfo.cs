using System;

namespace ScapeCore.Core.Batching.Resources
{
    public record struct ResourceInfo(string ResourceName, Type TargetType);
}