using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace TQVaultAE.Domain.Helpers;

/// <summary>
/// Provides optimized span-based string operations to reduce allocations.
/// </summary>
public static class SpanHelper
{
	/// <summary>
	/// Gets an uppercase ReadOnlySpan of the string without allocation.
	/// Uses pooled array for the conversion.
	/// </summary>
	/// <param name="str">The string to convert to uppercase span.</param>
	/// <returns>A pooled char array containing the uppercase string.</returns>
	public static PooledCharArray ToUpperSpan(string str)
	{
		if (string.IsNullOrEmpty(str))
			return PooledCharArray.Empty;

		// Use pooled array for conversion
		var pooled = PooledCharArray.Rent(str.Length);

		// Copy and convert to uppercase in one pass
		for (int i = 0; i < str.Length; i++)
			pooled.Span[i] = char.ToUpperInvariant(str[i]);

		return pooled;
	}

	/// <summary>
	/// Checks if the string starts with the specified prefix (case-insensitive).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWithIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> prefix)
	{
		if (span.Length < prefix.Length)
			return false;

		for (int i = 0; i < prefix.Length; i++)
		{
			if (char.ToUpperInvariant(span[i]) != char.ToUpperInvariant(prefix[i]))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Checks if two spans are equal (case-insensitive).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EqualsIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
	{
		if (span.Length != other.Length)
			return false;

		for (int i = 0; i < span.Length; i++)
		{
			if (char.ToUpperInvariant(span[i]) != char.ToUpperInvariant(other[i]))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Creates a new string by concatenating a prefix with a sliced portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars)
	{
		int suffixLength = span.Length - skipPrefixChars;
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}

	/// <summary>
	/// Creates a new string by concatenating a prefix with a middle portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars, int suffixLength)
	{
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars, suffixLength).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}

	/// <summary>
	/// Creates a new string by concatenating two spans.
	/// </summary>
	public static string ConcatSpans(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
	{
		var result = new char[first.Length + second.Length];
		first.CopyTo(result);
		second.CopyTo(result.AsSpan(first.Length));
		return new string(result);
	}
}

/// <summary>
/// Represents a pooled char array that should be returned to the pool when disposed.
/// </summary>
public sealed class PooledCharArray : IDisposable
{
	private char[]? _array;
	private int _length;

	/// <summary>
	/// Gets the span of the pooled array.
	/// </summary>
	public Span<char> Span => _array is null ? Span<char>.Empty : _array.AsSpan(0, _length);

	/// <summary>
	/// Gets the length of the data in the pooled array.
	/// </summary>
	public int Length => _length;

	/// <summary>
	/// An empty pooled array instance.
	/// </summary>
	public static PooledCharArray Empty { get; } = new(Array.Empty<char>(), 0);

	private PooledCharArray(char[] array, int length)
	{
		_array = array;
		_length = length;
	}

	/// <summary>
	/// Creates a pooled array of the specified length.
	/// </summary>
	public static PooledCharArray Rent(int length)
	{
		if (length <= 0)
			return Empty;

		return new PooledCharArray(ArrayPool<char>.Shared.Rent(length), length);
	}

	/// <summary>
	/// Creates a pooled array from an existing string.
	/// </summary>
	public static PooledCharArray FromString(string? str)
	{
		if (string.IsNullOrEmpty(str))
			return Empty;

		var pooled = Rent(str.Length);
		str.CopyTo(pooled.Span);
		return pooled;
	}

	/// <summary>
	/// Returns the pooled array to the pool.
	/// </summary>
	public void Dispose()
	{
		if (_array is not null)
		{
			ArrayPool<char>.Shared.Return(_array);
			_array = null;
			_length = 0;
		}
	}
}
