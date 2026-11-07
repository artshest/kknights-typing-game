using System;
using System.Globalization;

namespace KK.Build.Editor
{
    // Только MAJOR.MINOR.PATCH: хэш коммита к версии дописывает сборка, в bundleVersion он не хранится.
    public readonly struct BuildVersion : IEquatable<BuildVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public BuildVersion(int major, int minor, int patch)
        {
            if (major < 0) throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor));
            if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch));

            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public static BuildVersion Parse(string text)
        {
            if (TryParse(text, out var version))
                return version;

            throw new FormatException($"Версия должна быть в формате X.Y.Z, получено «{text}».");
        }

        public static bool TryParse(string text, out BuildVersion version)
        {
            version = default;
            if (text == null)
                return false;

            var parts = text.Split('.');
            if (parts.Length != 3)
                return false;

            if (!TryParsePart(parts[0], out var major)
                || !TryParsePart(parts[1], out var minor)
                || !TryParsePart(parts[2], out var patch))
                return false;

            version = new BuildVersion(major, minor, patch);
            return true;
        }

        // SemVer запрещает ведущие нули: иначе «0.1.0» и «0.01.0» — одна версия под двумя git-тегами.
        private static bool TryParsePart(string part, out int value)
        {
            value = 0;
            if (part.Length == 0 || (part.Length > 1 && part[0] == '0'))
                return false;

            foreach (var c in part)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        public BuildVersion NextPatch() => new BuildVersion(Major, Minor, Patch + 1);

        public BuildVersion NextMinor() => new BuildVersion(Major, Minor + 1, 0);

        public BuildVersion NextMajor() => new BuildVersion(Major + 1, 0, 0);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", Major, Minor, Patch);

        public bool Equals(BuildVersion other) =>
            Major == other.Major && Minor == other.Minor && Patch == other.Patch;

        public override bool Equals(object obj) => obj is BuildVersion other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);

        public static bool operator ==(BuildVersion left, BuildVersion right) => left.Equals(right);

        public static bool operator !=(BuildVersion left, BuildVersion right) => !left.Equals(right);
    }
}
