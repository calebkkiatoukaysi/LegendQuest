using System;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The fight's own dice: a PCG32 generator (O'Neill's permuted congruential generator, the 64-bit
/// state, 32-bit output XSH RR variant), seeded once, so a fight plays the same from the same seed on
/// any machine and any version of .NET.
/// </summary>
/// <remarks>
/// It is a Random so the rules' Dice, DiceExpression and AttackMath can roll with it as they are, but
/// every method a Random has is overridden here and none of them reach the base class's generator:
/// System.Random's own sequence isn't promised to stay the same between .NET versions, and this one
/// is. Ranges are drawn by rejection, so no face of a die comes up more often than another.
/// </remarks>
public sealed class Pcg32 : Random
{
    /// <summary>The multiplier of the congruential step, PCG's default.</summary>
    private const ulong Multiplier = 6364136223846793005UL;

    /// <summary>The stream increment: any odd number; this is PCG's default stream.</summary>
    private const ulong Increment = 1442695040888963407UL;

    private ulong _state;

    /// <summary>Seeds it, the way the reference implementation does: step, add the seed, step.</summary>
    /// <param name="seed">The seed.</param>
    public Pcg32(ulong seed)
    {
        _state = 0;
        NextUInt();
        _state += seed;
        NextUInt();
    }

    /// <summary>How many numbers it has drawn, for the simulator's sanity checks.</summary>
    public long Draws { get; private set; }

    /// <summary>The next 32 random bits.</summary>
    public uint NextUInt()
    {
        ulong old = _state;
        _state = unchecked(old * Multiplier + Increment);
        uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
        int rotate = (int)(old >> 59);
        Draws++;
        return (xorShifted >> rotate) | (xorShifted << (-rotate & 31));
    }

    /// <summary>A number from 0 to bound - 1, every one equally likely.</summary>
    /// <param name="bound">One more than the highest it can be; at least 1.</param>
    public uint Below(uint bound)
    {
        if (bound <= 1) return 0;
        uint threshold = (uint)(-bound % bound);
        while (true)
        {
            uint draw = NextUInt();
            if (draw >= threshold) return draw % bound;
        }
    }

    /// <summary>Whether something with this chance in a hundred happens.</summary>
    /// <param name="percent">The chance, 0 to 100.</param>
    public bool Percent(double percent) => percent > 0 && (percent >= 100 || NextDouble() * 100.0 < percent);

    /// <inheritdoc/>
    public override int Next() => (int)(NextUInt() >> 1);

    /// <inheritdoc/>
    public override int Next(int maxValue)
    {
        if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue));
        return (int)Below((uint)maxValue);
    }

    /// <inheritdoc/>
    public override int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
        long range = (long)maxValue - minValue;
        return range <= uint.MaxValue ? (int)(minValue + Below((uint)range)) : (int)(minValue + (long)(NextDouble() * range));
    }

    /// <inheritdoc/>
    public override long NextInt64() => (long)((((ulong)NextUInt() << 32) | NextUInt()) >> 1);

    /// <inheritdoc/>
    public override long NextInt64(long maxValue) => NextInt64(0, maxValue);

    /// <inheritdoc/>
    public override long NextInt64(long minValue, long maxValue)
    {
        if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
        ulong range = (ulong)(maxValue - minValue);
        if (range == 0) return minValue;
        ulong limit = ulong.MaxValue - ulong.MaxValue % range;
        while (true)
        {
            ulong draw = ((ulong)NextUInt() << 32) | NextUInt();
            if (draw < limit) return minValue + (long)(draw % range);
        }
    }

    /// <inheritdoc/>
    public override double NextDouble() => ((((ulong)NextUInt() << 32) | NextUInt()) >> 11) * (1.0 / (1UL << 53));

    /// <inheritdoc/>
    public override float NextSingle() => (NextUInt() >> 8) * (1f / (1 << 24));

    /// <inheritdoc/>
    public override void NextBytes(byte[] buffer) => NextBytes(buffer.AsSpan());

    /// <inheritdoc/>
    public override void NextBytes(Span<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i++) buffer[i] = (byte)NextUInt();
    }

    /// <inheritdoc/>
    protected override double Sample() => NextDouble();

    /// <summary>A seed for one run of something, mixed from a base seed and a few numbers (the run, a fight's hash), so neighbouring runs aren't neighbouring seeds.</summary>
    /// <param name="seed">The base seed.</param>
    /// <param name="parts">What to mix in.</param>
    public static ulong Mix(ulong seed, params ulong[] parts)
    {
        ulong mixed = seed;
        foreach (ulong part in parts)
        {
            mixed ^= part + 0x9E3779B97F4A7C15UL + (mixed << 6) + (mixed >> 2);
            mixed = SplitMix(mixed);
        }
        return mixed;
    }

    /// <summary>A stable hash of text (FNV-1a, 64 bits), for seeding by a fight's id: string.GetHashCode changes from run to run.</summary>
    /// <param name="text">The text.</param>
    public static ulong Hash(string text)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char c in text ?? string.Empty)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        return hash;
    }

    /// <summary>SplitMix64's finaliser, which spreads the bits of a number about.</summary>
    private static ulong SplitMix(ulong x)
    {
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        return x ^ (x >> 31);
    }
}
