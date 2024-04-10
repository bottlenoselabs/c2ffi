// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Assertions;
using c2ffi.Tests.Library.Models;

namespace c2ffi.Tests.Library;

public static class CTestTypeAssertionExtensions
{
    public static CTestTypeAssertions Should(this CTestType? instance)
    {
        return new CTestTypeAssertions(instance);
    }
}
