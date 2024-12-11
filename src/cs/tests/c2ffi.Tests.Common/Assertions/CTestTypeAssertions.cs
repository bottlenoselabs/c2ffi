// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace c2ffi.Tests.Library.Assertions;

public class CTestTypeAssertions(CTestType? instance) : ReferenceTypeAssertions<CTestType?, CTestTypeAssertions>(instance)
{
    protected override string Identifier => "type";

    [CustomAssertion]
    public void BeChar(string because = "", params object[] becauseArgs)
    {
        _ = Subject.Should().NotBeNull(because, becauseArgs);
        _ = Subject!.Name.Should().Be("char", because, becauseArgs);
        _ = Subject.NodeKind.Should().Be("primitive", because, becauseArgs);
        _ = Subject.AlignOf.Should().Be(1, because, becauseArgs);
        _ = Subject.SizeOf.Should().Be(1, because, becauseArgs);
        _ = Subject.IsAnonymous.Should().Be(false, because, becauseArgs);
        _ = Subject.InnerType.Should().BeNull(because, becauseArgs);
    }

    [CustomAssertion]
    public void BeInt(string because = "", params object[] becauseArgs)
    {
        _ = Subject.Should().NotBeNull(because, becauseArgs);
        _ = Subject!.Name.Should().Be("int", because, becauseArgs);
        _ = Subject.NodeKind.Should().Be("primitive", because, becauseArgs);
        _ = Subject.AlignOf.Should().Be(4, because, becauseArgs);
        _ = Subject.SizeOf.Should().Be(4, because, becauseArgs);
        _ = Subject.IsAnonymous.Should().Be(false, because, becauseArgs);
        _ = Subject.InnerType.Should().BeNull(because, becauseArgs);
    }
}
