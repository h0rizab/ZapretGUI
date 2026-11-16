using System.IO;
namespace ZapretGui.Models
{
    public sealed record ProfileInfo(string Name, string FullPath);

    public sealed record DependencyResult(bool Ok, string Message);
}

