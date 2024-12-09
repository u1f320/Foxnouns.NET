// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using NodaTime;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Foxnouns.Backend.Database;

[JsonConverter(typeof(JsonConverter))]
[TypeConverter(typeof(TypeConverter))]
public readonly struct Snowflake(ulong value) : IEquatable<Snowflake>
{
    public const long Epoch = 1_640_995_200_000; // 2022-01-01 at 00:00:00 UTC
    public readonly ulong Value = value;

    /// <summary>
    /// The time this snowflake was created.
    /// </summary>
    public Instant Time => Instant.FromUnixTimeMilliseconds(Timestamp);

    /// <summary>
    /// The Unix timestamp embedded in this snowflake, in milliseconds.
    /// </summary>
    public long Timestamp => (long)((Value >> 22) + Epoch);

    /// <summary>
    /// The process ID embedded in this snowflake.
    /// </summary>
    public byte ProcessId => (byte)((Value & 0x3E0000) >> 17);

    /// <summary>
    /// The thread ID embedded in this snowflake.
    /// </summary>
    public byte ThreadId => (byte)((Value & 0x1F000) >> 12);

    /// <summary>
    /// The increment embedded in this snowflake.
    /// </summary>
    public short Increment => (short)(Value & 0xFFF);

    public static bool operator <(Snowflake arg1, Snowflake arg2) => arg1.Value < arg2.Value;

    public static bool operator >(Snowflake arg1, Snowflake arg2) => arg1.Value > arg2.Value;

    public static bool operator ==(Snowflake arg1, Snowflake arg2) => arg1.Value == arg2.Value;

    public static bool operator !=(Snowflake arg1, Snowflake arg2) => arg1.Value != arg2.Value;

    public static implicit operator ulong(Snowflake s) => s.Value;

    public static implicit operator long(Snowflake s) => (long)s.Value;

    public static implicit operator Snowflake(ulong n) => new(n);

    public static implicit operator Snowflake(long n) => new((ulong)n);

    public static bool TryParse(string input, [NotNullWhen(true)] out Snowflake? snowflake)
    {
        snowflake = null;
        if (!ulong.TryParse(input, out ulong res))
            return false;
        snowflake = new Snowflake(res);
        return true;
    }

    public static Snowflake FromInstant(Instant instant) =>
        new((ulong)(instant.ToUnixTimeMilliseconds() - Epoch) << 22);

    public override bool Equals(object? obj) => obj is Snowflake other && Value == other.Value;

    public bool Equals(Snowflake other) => Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    /// <summary>
    /// An Entity Framework ValueConverter for Snowflakes to longs.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ValueConverter() : ValueConverter<Snowflake, long>(x => x, x => x);

    private class JsonConverter : JsonConverter<Snowflake>
    {
        public override void WriteJson(
            JsonWriter writer,
            Snowflake value,
            JsonSerializer serializer
        )
        {
            writer.WriteValue(value.Value.ToString());
        }

        public override Snowflake ReadJson(
            JsonReader reader,
            Type objectType,
            Snowflake existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        ) => ulong.Parse((string)reader.Value!);
    }

    private class TypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string);

        public override bool CanConvertTo(
            ITypeDescriptorContext? context,
            [NotNullWhen(true)] Type? destinationType
        ) => destinationType == typeof(Snowflake);

        public override object? ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value
        ) => TryParse((string)value, out Snowflake? snowflake) ? snowflake : null;
    }
}
