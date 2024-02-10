#!/bin/bash

{
  echo '<BuildinTypes Version="1.1.0.1" xmlns="http://schemas.microsoft.com/powershell/2004/04">';
  find . -type f -name '*.cs' -print0 \
    | xargs -0 grep -h -P -o '(?<=public class\s|public interface\s|public virtual\s|public abstract class\s|public sealed class\s|public record struct\s|public readonly struct\s|public readonly record struct\s|public readonly record class\s|public readonly class\s|public record class\s|public enum\s|public struct\s)\w+' \
    | sed 's/^/\t<Type>/;s/$/<\/Type>/';
  echo '</BuildinTypes>';
} > BuildinTypes.xml
