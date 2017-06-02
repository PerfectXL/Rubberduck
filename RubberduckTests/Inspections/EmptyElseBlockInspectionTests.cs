﻿using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using RubberduckTests.Mocks;
using Rubberduck.Inspections.Concrete;
using Rubberduck.Parsing.Inspections.Resources;

namespace RubberduckTests.Inspections
{
    [TestClass]
    public class EmptyElseBlockInspectionTests
    {
        [TestMethod]
        [TestCategory("Inspections")]
        public void InspectionType()
        {
            var actualInspection = new EmptyElseBlockInspection(null);
            var expectedInspection = CodeInspectionType.CodeQualityIssues;

            Assert.AreEqual(expectedInspection, actualInspection.InspectionType);
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void InspectionName()
        {
            const string expectedName = nameof(EmptyElseBlockInspection);
            var actualInspection = new EmptyElseBlockInspection(null);

            Assert.AreEqual(expectedName, actualInspection.Name);
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void EmptyElseBlock_FiresOnEmptyIfBlock_EmptyElseBlock()
        {
            const string inputcode =
@"Sub Foo()
    If True Then
    Else
    End If
End Sub";

            IVBComponent component;
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputcode, out component);
            var state = MockParser.CreateAndParse(vbe.Object);

            var actualInspection = new EmptyElseBlockInspection(state);
            var actualInspector = InspectionsHelper.GetInspector(actualInspection);
            var actualResults = actualInspector.FindIssuesAsync(state, CancellationToken.None).Result;
            const int expectedCount = 2;

            Assert.AreEqual(expectedCount, actualResults.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void EmptyElseBlock_FiresOnEmptyElseBlock_HasNonEmptyIfBlock()
        {
            const string inputCode =
@"Sub Foo()
    If True Then
        Dim d
        d = 0
    Else
    End If
End Sub";

            IVBComponent component;
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out component);
            var state = MockParser.CreateAndParse(vbe.Object);

            var actualInspection = new EmptyElseBlockInspection(state);
            var actualInspector = InspectionsHelper.GetInspector(actualInspection);
            var actualResults = actualInspector.FindIssuesAsync(state, CancellationToken.None).Result;
            const int expectedCount = 0;

            Assert.AreEqual(expectedCount, actualResults.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void EmptyElseBlock_FiresOnEmptyElseBlock_HasQuoteComment()
        {
            const string inputCode =
@"Sub Foo()
    If True Then
    Else
        'Some Comment
    End If
End Sub";
            IVBComponent component;
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out component);
            var state = MockParser.CreateAndParse(vbe.Object);

            var actualInspection = new EmptyElseBlockInspection(state);
            var actualInspector = InspectionsHelper.GetInspector(actualInspection);
            var actualResults = actualInspector.FindIssuesAsync(state, CancellationToken.None).Result;
            const int expectedCount = 0;

            Assert.AreEqual(expectedCount, actualResults.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]
        public void EmptyElseBlock_FiresOnEmptyElseBlock_HasRemComment()
        {
            const string inputCode =
@"Sub Foo()
    If True Then
    Else
        Rem Some Comment
    End If
End Sub";

            IVBComponent component;
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out component);
            var state = MockParser.CreateAndParse(vbe.Object);

            var actualInspection = new EmptyElseBlockInspection(state);
            var actualInspector = InspectionsHelper.GetInspector(actualInspection);
            var actualResults = actualInspector.FindIssuesAsync(state, CancellationToken.None).Result;
            const int expectedCount = 0;

            Assert.AreEqual(expectedCount, actualResults.Count());
        }

        [TestMethod]
        [TestCategory("Inspections")]

    }
}