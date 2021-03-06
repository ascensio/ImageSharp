﻿// <copyright file="AutoOrientTests.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests.Processing.Transforms
{
    using System;
    using ImageSharp.PixelFormats;
    using ImageSharp.Processing;

    using Xunit;

    public class AutoOrientTests : FileTestBase
    {
        public static readonly string[] FlipFiles = { TestImages.Bmp.F };

        public static readonly TheoryData<RotateType, FlipType, ushort> OrientationValues
            = new TheoryData<RotateType, FlipType, ushort>
        {
            { RotateType.None,      FlipType.None,       0 },
            { RotateType.None,      FlipType.None,       1 },
            { RotateType.None,      FlipType.Horizontal, 2 },
            { RotateType.Rotate180, FlipType.None,       3 },
            { RotateType.Rotate180, FlipType.Horizontal, 4 },
            { RotateType.Rotate90,  FlipType.Horizontal, 5 },
            { RotateType.Rotate270, FlipType.None,       6 },
            { RotateType.Rotate90,  FlipType.Vertical,   7 },
            { RotateType.Rotate90,  FlipType.None,       8 },
        };

        public static readonly TheoryData<ExifDataType, byte[]> InvalidOrientationValues
            = new TheoryData<ExifDataType, byte[]>
        {
            { ExifDataType.Byte, new byte[] { 1 } },
            { ExifDataType.SignedByte, new byte[] { 2 } },
            { ExifDataType.SignedShort, BitConverter.GetBytes((short) 3) },
            { ExifDataType.Long, BitConverter.GetBytes((uint) 4) },
            { ExifDataType.SignedLong, BitConverter.GetBytes((int) 5) }
        };

        [Theory]
        [WithFileCollection(nameof(FlipFiles), nameof(OrientationValues), DefaultPixelType)]
        public void ImageShouldAutoRotate<TPixel>(TestImageProvider<TPixel> provider, RotateType rotateType, FlipType flipType, ushort orientation)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.MetaData.ExifProfile = new ExifProfile();
                image.MetaData.ExifProfile.SetValue(ExifTag.Orientation, orientation);

                image.RotateFlip(rotateType, flipType)
                    .DebugSave(provider, string.Join("_", rotateType, flipType, orientation, "1_before"), Extensions.Bmp)
                    .AutoOrient()
                    .DebugSave(provider, string.Join("_", rotateType, flipType, orientation, "2_after"), Extensions.Bmp);
            }
        }

        [Theory]
        [WithFileCollection(nameof(FlipFiles), nameof(InvalidOrientationValues), DefaultPixelType)]
        public void ImageShouldAutoRotateInvalidValues<TPixel>(TestImageProvider<TPixel> provider, ExifDataType dataType, byte[] orientation)
            where TPixel : struct, IPixel<TPixel>
        {
            var profile = new ExifProfile();
            profile.SetValue(ExifTag.JPEGTables, orientation);

            byte[] bytes = profile.ToByteArray();
            // Change the tag into ExifTag.Orientation
            bytes[16] = 18;
            bytes[17] = 1;
            // Change the data type
            bytes[18] = (byte)dataType;
            // Change the number of components
            bytes[20] = 1;

            using (Image<TPixel> image = provider.GetImage())
            {
                image.MetaData.ExifProfile = new ExifProfile(bytes);
                image.AutoOrient();
            }
        }
    }
}