// This license notice applies to the code in UnsafePermissionService#matchWildcards,
// located in this file. The code is mostly identical to the code located here:
// https://github.com/dotnet/runtime/blob/6ca8c9bc0c4a5fc1082c690b6768ab3be8761b11/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemName.cs#L147
// The original code has been deemed unsuitable for InsanityBot's use, and therefore
// adapted here.
//
// Copyright (c) 2018 - present, .NET Foundation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace InsanityBot.Extensions.Permissions.Unsafe;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class UnsafePermissionService
{
    // one thing to note about the use of Memory<Char> and .Span calls here: these are purely on the stack.
    // there are no heap allocations incurred there: in fact matchWildcards is allocation-free, and resolveWildcards
    // only allocates once for its final result.
    private readonly Memory<Char> __wildcards;
    private readonly Memory<Char> __tolerate_anything_pattern;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IEnumerable<String> resolveWildcards(String input)
    {
        ReadOnlySpan<Char> expression = input.AsSpan();
        Int32 firstWildcardIndex = expression.IndexOfAny(__wildcards.Span);

        // special case: no wildcards were passed (which is in practice the most common case)
        // only one permission can therefore match, if any.
        // since this is the unsafe engine we're free to just... hope whoever gave us that pattern knew what they
        // were doing. no validity checking.
        if(firstWildcardIndex < 0)
        {
            return new String[] { input };
        }

        // the additional input.AsSpan call does not actually cause overhead.
        // the first .Where call serves as a filter for the relatively expensive matchWildcards call below.
        return this.__permissions
            .AsParallel()
            .Where(xm => xm.AsSpan()[..firstWildcardIndex] == input.AsSpan()[..firstWildcardIndex])
            .Where(xm => this.matchWildcards(input.AsSpan()[firstWildcardIndex..], xm.AsSpan()[firstWildcardIndex..]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Boolean matchWildcards(ReadOnlySpan<Char> expression, ReadOnlySpan<Char> permission)
    {
        // if nothing is specified, return true.
        if(expression.Length == 0)
        {
            return true;
        }

        // if the expression is set up to tolerate anything, return true
        // this optimization specifically saves a lot of time in conjunction to how resolveWildcards truncates
        // both the expression and the permissions to be matched by anything before the first wildcard
        if(expression.SequenceEqual(__tolerate_anything_pattern.Span))
        {
            return true;
        }

        // at this point we know that the expression length is greater than zero, and also that the expression
        // won't just match everything. therefore, at least one character is required for a successful match.
        if(permission.Length == 0)
        {
            return false;
        }

        // catch the expression starting with * and containing nothing else, which is effectively an EndsWith call
        if(expression[0] == '*')
        {
            ReadOnlySpan<Char> end = expression[1..];

            if(end.IndexOfAny(this.__wildcards.Span) < 0)
            {
                // the permission is shorter than the expression end, no need for an equality check
                if(permission.Length < end.Length)
                {
                    return false;
                }

                return permission.EndsWith(end, StringComparison.OrdinalIgnoreCase);
            }
        }

        Int32 expressionOffset, offset = 0;
        Int32 priorMatch, currentMatch, priorMatchCount, matchCount = 1;
        Char expressionChar, permissionChar = '\0';

        Span<Int32> temp = stackalloc Int32[0];
        Span<Int32> currentMatches = stackalloc Int32[16];
        Span<Int32> priorMatches = stackalloc Int32[16];
        priorMatches[0] = 0;

        Int32 currentState, maxState = expression.Length * 2;
        Boolean finished = false;

        while(!finished)
        {
            if(offset < permission.Length)
            {
                permissionChar = permission[offset++];
            }
            else
            {
                if(priorMatches[matchCount - 1] == maxState)
                {
                    break;
                }

                finished = true;
            }

            priorMatch = 0;
            currentMatch = 0;
            priorMatchCount = 0;

            while(priorMatch < matchCount)
            {
                expressionOffset = (priorMatches[priorMatch++] + 1) / 2;
                while(expressionOffset < expression.Length)
                {
                    currentState = expressionOffset * 2;
                    expressionChar = expression[expressionOffset];

                    if(currentMatch >= currentMatches.Length - 2)
                    {
                        Int32 newSize = currentMatches.Length * 2;
                        temp = new Int32[newSize];
                        currentMatches.CopyTo(temp);
                        currentMatches = temp;

                        temp = new Int32[newSize];
                        priorMatches.CopyTo(temp);
                        priorMatches = temp;
                    }

                    if(expressionChar == '*')
                    {
                        goto MatchZeroOrMore;
                    }
                    else
                    {
                        if(expressionChar == '\\')
                        {
                            if(++expressionOffset == expression.Length)
                            {
                                currentMatches[currentMatch++] = maxState;
                                goto ExpressionFinished;
                            }

                            currentState = expressionOffset * 2 + 2;
                            expressionChar = expression[expressionOffset];
                        }

                        if(finished)
                        {
                            goto ExpressionFinished;
                        }

                        if(expressionChar == '?')
                        {
                            currentMatches[currentMatch++] = currentState;
                        }
                        else if(expressionChar == permissionChar)
                        {
                            currentMatches[currentMatch++] = currentState;
                        }
                    }

                MatchZeroOrMore:
                    currentMatches[currentMatch++] = currentState;
                } // while(expressionOffset < expression.Length)

            ExpressionFinished:
                if((priorMatch < matchCount) && (priorMatchCount < currentMatch))
                {
                    while(priorMatchCount < currentMatch)
                    {
                        Int32 previousLength = priorMatches.Length;
                        while((priorMatch < previousLength) && (priorMatches[priorMatch] < currentMatches[priorMatchCount]))
                        {
                            priorMatch++;
                        }
                        priorMatchCount++;
                    }
                }
            } // while(sourceCount < matchesCount)

            if(currentMatch == 0)
            {
                return false;
            }

            temp = priorMatches;
            priorMatches = currentMatches;
            currentMatches = temp;

            matchCount = currentMatch;
        } // while(!finished)

        currentState = priorMatches[matchCount - 1];
        return currentState == maxState;
    }
}
