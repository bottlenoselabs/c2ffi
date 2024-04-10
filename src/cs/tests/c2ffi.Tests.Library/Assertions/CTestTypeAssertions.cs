// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace c2ffi.Tests.Library.Assertions;

public class CTestTypeAssertions : ReferenceTypeAssertions<CTestType?, CTestTypeAssertions>
{
    public CTestTypeAssertions(CTestType? instance)
        : base(instance)
    {
    }

    protected override string Identifier => "type";

    [CustomAssertion]
    public void BeChar(string because = "", params object[] becauseArgs)
    {
        Subject.Should().NotBeNull(because, becauseArgs);
        Subject!.Name.Should().Be("char", because, becauseArgs);
        Subject.NodeKind.Should().Be("primitive", because, becauseArgs);
        Subject.AlignOf.Should().Be(1, because, becauseArgs);
        Subject.SizeOf.Should().Be(1, because, becauseArgs);
        Subject.IsAnonymous.Should().Be(false, because, becauseArgs);
        Subject.InnerType.Should().BeNull(because, becauseArgs);
    }

    [CustomAssertion]
    public void BeInt(string because = "", params object[] becauseArgs)
    {
        Subject.Should().NotBeNull(because, becauseArgs);
        Subject!.Name.Should().Be("int", because, becauseArgs);
        Subject.NodeKind.Should().Be("primitive", because, becauseArgs);
        Subject.AlignOf.Should().Be(4, because, becauseArgs);
        Subject.SizeOf.Should().Be(4, because, becauseArgs);
        Subject.IsAnonymous.Should().Be(false, because, becauseArgs);
        Subject.InnerType.Should().BeNull(because, becauseArgs);
    }
}
