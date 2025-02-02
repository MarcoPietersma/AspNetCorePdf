﻿using PdfSharp.Fonts;
using System;
using System.Diagnostics;
using System.IO;

namespace Macaw.Pdf
{
    //This implementation is obviously not very good --> Though it should be enough for everyone to implement their own.
    public class FontResolver : IFontResolver
    {
        private readonly string _resourcesPath = string.Empty;

        public FontResolver(string resourcesPath)
        {
            _resourcesPath = resourcesPath;
        }

        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            Debug.WriteLine($"Trying to resolve {familyName},{isBold},{isItalic}");
            if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo($"{_resourcesPath}\\OpenSans-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo($"{_resourcesPath}\\OpenSans-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo($"{_resourcesPath}\\OpenSans-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo($"{_resourcesPath}\\OpenSans-Regular.ttf");
                }
            }
            return null;
        }
    }
}