using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Audio;

[StructLayout(LayoutKind.Sequential)]
public struct FNVID<T>  where T : struct
{
    private readonly static Dictionary<T, string> _names = [];

    public T _value;
    
    public FNVID(string name)
    {
        T hash = Hash(name);

        _value = hash;
        _names.TryAdd(hash, name);
    }

    public FNVID(T value = default)
    {
        _value = value;
        _names.TryAdd(value, "");
    }

    public readonly T Value => _value;
    public readonly string String => _names.TryGetValue(_value, out string? name) ? name : "";

    public override readonly int GetHashCode() => _value.GetHashCode();
    public override readonly string ToString() => !string.IsNullOrEmpty(String) ? String : _value.ToString() ?? "";

    public static FNVID<T> Read(BinaryReader reader)
    {
        T value = default;
        reader.Read(MemoryMarshal.AsBytes(new Span<T>(ref value)));
        return new(value);
    }
    public static bool TryMatch(string value, [NotNullWhen(true)] out FNVID<T>? match)
    {
        T hash = Hash(value);

        if (!_names.TryAdd(hash, value) && !string.IsNullOrEmpty(_names[hash]))
        {
            match = null;
            return false;
        }

        if (string.IsNullOrEmpty(_names[hash]))
        {
            _names[hash] = value;
        }

        match = hash;
        return true;
    }
    public static int Count() => _names.Count;
    public static void Clear() => _names.Clear();

    public static implicit operator T(FNVID<T> value) => value._value;
    public static implicit operator FNVID<T>(T value) => new(value);

    private static T Hash(string value) => Type.GetTypeCode(typeof(T)) switch
    {
        TypeCode.UInt32 or TypeCode.Int32 => (T)(object)FNV1.Compute32(value),
        TypeCode.UInt64 or TypeCode.Int64 => (T)(object)FNV1.Compute64(value),
        _ => throw new InvalidOperationException(nameof(T))
    };
}