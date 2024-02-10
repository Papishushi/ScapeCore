$xmlContent = @'
<BuildinTypes Version="1.1.0.1" xmlns="http://schemas.microsoft.com/powershell/2004/04">
'@ + "`n"

Get-ChildItem -Recurse -Filter '*.cs' | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $matches = [Regex]::Matches($content, '(?<=public class\s|public interface\s|public virtual\s|public abstract class\s|public sealed class\s|public record struct\s|public readonly struct\s|public readonly record struct\s|public readonly record class\s|public readonly class\s|public record class\s|public enum\s|public struct\s)\w+')

    foreach ($match in $matches) {
        $xmlContent += "`t<Type>$($match.Value)</Type>`n"
    }
}

$xmlContent += '</BuildinTypes>'
$xmlContent | Out-File -FilePath 'BuildinTypes.xml'
