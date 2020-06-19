using System.Collections.Generic;
using System.IO;

//using ID4 = System.UInt32;
using I1 = System.SByte;
using I2 = System.Int16;
using I4 = System.Int32;
using U1 = System.Byte;
using U2 = System.UInt16;
using U4 = System.UInt32;
using F4 = System.Single;
using S0 = System.String;
using VX = System.UInt32;
using COL4 = RenderStack.Math.Vector4;
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public class LWFile
    {
        private Stream  stream;
        private ID      type;
        private ulong   bytesRead;
        private ulong   length;

        private Stack<ulong>  stack = new Stack<ulong>();  //!<  Domain guardian stack

        public ulong    Len     { get { return length; } set { length = value; } }
        public ID       Type    { get { return type; } set { type = value; } }

        public LWFile(string name)
        {
            var file = new FileStream(name, FileMode.Open);
            stream = new BufferedStream(file);
        }

        public void Close()
        {
            stream.Close();
        }

        public void  Push(ulong len)
        {
            stack.Push(bytesRead + len);
        }
        public ulong Pop()
        {
            return Pop(false);
        }
        public ulong Pop(bool skip_rest)
        {
            if(skip_rest)
            {
                ulong top = stack.Peek();
                while(bytesRead < top)
                {
                    ReadU1();
                }
            }
            return stack.Pop() - bytesRead;
        }
        public ulong Left()
        {
            return stack.Peek() - bytesRead;
        }
        public ulong BytesRead()
        {
            return bytesRead;
        }

        public ID ReadID4()
        {
            return new ID(ReadU4());
        }
#if false
            if(stack.Peek() >= (bytes_read + 4))
            {
                ID4 data = is.read_long();
                bytes_read += 4;
                return data;
            }else{
                return 0;
            }
#endif
        private int Byte()
        {
            int c = stream.ReadByte();
            if(c == -1)
            {
                throw new System.IO.EndOfStreamException();
            }
            ++bytesRead;
            return c;
        }
        public I1 ReadI1()
        {
            if(stack.Peek() >= bytesRead + 1)
            {
                return (I1)Byte();
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (I1)0;
            }
        }
        public I2 ReadI2()
        {
            if(stack.Peek() >= bytesRead + 2)
            {
                byte c1 = (byte)Byte();
                byte c2 = (byte)Byte();
                return (I2)((c1 << 8) | c2);
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (I2)0;
            }
        }
        public I4 ReadI4()
        {
            if(stack.Peek() >= bytesRead + 4)
            {
                byte c1 = (byte)Byte();
                byte c2 = (byte)Byte();
                byte c3 = (byte)Byte();
                byte c4 = (byte)Byte();
                return (I4)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (I4)0;
            }
        }
        public U1 ReadU1()
        {
            if(stack.Peek() >= bytesRead + 1)
            {
                return (U1)Byte();
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (U1)0;
            }
        }
        public U2 ReadU2()
        {
            if(stack.Peek() >= bytesRead + 2)
            {
                byte c1 = (byte)Byte();
                byte c2 = (byte)Byte();
                return (U2)((c1 << 8) | c2);
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (U2)0;
            }
        }
        public U4 ReadU4() 
        {
            if(stack.Peek() >= bytesRead + 4)
            {
                byte c1 = (byte)Byte();
                byte c2 = (byte)Byte();
                byte c3 = (byte)Byte();
                byte c4 = (byte)Byte();
                return (U4)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (U4)0;
            }
        }
        public F4 ReadF4()
        {
            if(stack.Peek() >= bytesRead + 4)
            {
#if false
                byte[] c = new byte[4];
                c[0] = (byte)Byte();
                c[1] = (byte)Byte();
                c[2] = (byte)Byte();
                c[3] = (byte)Byte();
#else
                U4 u = ReadU4();
                byte[] c = System.BitConverter.GetBytes(u);
                F4 f = System.BitConverter.ToSingle(c, 0);
#endif
                return f;
            }
            else
            {
                throw new System.IO.EndOfStreamException();
                //return (F4)0.0f;
            }
        }

        /*  String  S0

            Names or other character strings are written as a series
            of ASCII character values followed by a zero (or null) byte. 
            If the length of the string including the null terminating
            byte is odd, an extra null is added so that the data that 
            follows will begin on an even byte boundary.
        */
        public S0 ReadS0()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for(;;)
            {
                if(stack.Peek() < (bytesRead + 1))
                {
                    break;
                }
                char b = (char)Byte();
                if(b == 0)
                {
                    break;
                }
                sb.Append(b);
            }

            if((bytesRead & 1) != 0)
            {
                Byte();
            }

            return sb.ToString();
        }

        /*  Variable-length Index  VX ::= index[U2] | (index + 0xFF000000)[U4]

            This is an index into an array of items (points or polygons), 
            or a collection of items each uniquely identified by an integer
            (clips or envelopes). A VX is written as a variable length 2- or 
            4-byte element. If the index value is less than 65,280 (0xFF00), 
            then the index is written as an unsigned two-byte integer. Otherwise 
            the index is written as an unsigned four byte integer with bits 
            24-31 set. When reading an index, if the first byte encountered is 
            255 (0xFF), then the four-byte form is being used and the first 
            byte should be discarded or masked out.
        */
        public VX ReadVX()
        {
            U2 data = ReadU2();

            if((data & 0xff00) == 0xff00)
            {
                U2 data2 = ReadU2();
                return (VX)((data & 0x00ff) << 16) | data2;
            }
            else
            {
                return data;
            }
        }
        public COL4  ReadCOL4()
        {
            float red   = (float)(ReadI1() / 255.0f);
            float green = (float)(ReadI1() / 255.0f);
            float blue  = (float)(ReadI1() / 255.0f);
            return new COL4(red, green, blue, 1.0f);
        }
        public COL12 ReadCOL12()
        {
            float r = ReadF4();
            float g = ReadF4();
            float b = ReadF4();
            return new COL12(r, g, b);
        }
        public VEC12 ReadVEC12()
        {
            float x = ReadF4();
            float y = ReadF4();
            float z = ReadF4();
            return new VEC12(x, y, -z);
        }
        public FP4 ReadFP4()
        {
            return ReadF4();
        }
        public ANG4 ReadANG4()
        {
            return ReadF4();
        }
        public FNAM0 ReadFNAM0()
        {
            return ReadS0();
        }
    }
}
