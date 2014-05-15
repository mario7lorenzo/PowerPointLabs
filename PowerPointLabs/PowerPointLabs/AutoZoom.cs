﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using PowerPointLabs.Models;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace PowerPointLabs
{
    class AutoZoom
    {
        public static bool backgroundZoomChecked = true;

        public static void AddDrillDownAnimation()
        {
            try
            {
                var currentSlide = PowerPointPresentation.CurrentSlide as PowerPointSlide;
                PowerPoint.Shape selectedShape = Globals.ThisAddIn.Application.ActiveWindow.Selection.ShapeRange[1];

                if (currentSlide == null || currentSlide.Index == PowerPointPresentation.SlideCount)
                {
                    System.Windows.Forms.MessageBox.Show("Please select the correct slide", "Unable to Add Animations");
                    return;
                }

                PrepareZoomShape(currentSlide, ref selectedShape);
                PowerPointSlide nextSlide = GetNextSlide(currentSlide);
                if (nextSlide.Shapes.Count == 0)
                    return;

                PowerPoint.Shape nextSlidePicture = null, shapeToZoom = null;
                PowerPointDrillDownSlide addedSlide = null;

                if (backgroundZoomChecked)
                {
                    nextSlidePicture = GetNextSlidePictureWithBackground(currentSlide, nextSlide);
                    PrepareNextSlidePicture(currentSlide, selectedShape, ref nextSlidePicture);

                    addedSlide = (PowerPointDrillDownSlide)currentSlide.CreateDrillDownSlide();
                    addedSlide.DeleteAllShapes();
                    
                    currentSlide.Copy();
                    shapeToZoom = addedSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
                    FitShapeToSlide(ref shapeToZoom);
                    shapeToZoom.ZOrder(Office.MsoZOrderCmd.msoBringToFront);

                    addedSlide.PrepareForDrillDown();
                    addedSlide.AddDrillDownAnimation(shapeToZoom, nextSlidePicture);
                }
                else
                {
                    PowerPoint.Shape pictureOnNextSlide = null;
                    nextSlidePicture = GetNextSlidePictureWithoutBackground(currentSlide, nextSlide, ref pictureOnNextSlide);
                    PrepareNextSlidePicture(currentSlide, selectedShape, ref nextSlidePicture);
                    addedSlide = (PowerPointDrillDownSlide)currentSlide.CreateDrillDownSlide();
                    shapeToZoom = addedSlide.GetShapeWithSameIDAndName(nextSlidePicture);
                    addedSlide.DeleteShapeAnimations(shapeToZoom);

                    addedSlide.PrepareForDrillDown();
                    addedSlide.AddDrillDownAnimation(shapeToZoom, pictureOnNextSlide);
                    pictureOnNextSlide.Delete();
                }

                Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(addedSlide.Index);
                Globals.ThisAddIn.Application.CommandBars.ExecuteMso("AnimationPreview");
                AddAckSlide();
            }
            catch (Exception e)
            {
                //LogException(e, "AddDrillDownAnimation");
                throw;
            }
        }

        public static void AddStepBackAnimation()
        {
            try
            {
                var currentSlide = PowerPointPresentation.CurrentSlide as PowerPointSlide;
                PowerPoint.Shape selectedShape = Globals.ThisAddIn.Application.ActiveWindow.Selection.ShapeRange[1];

                if (currentSlide == null || currentSlide.Index == 1)
                {
                    System.Windows.Forms.MessageBox.Show("Please select the correct slide", "Unable to Add Animations");
                    return;
                }

                PrepareZoomShape(currentSlide, ref selectedShape);
                PowerPointSlide previousSlide = GetPreviousSlide(currentSlide);
                if (previousSlide.Shapes.Count == 0)
                    return;

                PowerPoint.Shape previousSlidePicture = null, shapeToZoom = null;
                PowerPointStepBackSlide addedSlide = null;

                if (backgroundZoomChecked)
                {
                    previousSlidePicture = GetPreviousSlidePictureWithBackground(currentSlide, previousSlide);
                    PreparePreviousSlidePicture(currentSlide, selectedShape, ref previousSlidePicture);

                    addedSlide = (PowerPointStepBackSlide)previousSlide.CreateStepBackSlide();
                    addedSlide.DeleteAllShapes();

                    shapeToZoom = GetStepBackWithBackgroundShapeToZoom(currentSlide, addedSlide, previousSlidePicture);

                    addedSlide.PrepareForStepBack();
                    addedSlide.AddStepBackAnimation(shapeToZoom, previousSlidePicture);
                }
                else
                {
                    addedSlide = (PowerPointStepBackSlide)previousSlide.CreateStepBackSlide();
                    addedSlide.DeleteAllShapes();

                    shapeToZoom = GetStepBackWithoutBackgroundShapeToZoom(currentSlide, addedSlide, previousSlide);
                    shapeToZoom.Copy();
                    previousSlidePicture = currentSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
                    PreparePreviousSlidePicture(currentSlide, selectedShape, ref previousSlidePicture);

                    addedSlide.PrepareForStepBack();
                    addedSlide.AddStepBackAnimation(shapeToZoom, previousSlidePicture);
                }

                currentSlide.Transition.EntryEffect = PowerPoint.PpEntryEffect.ppEffectFadeSmoothly;
                currentSlide.Transition.Duration = 0.25f;
                Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(addedSlide.Index);
                Globals.ThisAddIn.Application.CommandBars.ExecuteMso("AnimationPreview");
                AddAckSlide();
            }
            catch (Exception e)
            {
                //LogException(e, "AddStepBackAnimation");
                throw;
            }
        }

        private static void RemoveTextFromShape(PowerPoint.Shape shape)
        {
            if (shape.HasTextFrame == Office.MsoTriState.msoTrue)
                if (shape.TextFrame.HasText == Office.MsoTriState.msoTrue)
                    shape.TextFrame.TextRange.Text = "";
        }

        private static void PrepareZoomShape(PowerPointSlide currentSlide, ref PowerPoint.Shape selectedShape)
        {
            currentSlide.DeleteShapeAnimations(selectedShape);
            RemoveTextFromShape(selectedShape);
            selectedShape.Rotation = 0;
        }

        private static PowerPointSlide GetNextSlide(PowerPointSlide currentSlide)
        {
            PowerPointSlide nextSlide = PowerPointPresentation.Slides.ElementAt(currentSlide.Index);
            PowerPointSlide tempSlide = nextSlide;
            while (nextSlide.Name.Contains("PPTLabsZoomIn") && nextSlide.Index < PowerPointPresentation.SlideCount)
            {
                nextSlide = PowerPointPresentation.Slides.ElementAt(tempSlide.Index);
                tempSlide.Delete();
            }
            nextSlide.Transition.EntryEffect = PowerPoint.PpEntryEffect.ppEffectFadeSmoothly;
            nextSlide.Transition.Duration = 0.25f;
            return nextSlide;
        }

        private static PowerPointSlide GetPreviousSlide(PowerPointSlide currentSlide)
        {
            PowerPointSlide previousSlide = PowerPointPresentation.Slides.ElementAt(currentSlide.Index - 2);
            PowerPointSlide tempSlide = previousSlide;
            while (previousSlide.Name.Contains("PPTLabsZoomOut") && previousSlide.Index > 1)
            {
                previousSlide = PowerPointPresentation.Slides.ElementAt(tempSlide.Index - 2);
                tempSlide.Delete();
            }

            return previousSlide;
        }

        private static PowerPoint.Shape GetNextSlidePictureWithoutBackground(PowerPointSlide currentSlide, PowerPointSlide nextSlide, ref PowerPoint.Shape pictureOnNextSlide)
        {
            Globals.ThisAddIn.Application.ActiveWindow.Selection.Unselect();
            Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(nextSlide.Index);

            List<PowerPoint.Shape> shapesOnNextSlide = new List<PowerPoint.Shape>();
            foreach (PowerPoint.Shape sh in nextSlide.Shapes)
            {
                if (!nextSlide.HasEntryAnimation(sh))
                    shapesOnNextSlide.Add(sh);
            }

            PowerPoint.Shape shapeCopy = null;
            foreach (PowerPoint.Shape sh in shapesOnNextSlide)
            {
                sh.Copy();
                shapeCopy = nextSlide.Shapes.Paste()[1];
                CopyShapeAttributes(sh, ref shapeCopy);
                shapeCopy.Select(Office.MsoTriState.msoFalse);
            }

            PowerPoint.Selection sel = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            PowerPoint.Shape shapeGroup = null;
            if (sel.ShapeRange.Count > 1)
                shapeGroup = sel.ShapeRange.Group();
            else
                shapeGroup = sel.ShapeRange[1];

            shapeGroup.Copy();
            pictureOnNextSlide = nextSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            CopyShapePosition(shapeGroup, ref pictureOnNextSlide);
            shapeGroup.Delete();

            pictureOnNextSlide.Copy();
            PowerPoint.Shape slidePicture = currentSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            return slidePicture;
        }

        private static PowerPoint.Shape GetNextSlidePictureWithBackground(PowerPointSlide currentSlide, PowerPointSlide nextSlide)
        {
            PowerPointSlide nextSlideCopy = nextSlide.Duplicate();
            List<PowerPoint.Shape> shapes = nextSlideCopy.Shapes.Cast<PowerPoint.Shape>().ToList();
            var matchingShapes = shapes.Where(current => nextSlideCopy.HasEntryAnimation(current));
            foreach (PowerPoint.Shape s in matchingShapes)
                s.Delete();

            nextSlideCopy.Copy();
            PowerPoint.Shape slidePicture = currentSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            nextSlideCopy.Delete();
            return slidePicture;
        }

        private static PowerPoint.Shape GetPreviousSlidePictureWithBackground(PowerPointSlide currentSlide, PowerPointSlide previousSlide)
        {
            PowerPointSlide previousSlideCopy = previousSlide.Duplicate();
            List<PowerPoint.Shape> shapes = previousSlideCopy.Shapes.Cast<PowerPoint.Shape>().ToList();
            var matchingShapes = shapes.Where(current => previousSlideCopy.HasExitAnimation(current));
            foreach (PowerPoint.Shape s in matchingShapes)
                s.Delete();

            previousSlideCopy.Copy();
            PowerPoint.Shape slidePicture = currentSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            previousSlideCopy.Delete();
            return slidePicture;
        }

        private static void PrepareNextSlidePicture(PowerPointSlide currentSlide, PowerPoint.Shape selectedShape, ref PowerPoint.Shape nextSlidePicture)
        {
            if (selectedShape.Name.Contains("PPTZoomInShape"))
                CopyShapeAttributes(selectedShape, ref nextSlidePicture);
            else
            {
                nextSlidePicture.LockAspectRatio = Office.MsoTriState.msoTrue;
                if (selectedShape.Width > selectedShape.Height)
                    nextSlidePicture.Height = selectedShape.Height;
                else
                    nextSlidePicture.Width = selectedShape.Width;

                CopyShapePosition(selectedShape, ref nextSlidePicture);
            }
 
            selectedShape.Visible = Office.MsoTriState.msoFalse;
            nextSlidePicture.Name = "PPTZoomInShape" + DateTime.Now.ToString("yyyyMMddHHmmssffff");

            PowerPoint.Effect effectAppear = currentSlide.TimeLine.MainSequence.AddEffect(nextSlidePicture, PowerPoint.MsoAnimEffect.msoAnimEffectFade, PowerPoint.MsoAnimateByLevel.msoAnimateLevelNone, PowerPoint.MsoAnimTriggerType.msoAnimTriggerOnPageClick);
            effectAppear.Timing.Duration = 0.50f;
        }

        private static void PreparePreviousSlidePicture(PowerPointSlide currentSlide, PowerPoint.Shape selectedShape, ref PowerPoint.Shape previousSlidePicture)
        {
            if (selectedShape.Name.Contains("PPTZoomOutShape"))
                CopyShapeAttributes(selectedShape, ref previousSlidePicture);
            else
            {
                previousSlidePicture.LockAspectRatio = Office.MsoTriState.msoTrue;
                if (selectedShape.Width > selectedShape.Height)
                    previousSlidePicture.Height = selectedShape.Height;
                else
                    previousSlidePicture.Width = selectedShape.Width;

                CopyShapePosition(selectedShape, ref previousSlidePicture);
            }

            selectedShape.Visible = Office.MsoTriState.msoFalse;
            previousSlidePicture.Name = "PPTZoomOutShape" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        private static PowerPoint.Shape GetStepBackWithBackgroundShapeToZoom(PowerPointSlide currentSlide, PowerPointSlide addedSlide, PowerPoint.Shape previousSlidePicture)
        {
            currentSlide.Copy();
            PowerPoint.Shape currentSlideCopy = addedSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            FitShapeToSlide(ref currentSlideCopy);

            previousSlidePicture.Copy();
            PowerPoint.Shape previousSlidePictureCopy = addedSlide.Shapes.Paste()[1];

            Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(addedSlide.Index);
            currentSlideCopy.Select();
            previousSlidePictureCopy.Select(Office.MsoTriState.msoFalse);
            PowerPoint.ShapeRange selection = Globals.ThisAddIn.Application.ActiveWindow.Selection.ShapeRange;
            PowerPoint.Shape shapeGroup = selection.Group();

            shapeGroup.LockAspectRatio = Office.MsoTriState.msoFalse;
            shapeGroup.Left = (PowerPointPresentation.SlideWidth / 2) - ((previousSlidePicture.Left + (previousSlidePicture.Width) / 2) * PowerPointPresentation.SlideWidth / previousSlidePicture.Width);
            shapeGroup.Top = (PowerPointPresentation.SlideHeight / 2) - ((previousSlidePicture.Top + (previousSlidePicture.Height) / 2) * PowerPointPresentation.SlideHeight / previousSlidePicture.Height);
            shapeGroup.Width = PowerPointPresentation.SlideWidth * PowerPointPresentation.SlideWidth / previousSlidePicture.Width;
            shapeGroup.Height = PowerPointPresentation.SlideHeight * PowerPointPresentation.SlideHeight / previousSlidePicture.Height;

            shapeGroup.Left += (0 - previousSlidePictureCopy.Left);
            shapeGroup.Top += (0 - previousSlidePictureCopy.Top);

            shapeGroup.ZOrder(Office.MsoZOrderCmd.msoBringToFront);

            return shapeGroup;
        }

        private static PowerPoint.Shape GetStepBackWithoutBackgroundShapeToZoom(PowerPointSlide currentSlide, PowerPointSlide addedSlide, PowerPointSlide previousSlide)
        {
            Globals.ThisAddIn.Application.ActiveWindow.Selection.Unselect();
            Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(addedSlide.Index);

            foreach (PowerPoint.Shape sh in previousSlide.Shapes)
            {
                if (!previousSlide.HasExitAnimation(sh))
                {
                    sh.Copy();
                    PowerPoint.Shape shapeCopy = addedSlide.Shapes.Paste()[1];
                    CopyShapeAttributes(sh, ref shapeCopy);
                    shapeCopy.Select(Office.MsoTriState.msoFalse);
                } 
            }

            PowerPoint.Selection sel = Globals.ThisAddIn.Application.ActiveWindow.Selection;
            PowerPoint.Shape shapeGroup = null;
            if (sel.ShapeRange.Count > 1)
                shapeGroup = sel.ShapeRange.Group();
            else
                shapeGroup = sel.ShapeRange[1];

            shapeGroup.Copy();
            PowerPoint.Shape previousSlidePicture = addedSlide.Shapes.PasteSpecial(PowerPoint.PpPasteDataType.ppPastePNG)[1];
            CopyShapePosition(shapeGroup, ref previousSlidePicture);
            previousSlidePicture.Name = "PPTZoomOutShape" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            shapeGroup.Delete();

            return previousSlidePicture;
        }

        private static void CopyShapePosition(PowerPoint.Shape shapeToCopy, ref PowerPoint.Shape shapeToMove)
        {
            shapeToMove.Left = shapeToCopy.Left + (shapeToCopy.Width / 2) - (shapeToMove.Width / 2);
            shapeToMove.Top = shapeToCopy.Top + (shapeToCopy.Height / 2) - (shapeToMove.Height / 2);
        }

        private static void CopyShapeSize(PowerPoint.Shape shapeToCopy, ref PowerPoint.Shape shapeToMove)
        {
            shapeToMove.LockAspectRatio = Office.MsoTriState.msoFalse;
            shapeToMove.Width = shapeToCopy.Width;
            shapeToMove.Height = shapeToCopy.Height;
        }

        private static void CopyShapeAttributes(PowerPoint.Shape shapeToCopy, ref PowerPoint.Shape shapeToMove)
        {
            CopyShapeSize(shapeToCopy, ref shapeToMove);
            CopyShapePosition(shapeToCopy, ref shapeToMove);
        }

        private static void FitShapeToSlide(ref PowerPoint.Shape shapeToMove)
        {
            shapeToMove.LockAspectRatio = Office.MsoTriState.msoFalse;
            shapeToMove.Left = 0;
            shapeToMove.Top = 0;
            shapeToMove.Width = PowerPointPresentation.SlideWidth;
            shapeToMove.Height = PowerPointPresentation.SlideHeight;
        }

        private static void AddAckSlide()
        {
            try
            {
                PowerPointSlide lastSlide = PowerPointPresentation.Slides.Last();
                if (!lastSlide.isAckSlide())
                {
                    lastSlide.CreateAckSlide();
                }
            }
            catch (Exception e)
            {
                //LogException(e, "AddAckSlide");
                throw;
            }
        }
    }
}
