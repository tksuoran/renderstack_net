//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;
using System.IO;
using System.Collections.Generic;
//using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RenderStack.Graphics
{
    /// \brief Contains one pixel colour values for Mipmap for Image
    /// 
    /// \note Mostly stable.
    public class Mipmap
    {
        public int Width    { get; private set; }
        public int Height   { get; private set; }

        private byte[] pixels;

        public byte[] Pixels { get { return pixels; } }

        public Mipmap(int width, int height)
        {
            Width = width;
            Height = height;
            pixels = new byte[width * height * 4];
        }

        public byte this[int x, int y, int c]
        {
            get
            {
                return pixels[(x + y * Width) * 4 + c];
            }
            set
            {
                pixels[(x + y * Width) * 4 + c] = value;
            }
        }
    }

    /// Holds pixel colour values for an Image. Can generate mipmaps using NVIDIA texture tools
    /// 
    /// \note Somewhat stable, somewhat experimental.
    public class Image : IDisposable, Nvidia.TextureTools.IOutputHandler
    {
        private Dictionary<int, Mipmap> mipmaps = new Dictionary<int, Mipmap>();
        public Dictionary<int, Mipmap> Mipmaps { get { return mipmaps; } }

        public int Width    { get; private set; }
        public int Height   { get; private set; }

        public Image(string file)
        {
            try
            {
                if(System.IO.File.Exists(file) == false)
                {
                    System.Diagnostics.Trace.TraceError("Error: File does not exist " + file);
                    MakeFallback();
                }

                if(file.EndsWith(".tga"))
                {
                    using(Paloma.TargaImage targa = new Paloma.TargaImage(file))
                    {
                        var bitmap = targa.Image;
                        UpdatePixels(bitmap);
                        targa.ClearAll();
                        targa.Dispose();
                    }
                }
                else
                {
                    var bitmap = new Bitmap(file);
                    UpdatePixels(bitmap);
                }
            }
            catch(System.Exception)
            {
                System.Diagnostics.Trace.TraceError("Error: Could not load " + file);
                MakeFallback();
            }
        }

        public Image(int width, int height, float r, float g, float b, float a)
        {
            Width   = width;
            Height  = height;

            Mipmap root = new Mipmap(Width, Height);
            Mipmaps[0] = root;

            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    root[x, y, 0] = (byte)(255.0f * r);
                    root[x, y, 1] = (byte)(255.0f * g);
                    root[x, y, 2] = (byte)(255.0f * b);
                    root[x, y, 3] = (byte)(255.0f * a);
                }
            }
        }

        private void MakeFallback()
        {
            if(Mipmaps.ContainsKey(0) == false || Mipmaps[0] == null)
            {
                Mipmaps[0] = new Mipmap(Width, Height);
                for(int y = 0; y < Height; y++)
                {
                    for(int x = 0; x < Width; x++)
                    {
                        // r 0 b
                        // g 1 g
                        // b 2 r
                        // a 3 a
                        //this[x, Height - 1 - y, 0] = data[x * 4 + (y * bmdata.Stride) + 2];
                        //this[x, Height - 1 - y, 1] = data[x * 4 + (y * bmdata.Stride) + 1];
                        //this[x, Height - 1 - y, 2] = data[x * 4 + (y * bmdata.Stride) + 0];
                        //this[x, Height - 1 - y, 3] = data[x * 4 + (y * bmdata.Stride) + 3];
                        Mipmaps[0][x, y, 0] = (byte)((System.Math.Max(x, y) % 64) * 4);
                        Mipmaps[0][x, y, 1] = 0;
                        Mipmaps[0][x, y, 2] = 0;
                        Mipmaps[0][x, y, 3] = 127;
                    }
                }
            }
        }

        public void Pow(float e)
        {
            for(int y = 0; y < Height; ++y)
            {
                for(int x = 0; x < Width; ++x)
                {
                    float r = (float)(Mipmaps[0][x, y, 0]) / 255.0f;
                    float g = (float)(Mipmaps[0][x, y, 1]) / 255.0f;
                    float b = (float)(Mipmaps[0][x, y, 2]) / 255.0f;
                    r = (float)System.Math.Pow(r, e);
                    g = (float)System.Math.Pow(g, e);
                    b = (float)System.Math.Pow(b, e);
                    Mipmaps[0][x, y, 0] = (byte)(r * 255.0f);
                    Mipmaps[0][x, y, 1] = (byte)(g * 255.0f);
                    Mipmaps[0][x, y, 2] = (byte)(b * 255.0f);
                }
            }
        }

        private void UpdatePixels(System.Drawing.Bitmap bitmap)
        {
            Width   = bitmap.Width;
            Height  = bitmap.Height;

            Mipmap root = new Mipmap(Width, Height);
            Mipmaps[0] = root;

            System.Drawing.Imaging.BitmapData bmdata = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            byte[] data = new byte[bitmap.Size.Width * bitmap.Size.Height * 4];
            Marshal.Copy(bmdata.Scan0, data, 0, bitmap.Size.Width * bitmap.Size.Height * 4);
            bitmap.UnlockBits(bmdata);

            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    // r 0 b
                    // g 1 g
                    // b 2 r
                    // a 3 a
                    //this[x, Height - 1 - y, 0] = data[x * 4 + (y * bmdata.Stride) + 2];
                    //this[x, Height - 1 - y, 1] = data[x * 4 + (y * bmdata.Stride) + 1];
                    //this[x, Height - 1 - y, 2] = data[x * 4 + (y * bmdata.Stride) + 0];
                    //this[x, Height - 1 - y, 3] = data[x * 4 + (y * bmdata.Stride) + 3];
                    root[x, y, 0] = data[x * 4 + (y * bmdata.Stride) + 2];
                    root[x, y, 1] = data[x * 4 + (y * bmdata.Stride) + 1];
                    root[x, y, 2] = data[x * 4 + (y * bmdata.Stride) + 0];
                    root[x, y, 3] = data[x * 4 + (y * bmdata.Stride) + 3];
                }
            }
        }

        //  http://code.google.com/p/nvidia-texture-tools/wiki/ApiDocumentation#Specifying_Input_Images
        public void GenerateMipmaps()
        {
            try
            {
                using(
                    Nvidia.TextureTools.CompressorOptionsBundle options = new Nvidia.TextureTools.CompressorOptionsBundle()
                )
                {
                    options.InputOptions.SetTextureLayout(
                        Nvidia.TextureTools.TextureType.Texture2D, 
                        Width, 
                        Height,
                        1
                    );
                    options.InputOptions.SetMipmapData      (Mipmaps[0].Pixels, Width, Height, 1, 0, 0);
                    options.InputOptions.SetAlphaMode       (Nvidia.TextureTools.AlphaMode.Transparency);
                    options.InputOptions.SetColorTransform  (Nvidia.TextureTools.ColorTransform.None);
                    options.InputOptions.SetGamma           (2.2f, 2.2f);
                    options.InputOptions.SetWrapMode        (Nvidia.TextureTools.WrapMode.Clamp);
                    options.InputOptions.SetMipmapFilter    (Nvidia.TextureTools.MipmapFilter.Kaiser);
                    options.InputOptions.SetKaiserParameters(3.0f, 4.0f, 1.0f);
                    options.InputOptions.SetNormalMap       (false);
                    options.InputOptions.SetNormalizeMipmaps(false);
                    options.CompressionOptions.SetFormat    (Nvidia.TextureTools.Format.RGBA);
                    options.OutputOptions.SetOutputHandler  (this);

                    options.Compress();
                }
            }
            catch(System.Exception)
            {
                System.Diagnostics.Trace.TraceWarning("Unable to generate mipmaps, could not use nvtt?");
                MakeFallback();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IOutputHandler Members

        int     nvttSize;
        //int     nvttDepth;
        //int     nvttFace;
        int     nvttOffset;
        Mipmap  nvttMipmap;

        void Nvidia.TextureTools.IOutputHandler.BeginImage(
            int size, 
            int width, 
            int height, 
            int depth, 
            int face, 
            int miplevel
        )
        {
            //Logger.Log("Incoming mipmap " + width + " x " + height + " level " + miplevel);
            nvttSize    = size;
            //nvttDepth   = depth;
            //nvttFace    = face;
            nvttOffset  = 0;
            if(size != width * height * depth * 4)
            {
                throw new InvalidDataException();
            }
            nvttMipmap = new Mipmap(width, height);
            Mipmaps[miplevel] = nvttMipmap;
        }

        bool Nvidia.TextureTools.IOutputHandler.WriteDataUnsafe(
            System.IntPtr data, int size
        )
        {
            if(nvttMipmap == null)
            {
                //Logger.Log("no mipmap yet, assuming header, skipping...");
                return true;
            }
            System.Runtime.InteropServices.Marshal.Copy(
                data, 
                nvttMipmap.Pixels, 
                nvttOffset, 
                size
            );
            nvttOffset += size;
            if(nvttOffset == nvttSize)
            {
                //Logger.Log("Mipmap " + nvttMipmap.Width + " x " + nvttMipmap.Height + " completed");
                nvttMipmap = null;
            }
            return true;
        }

        #endregion
    }
}
