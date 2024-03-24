// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Parse;

public class MacroObjectCandidate
{
    public string Name { get; init; } = string.Empty;

    public CLocation? Location { get; init; }

    public ImmutableArray<string> Tokens { get; init; } = ImmutableArray<string>.Empty;

    public static MacroObjectCandidate? New(clang.CXCursor clangCursor)
    {
        var name = clangCursor.Spelling();
        var location = clangCursor.Location();

        // clang doesn't have a thing where we can easily get a value of a macro
        // we need to:
        //  1. get the text range of the cursor
        //  2. get the tokens over said text range
        //  3. go through the tokens to parse the value
        // this means we get to do token parsing ourselves
        // NOTE: The first token will always be the name of the macro
        var translationUnit = clang.clang_Cursor_getTranslationUnit(clangCursor);
        string[] tokens;
        unsafe
        {
            var range = clang.clang_getCursorExtent(clangCursor);
            var tokensC = (clang.CXToken*)0;
            uint tokensCount = 0;

            clang.clang_tokenize(translationUnit, range, &tokensC, &tokensCount);

            var isFlag = tokensCount is 0 or 1;
            if (isFlag)
            {
                clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
                return null;
            }

            tokens = new string[tokensCount - 1];
            for (var i = 1; i < (int)tokensCount; i++)
            {
                var tokenString = clang.clang_getTokenSpelling(translationUnit, tokensC[i]).String();

                // CLANG BUG?: https://github.com/FNA-XNA/FAudio/blob/b84599a5e6d7811b02329709a166a337de158c5e/include/FAPOBase.h#L90
                if (tokenString.StartsWith('\\'))
                {
                    tokenString = tokenString.TrimStart('\\');
                }

                if (tokenString.StartsWith("__", StringComparison.InvariantCulture) &&
                    tokenString.EndsWith("__", StringComparison.InvariantCulture))
                {
                    clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
                    return null;
                }

                tokens[i - 1] = tokenString.Trim();
            }

            clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
        }

        var result = new MacroObjectCandidate
        {
            Name = name,
            Tokens = tokens.ToImmutableArray(),
            Location = location
        };

        return result;
    }
}
