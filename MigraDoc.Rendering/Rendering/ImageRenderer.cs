#region MigraDoc - Creating Documents on the Fly

// Authors: Klaus Potzesny
//
// Copyright (c) 2001-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com http://www.migradoc.com http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering.Resources;
using PdfSharp.Drawing;
using System;
using System.Diagnostics;
using System.IO;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders images.
    /// </summary>
    public class ImageRenderer : ShapeRenderer
    {
        public ImageRenderer(XGraphics gfx, Image image, FieldInfos fieldInfos)
            : base(gfx, image, fieldInfos)
        {
            _image = image;
            var renderInfo = new ImageRenderInfo
            {
                DocumentObject = _shape
            };
            _renderInfo = renderInfo;
        }

        public ImageRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _image = (Image)renderInfo.DocumentObject;
        }

        public override void Format(Area area, FormatInfo previousFormatInfo)
        {
            _imageFilePath = _image.GetFilePath(_documentRenderer.WorkingDirectory);
            // The Image is stored in the string if path starts with "base64:", otherwise we check
            // whether the file exists.
            if (!_imageFilePath.StartsWith("base64:") &&
                !XImage.ExistsFile(_imageFilePath))
            {
                _failure = ImageFailure.FileNotFound;
                Debug.WriteLine(Messages2.ImageNotFound(_image.Name), "warning");
            }
            var formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
            formatInfo.Failure = _failure;
            formatInfo.ImagePath = _imageFilePath;
            CalculateImageDimensions();
            base.Format(area, previousFormatInfo);
        }

        protected override XUnit ShapeHeight
        {
            get
            {
                var formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
                return formatInfo.Height + _lineFormatRenderer.GetWidth();
            }
        }

        protected override XUnit ShapeWidth
        {
            get
            {
                var formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
                return formatInfo.Width + _lineFormatRenderer.GetWidth();
            }
        }

        public override void Render()
        {
            RenderFilling();

            var formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var destRect = new XRect(contentArea.X, contentArea.Y, formatInfo.Width, formatInfo.Height);

            if (formatInfo.Failure == ImageFailure.None)
            {
                XImage xImage = null;
                try
                {
                    var srcRect = new XRect(formatInfo.CropX, formatInfo.CropY, formatInfo.CropWidth, formatInfo.CropHeight);
                    //xImage = XImage.FromFile(formatInfo.ImagePath);
                    xImage = CreateXImage(formatInfo.ImagePath);
                    _gfx.DrawImage(xImage, destRect, srcRect, XGraphicsUnit.Point); //Pixel.
                }
                catch (Exception)
                {
                    RenderFailureImage(destRect);
                }
                finally
                {
                    if (xImage != null)
                    {
                        xImage.Dispose();
                    }
                }
            }
            else
            {
                RenderFailureImage(destRect);
            }

            RenderLine();
        }

        private void RenderFailureImage(XRect destRect)
        {
            _gfx.DrawRectangle(XBrushes.LightGray, destRect);
            string failureString;
            var formatInfo = (ImageFormatInfo)RenderInfo.FormatInfo;

            switch (formatInfo.Failure)
            {
                case ImageFailure.EmptySize:
                    failureString = Messages2.DisplayEmptyImageSize;
                    break;

                case ImageFailure.FileNotFound:
                    failureString = Messages2.DisplayImageFileNotFound;
                    break;

                case ImageFailure.InvalidType:
                    failureString = Messages2.DisplayInvalidImageType;
                    break;

                case ImageFailure.NotRead:
                default:
                    failureString = Messages2.DisplayImageNotRead;
                    break;
            }

            // Create stub font
            var font = new XFont("Courier New", 8);
            _gfx.DrawString(failureString, font, XBrushes.Red, destRect, XStringFormats.Center);
        }

        private void CalculateImageDimensions()
        {
            var formatInfo = (ImageFormatInfo)_renderInfo.FormatInfo;

            if (formatInfo.Failure == ImageFailure.None)
            {
                XImage xImage = null;
                try
                {
                    //xImage = XImage.FromFile(_imageFilePath);
                    xImage = CreateXImage(_imageFilePath);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine(Messages2.InvalidImageType(ex.Message));
                    formatInfo.Failure = ImageFailure.InvalidType;
                }

                if (formatInfo.Failure == ImageFailure.None)
                {
                    try
                    {
                        XUnit usrWidth = _image.Width.Point;
                        XUnit usrHeight = _image.Height.Point;
                        var usrWidthSet = !_image._width.IsNull;
                        var usrHeightSet = !_image._height.IsNull;

                        var resultWidth = usrWidth;
                        var resultHeight = usrHeight;

                        Debug.Assert(xImage != null);
                        double xPixels = xImage.PixelWidth;
                        var usrResolutionSet = !_image._resolution.IsNull;

                        var horzRes = usrResolutionSet ? _image.Resolution : xImage.HorizontalResolution;
                        var vertRes = usrResolutionSet ? _image.Resolution : xImage.VerticalResolution;

                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        if (horzRes == 0 && vertRes == 0)
                        {
                            horzRes = 72;
                            vertRes = 72;
                        }
                        else if (horzRes == 0)
                        {
                            Debug.Assert(false, "How can this be?");
                            horzRes = 72;
                        }
                        else if (vertRes == 0)
                        {
                            Debug.Assert(false, "How can this be?");
                            vertRes = 72;
                        }
                        // ReSharper restore CompareOfFloatsByEqualityOperator

                        var inherentWidth = XUnit.FromInch(xPixels / horzRes);
                        double yPixels = xImage.PixelHeight;
                        var inherentHeight = XUnit.FromInch(yPixels / vertRes);

                        //bool lockRatio = _image.IsNull("LockAspectRatio") ? true : _image.LockAspectRatio;
                        var lockRatio = _image._lockAspectRatio.IsNull || _image.LockAspectRatio;

                        var scaleHeight = _image.ScaleHeight;
                        var scaleWidth = _image.ScaleWidth;
                        //bool scaleHeightSet = !_image.IsNull("ScaleHeight");
                        //bool scaleWidthSet = !_image.IsNull("ScaleWidth");
                        var scaleHeightSet = !_image._scaleHeight.IsNull;
                        var scaleWidthSet = !_image._scaleWidth.IsNull;

                        if (lockRatio && !(scaleHeightSet && scaleWidthSet))
                        {
                            if (usrWidthSet && !usrHeightSet)
                            {
                                resultHeight = inherentHeight / inherentWidth * usrWidth;
                            }
                            else if (usrHeightSet && !usrWidthSet)
                            {
                                resultWidth = inherentWidth / inherentHeight * usrHeight;
                            }
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            else if (!usrHeightSet && !usrWidthSet)
                            {
                                resultHeight = inherentHeight;
                                resultWidth = inherentWidth;
                            }

                            if (scaleHeightSet)
                            {
                                resultHeight = resultHeight * scaleHeight;
                                resultWidth = resultWidth * scaleHeight;
                            }
                            if (scaleWidthSet)
                            {
                                resultHeight = resultHeight * scaleWidth;
                                resultWidth = resultWidth * scaleWidth;
                            }
                        }
                        else
                        {
                            if (!usrHeightSet)
                            {
                                resultHeight = inherentHeight;
                            }

                            if (!usrWidthSet)
                            {
                                resultWidth = inherentWidth;
                            }

                            if (scaleHeightSet)
                            {
                                resultHeight = resultHeight * scaleHeight;
                            }

                            if (scaleWidthSet)
                            {
                                resultWidth = resultWidth * scaleWidth;
                            }
                        }

                        formatInfo.CropWidth = (int)xPixels;
                        formatInfo.CropHeight = (int)yPixels;
                        if (_image._pictureFormat != null && !_image._pictureFormat.IsNull())
                        {
                            var picFormat = _image.PictureFormat;
                            //Cropping in pixels.
                            XUnit cropLeft = picFormat.CropLeft.Point;
                            XUnit cropRight = picFormat.CropRight.Point;
                            XUnit cropTop = picFormat.CropTop.Point;
                            XUnit cropBottom = picFormat.CropBottom.Point;
                            formatInfo.CropX = (int)(horzRes * cropLeft.Inch);
                            formatInfo.CropY = (int)(vertRes * cropTop.Inch);
                            formatInfo.CropWidth -= (int)(horzRes * ((XUnit)(cropLeft + cropRight)).Inch);
                            formatInfo.CropHeight -= (int)(vertRes * ((XUnit)(cropTop + cropBottom)).Inch);

                            //Scaled cropping of the height and width.
                            var xScale = resultWidth / inherentWidth;
                            var yScale = resultHeight / inherentHeight;

                            cropLeft = xScale * cropLeft;
                            cropRight = xScale * cropRight;
                            cropTop = yScale * cropTop;
                            cropBottom = yScale * cropBottom;

                            resultHeight = resultHeight - cropTop - cropBottom;
                            resultWidth = resultWidth - cropLeft - cropRight;
                        }
                        if (resultHeight <= 0 || resultWidth <= 0)
                        {
                            formatInfo.Width = XUnit.FromCentimeter(2.5);
                            formatInfo.Height = XUnit.FromCentimeter(2.5);
                            Debug.WriteLine(Messages2.EmptyImageSize);
                            _failure = ImageFailure.EmptySize;
                        }
                        else
                        {
                            formatInfo.Width = resultWidth;
                            formatInfo.Height = resultHeight;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(Messages2.ImageNotReadable(_image.Name, ex.Message));
                        formatInfo.Failure = ImageFailure.NotRead;
                    }
                    finally
                    {
                        if (xImage != null)
                        {
                            xImage.Dispose();
                        }
                    }
                }
            }
            if (formatInfo.Failure != ImageFailure.None)
            {
                if (!_image._width.IsNull)
                {
                    formatInfo.Width = _image.Width.Point;
                }
                else
                {
                    formatInfo.Width = XUnit.FromCentimeter(2.5);
                }

                if (!_image._height.IsNull)
                {
                    formatInfo.Height = _image.Height.Point;
                }
                else
                {
                    formatInfo.Height = XUnit.FromCentimeter(2.5);
                }
            }
        }

        private XImage CreateXImage(string uri)
        {
            if (uri.StartsWith("base64:"))
            {
                var base64 = uri.Substring("base64:".Length);
                var bytes = Convert.FromBase64String(base64);
#if WPF || CORE_WITH_GDI || GDI
                // WPF stores a reference to the stream publicly. We must not destroy the stream
                // here, otherwise rendering the PDF will fail. Same for GDI. CORE currently uses
                // the GDI implementation. We have to rely on the garbage collector to properly
                // dispose the MemoryStream.
                {
                    Stream stream = new MemoryStream(bytes);
                    var image = XImage.FromStream(stream);
                    return image;
                }
#else
                using (Stream stream = new MemoryStream(bytes))
                {
                    XImage image = XImage.FromStream(stream);
                    return image;
                }
#endif
            }
            return XImage.FromFile(uri);
        }

        private readonly Image _image;
        private string _imageFilePath;
        private ImageFailure _failure;
    }
}