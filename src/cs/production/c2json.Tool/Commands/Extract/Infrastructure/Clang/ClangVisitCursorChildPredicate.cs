// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using static bottlenoselabs.clang;

namespace c2json.Tool.Commands.Extract.Infrastructure.Clang;

public delegate bool ClangVisitCursorChildPredicate(CXCursor child, CXCursor parent);
