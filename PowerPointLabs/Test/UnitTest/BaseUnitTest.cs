﻿using System.Globalization;
using System.IO;
using TestInterface;
using Microsoft.Office.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Util;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using PowerPointLabs.Utils;

namespace Test.UnitTest
{
    [TestClass]
    public abstract class BaseUnitTest
    {
        public TestContext TestContext { get; set; }

        protected IPowerPointOperations PpOperations;

        protected PowerPoint.Application App;

        protected PowerPoint.Presentation Pres;

        // To be implemented by downstream testing classes,
        // specify the name for the testing slide.
        // It is assumed that the testing slides reside
        // in "doc/test" folder.
        protected abstract string GetTestingSlideName();

        [TestInitialize]
        public void Setup()
        {
            CultureUtil.SetDefaultCulture(CultureInfo.GetCultureInfo("en-US"));
            App = new PowerPoint.Application();
            Pres = App.Presentations.Open(
                PathUtil.GetDocTestPath() + GetTestingSlideName(),
                WithWindow: MsoTriState.msoFalse);
            PpOperations = new UnitTestPpOperations(Pres, App);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
            {
                if (!Directory.Exists(PathUtil.GetTestFailurePath()))
                {
                    Directory.CreateDirectory(PathUtil.GetTestFailurePath());
                }
                PpOperations.SavePresentationAs(
                    PathUtil.GetTestFailurePresentationPath(
                        TestContext.TestName + "_" +
                        GetTestingSlideName()));
            }
            PpOperations.ClosePresentation();
        }
    }
}
