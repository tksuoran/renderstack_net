using System;
using System.Runtime.InteropServices;
using RenderStack.Math;

namespace RenderStack.Graphics
{
    public interface IParams
    {
        bool Dirty      { get; set; }
        int  Elements   { get; }
        int  Dimension  { get; }

        bool IsCompatibleWith(object p);
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Params : IParams
    {
        private int elements;
        private int dimension;
        protected bool dirty = true; 

        public Params(int elements, int dimension)
        {
            this.elements = elements;
            this.dimension = dimension;
        }

        public bool Dirty       { get { return dirty; } set { dirty = value; } }
        public int  Elements    { get { return elements; } }
        public int  Dimension   { get { return dimension; } }

        public bool IsCompatibleWith(object o)
        {
            Params other = o as Params;
            if(o == null)
            {
                return false;
            }
            if(other.Dimension != this.Dimension)
            {
                return false;
            }
            if(other.Elements != this.Elements)
            {
                return false;
            }
            return true;
        }
    }
    public interface IParams<T> : IParams, IComparable where T : IEquatable<T>, IComparable<T>, IComparable
    {
        int  Index  { get; set; }
        T[]  Value  { get; }
        T    X      { get; set; }
        T    Y      { get; set; }
        T    Z      { get; set; }
        T    W      { get; set; }

        T this[int index, int component]{ get; set; }
        T this[int index]{ get; set; }

        void Set(T x);
        void Set(T x, T y);
        void Set(T x, T y, T z);
        void Set(T x, T y, T z, T w);
        void SetI(int index, T x);
        void SetI(int index, T x, T y);
        void SetI(int index, T x, T y, T z);
        void SetI(int index, T x, T y, T z, T w);

        void CopyFrom(IUniformValue source);
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public abstract class Params<T> : Params, IParams<T>, IComparable where T : IEquatable<T>, IComparable<T>, IComparable
    {
        private T[] value;

        public int  Index { get; set; }
        public T[]  Value { get { return value; } }
        public T    X { get { return Value[0]; } set { if(!Value[0].Equals(value)){ Value[0] = value; dirty = true; } } }
        public T    Y { get { return Value[1]; } set { if(!Value[1].Equals(value)){ Value[1] = value; dirty = true; } } }
        public T    Z { get { return Value[2]; } set { if(!Value[2].Equals(value)){ Value[2] = value; dirty = true; } } }
        public T    W { get { return Value[3]; } set { if(!Value[3].Equals(value)){ Value[3] = value; dirty = true; } } }

        public T this[int index, int component]
        {
            get
            {
                return Value[index * Elements + component];
            }
            set
            {
                if(!Value[index * Elements + component].Equals(value))
                {
                    Value[index * Elements + component] = value;
                    dirty = true;
                }
            }
        }
        public T this[int index]
        {
            get 
            { 
                return Value[index]; 
            }
            set
            { 
                if(!Value[index].Equals(value))
                {
                    Value[index] = value;
                    dirty = true;
                }
            }
        }

        public Params():base(1, 1)
        {
            value = new T[1 * 1];
        }

        public Params(int elements, int dimension):base(elements, dimension)
        {
            value = new T[dimension * elements];
        }
        public Params(T x):base(1, 1)
        {
            value = new T[1 * 1];
            X = x;
        }
        public Params(T x, T y):base(2, 1)
        {
            value = new T[1 * 2];
            X = x;
            Y = y;
        }
        public Params(T x, T y, T z):base(3, 1)
        {
            value = new T[1 * 3];
            X = x;
            Y = y;
            Z = z;
        }
        public Params(T x, T y, T z, T w):base(4, 1)
        {
            value = new T[1 * 4];
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Set(T x)
        {
            X = x;
        }
        public void Set(T x, T y)
        {
            X = x;
            Y = y;
        }
        public void Set(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public void Set(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public void SetI(int index, T x)
        {
            Value[index * Elements + 0] = x;
        }
        public void SetI(int index, T x, T y)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
        }
        public void SetI(int index, T x, T y, T z)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
            Value[index * Elements + 2] = z;
        }
        public void SetI(int index, T x, T y, T z, T w)
        {
            Value[index * Elements + 0] = x;
            Value[index * Elements + 1] = y;
            Value[index * Elements + 2] = z;
            Value[index * Elements + 3] = w;
        }

        public int CompareTo(object o)
        {
            Params<T> other = (Params<T>)o;
            for(int i = 0; i < Value.Length; ++i)
            {
                if(Value[i].CompareTo(other.Value[i]) < 0)
                {
                    return -1;
                }
                if(Value[i].CompareTo(other.Value[i]) > 0)
                {
                    return 1;
                }
            }
            return 0;
        }
        public void CopyFrom(IUniformValue source)
        {
            Params<T> other = (Params<T>)source;
            for(int i = 0; i < Value.Length; ++i)
            {
                Value[i] = other.Value[i];
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Ints : Params<int>, IParams<int>, IUniformValue
    {
        public IUniformValue GetUninitialized()
        {
            Ints clone = new Ints(Elements, Dimension);
            for(int i = 0; i < Value.Length; ++i)
            {
                clone.Value[i] = int.MaxValue;
            }
            return clone;
        }
        public Ints():base(1,1){}
        public Ints(int elements, int dimension):base(elements, dimension){}
        public Ints(int x):base(x){}
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class UInts : Params<uint>, IParams<uint>, IUniformValue
    {
        public IUniformValue GetUninitialized()
        {
            UInts clone = new UInts(Elements, Dimension);
            for(int i = 0; i < Value.Length; ++i)
            {
                clone.Value[i] = uint.MaxValue;
            }
            return clone;
        }
        public UInts():base(1,1){}
        public UInts(int elements, int dimension):base(elements, dimension){}
        public UInts(uint x):base(x){}
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Floats : Params<float>, IUniformValue
    {
        public IUniformValue GetUninitialized()
        {
            Floats clone = new Floats(Elements, Dimension);
            for(int i = 0; i < Value.Length; ++i)
            {
                clone.Value[i] = float.MaxValue;
            }
            return clone;
        }
        public Floats():base(1,1){}
        public Floats(int elements, int dimension):base(elements, dimension){}
        public Floats(float x):base(x){}
        public Floats(float x, float y):base(x,y){}
        public Floats(float x, float y, float z):base(x,y,z){}
        public Floats(float x, float y, float z, float w):base(x,y,z,w){}
        public void Set(Vector2 v)
        {
            if(Elements != 2)
            {
                throw new ArgumentException();
            }
            X = v.X;
            Y = v.Y;
        }
        public void Set(Vector3 v)
        {
            if(Elements != 3)
            {
                throw new ArgumentException();
            }
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }
        public void Set(Vector4 v)
        {
            if(Elements != 4)
            {
                throw new ArgumentException();
            }
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }
        public void SetI(int index, Vector2 v)
        {
            if(Elements != 2)
            {
                throw new ArgumentException();
            }
            if(!Value[index * Elements + 0].Equals(v.X)) { Value[index * Elements + 0] = v.X; dirty = true; }
            if(!Value[index * Elements + 1].Equals(v.Y)) { Value[index * Elements + 1] = v.Y; dirty = true; }
        }
        public void SetI(int index, Vector3 v)
        {
            if(Elements != 3)
            {
                throw new ArgumentException();
            }
            if(!Value[index * Elements + 0].Equals(v.X)) { Value[index * Elements + 0] = v.X; dirty = true; }
            if(!Value[index * Elements + 1].Equals(v.Y)) { Value[index * Elements + 1] = v.Y; dirty = true; }
            if(!Value[index * Elements + 2].Equals(v.Z)) { Value[index * Elements + 2] = v.Z; dirty = true; }
        }
        public void SetI(int index, Vector4 v)
        {
            if(Elements != 4)
            {
                throw new ArgumentException();
            }
            if(!Value[index * Elements + 0].Equals(v.X)) { Value[index * Elements + 0] = v.X; dirty = true; }
            if(!Value[index * Elements + 1].Equals(v.Y)) { Value[index * Elements + 1] = v.Y; dirty = true; }
            if(!Value[index * Elements + 2].Equals(v.Z)) { Value[index * Elements + 2] = v.Z; dirty = true; }
            if(!Value[index * Elements + 3].Equals(v.W)) { Value[index * Elements + 3] = v.W; dirty = true; }
        }
        public void SetI(Matrix4 m)
        {
            SetI(0, m);
        }
        public void SetI(int index, Matrix4 m)
        {
            if(Elements != 16)
            {
                throw new ArgumentException("Set(Matrix4) with Elements != 16");
            }

#if false
            if(!Value[index * Elements +  0].Equals(m._00)) { Value[index * Elements +  0] = m._00; dirty = true; }
            if(!Value[index * Elements +  1].Equals(m._10)) { Value[index * Elements +  1] = m._10; dirty = true; }
            if(!Value[index * Elements +  2].Equals(m._20)) { Value[index * Elements +  2] = m._20; dirty = true; }
            if(!Value[index * Elements +  3].Equals(m._30)) { Value[index * Elements +  3] = m._30; dirty = true; }
            if(!Value[index * Elements +  4].Equals(m._01)) { Value[index * Elements +  4] = m._01; dirty = true; }
            if(!Value[index * Elements +  5].Equals(m._11)) { Value[index * Elements +  5] = m._11; dirty = true; }
            if(!Value[index * Elements +  6].Equals(m._21)) { Value[index * Elements +  6] = m._21; dirty = true; }
            if(!Value[index * Elements +  7].Equals(m._31)) { Value[index * Elements +  7] = m._31; dirty = true; }
            if(!Value[index * Elements +  8].Equals(m._02)) { Value[index * Elements +  8] = m._02; dirty = true; }
            if(!Value[index * Elements +  9].Equals(m._12)) { Value[index * Elements +  9] = m._12; dirty = true; }
            if(!Value[index * Elements + 10].Equals(m._22)) { Value[index * Elements + 10] = m._22; dirty = true; }
            if(!Value[index * Elements + 11].Equals(m._32)) { Value[index * Elements + 11] = m._32; dirty = true; }
            if(!Value[index * Elements + 12].Equals(m._03)) { Value[index * Elements + 12] = m._03; dirty = true; }
            if(!Value[index * Elements + 13].Equals(m._13)) { Value[index * Elements + 13] = m._13; dirty = true; }
            if(!Value[index * Elements + 14].Equals(m._23)) { Value[index * Elements + 14] = m._23; dirty = true; }
            if(!Value[index * Elements + 15].Equals(m._33)) { Value[index * Elements + 15] = m._33; dirty = true; }
#else
            Value[index * Elements +  0] = m._00;
            Value[index * Elements +  1] = m._10;
            Value[index * Elements +  2] = m._20;
            Value[index * Elements +  3] = m._30;
            Value[index * Elements +  4] = m._01;
            Value[index * Elements +  5] = m._11;
            Value[index * Elements +  6] = m._21;
            Value[index * Elements +  7] = m._31;
            Value[index * Elements +  8] = m._02;
            Value[index * Elements +  9] = m._12;
            Value[index * Elements + 10] = m._22;
            Value[index * Elements + 11] = m._32;
            Value[index * Elements + 12] = m._03;
            Value[index * Elements + 13] = m._13;
            Value[index * Elements + 14] = m._23;
            Value[index * Elements + 15] = m._33;
            dirty = true;
#endif
        }
        public Matrix4 Get(int index)
        {
            Matrix4 m;
            if(Elements != 16)
            {
                throw new ArgumentException("Set(Matrix4) with Elements != 16");
            }

            m._00 = Value[index * Elements +  0];
            m._10 = Value[index * Elements +  1];
            m._20 = Value[index * Elements +  2];
            m._30 = Value[index * Elements +  3];
            m._01 = Value[index * Elements +  4];
            m._11 = Value[index * Elements +  5];
            m._21 = Value[index * Elements +  6];
            m._31 = Value[index * Elements +  7];
            m._02 = Value[index * Elements +  8];
            m._12 = Value[index * Elements +  9];
            m._22 = Value[index * Elements + 10];
            m._32 = Value[index * Elements + 11];
            m._03 = Value[index * Elements + 12];
            m._13 = Value[index * Elements + 13];
            m._23 = Value[index * Elements + 14];
            m._33 = Value[index * Elements + 15];

            return m;
        }

        #region IValueProvider Members

        public object GetValue(object target)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var v in Value)
            {
                sb.Append(v.ToString());
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /*public void SetValue(object target, object value)
        {
            string[] array = (value as string).Split(' ');
            elements = array.Length;
            dimension = 1;
            dirty = true;
            this.Value = new float[Elements];
            for(int i = 0; i < Elements; ++i)
            {
                Value[i] = float.Parse(array[i]);
            }
        }*/

        #endregion
    }

    public class MultiInts : IParams<int>, IUniformValue
    {
        private Ints              first;
        private readonly Ints[]   array;

        #region IParams
        public bool     Dirty       { get { return first.Dirty; } set { foreach(var f in array) f.Dirty = value; } }
        public int      Elements    { get { return first.Elements; } }
        public int      Dimension   { get { return first.Dimension; } }

        public bool     IsCompatibleWith(object o){ return first.IsCompatibleWith(o); } // also IUniformValue
        #endregion
        #region IParams<float>
        public int      Index       { get; set; } // also IUniformValue
        public int[]    Value       { get { return first.Value; } }

        public int      X           { get { return first.X; } set { foreach(var f in array) f.X = value; } }
        public int      Y           { get { return first.Y; } set { foreach(var f in array) f.Y = value; } }
        public int      Z           { get { return first.Z; } set { foreach(var f in array) f.Z = value; } }
        public int      W           { get { return first.W; } set { foreach(var f in array) f.W = value; } }

        public int      this[int index, int component]{ get { return first[index, component]; } set { foreach(var f in array) f[index, component] = value; } }
        public int      this[int index]{ get { return first[index]; } set { foreach(var f in array) f[index] = value; } }

        public void Set(int x){ foreach(var f in array)f.Set(x); }
        public void Set(int x, int y){ foreach(var f in array)f.Set(x, y); }
        public void Set(int x, int y, int z){ foreach(var f in array)f.Set(x, y, z); }
        public void Set(int x, int y, int z, int w){ foreach(var f in array)f.Set(x, y, z, w); }
        public void SetI(int index, int x){ foreach(var f in array)f.SetI(index, x); }
        public void SetI(int index, int x, int y){ foreach(var f in array)f.SetI(index, x, y); }
        public void SetI(int index, int x, int y, int z){ foreach(var f in array)f.SetI(index, x, y, z); }
        public void SetI(int index, int x, int y, int z, int w){ foreach(var f in array)f.SetI(index, x, y, z, w); }

        public int CompareTo(object o){ return first.CompareTo(o); }
        public void CopyFrom(IUniformValue source){ foreach(var f in array)f.CopyFrom(source); } // also IUniformValue
        #endregion
        #region IUniformValue
        public IUniformValue GetUninitialized(){ return first.GetUninitialized(); } 
        #endregion

        public MultiInts(string name, Ints @default, params IUniformBuffer[] buffers)
        {
            if(buffers.Length > 0)
            {
                this.array = new Ints[buffers.Length];
                for(int i = 0; i < buffers.Length; ++i)
                {
                    array[i] = 
                        (buffers[i] != null) && (buffers[i].Contains(name)) 
                        ? buffers[i].Ints(name)
                        : @default;
                }
            }
            else
            {
                this.array = new Ints[1];
                array[0] = @default;
            }
            first = array[0];
        }
#if false
        public void Set(IVector2 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void Set(IVector3 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void Set(Vector4 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void SetI(int index, Vector2 v)
        {
            foreach(var f in floats) f.Set(index, v);
        }
        public void SetI(int index, Vector3 v)
        {
            foreach(var f in floats) f.Set(index, v);
        }
        public void SetI(int index, Vector4 v)
        {
            foreach(var f in floats) f.Set(index, v);
        }
        public void Set(Matrix4 m)
        {
            foreach(var f in floats) f.Set(m);
        }
        public void SetI(int index, Matrix4 m)
        {
            foreach(var f in floats) f.Set(index, m);
        }
        public Matrix4 Get(int index)
        {
            return first.Get(index);
        }
#endif
    }

    public class MultiFloats : IParams<float>, IUniformValue
    {
        private Floats              first;
        private readonly Floats[]   floats;

        #region IParams
        public bool     Dirty       { get { return first.Dirty; } set { foreach(var f in floats) f.Dirty = value; } }
        public int      Elements    { get { return first.Elements; } }
        public int      Dimension   { get { return first.Dimension; } }

        public bool     IsCompatibleWith(object o){ return first.IsCompatibleWith(o); } // also IUniformValue
        #endregion
        #region IParams<float>
        public int      Index       { get; set; } // also IUniformValue
        public float[]  Value       { get { return first.Value; } }

        public float    X           { get { return first.X; } set { foreach(var f in floats) f.X = value; } }
        public float    Y           { get { return first.Y; } set { foreach(var f in floats) f.Y = value; } }
        public float    Z           { get { return first.Z; } set { foreach(var f in floats) f.Z = value; } }
        public float    W           { get { return first.W; } set { foreach(var f in floats) f.W = value; } }

        public float    this[int index, int component]{ get { return first[index, component]; } set { foreach(var f in floats) f[index, component] = value; } }
        public float    this[int index]{ get { return first[index]; } set { foreach(var f in floats) f[index] = value; } }

        public void Set(float x){ foreach(var f in floats)f.Set(x); }
        public void Set(float x, float y){ foreach(var f in floats)f.Set(x, y); }
        public void Set(float x, float y, float z){ foreach(var f in floats)f.Set(x, y, z); }
        public void Set(float x, float y, float z, float w){ foreach(var f in floats)f.Set(x, y, z, w); }
        public void SetI(int index, float x){ foreach(var f in floats)f.SetI(index, x); }
        public void SetI(int index, float x, float y){ foreach(var f in floats)f.SetI(index, x, y); }
        public void SetI(int index, float x, float y, float z){ foreach(var f in floats)f.SetI(index, x, y, z); }
        public void SetI(int index, float x, float y, float z, float w){ foreach(var f in floats)f.SetI(index, x, y, z, w); }

        public int CompareTo(object o){ return first.CompareTo(o); }
        public void CopyFrom(IUniformValue source){ foreach(var f in floats)f.CopyFrom(source); } // also IUniformValue
        #endregion
        #region IUniformValue
        public IUniformValue GetUninitialized(){ return first.GetUninitialized(); } 
        #endregion

        public MultiFloats(string name, Floats @default, params IUniformBuffer[] buffers)
        {
            if(buffers.Length > 0)
            {
                this.floats = new Floats[buffers.Length];
                for(int i = 0; i < buffers.Length; ++i)
                {
                    floats[i] = 
                        (buffers[i] != null) && (buffers[i].Contains(name)) 
                        ? buffers[i].Floats(name)
                        : @default;
                }
            }
            else
            {
                this.floats = new Floats[1];
                floats[0] = @default;
            }
            first = floats[0];
        }
        public void Set(Vector2 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void Set(Vector3 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void Set(Vector4 v)
        {
            foreach(var f in floats) f.Set(v);
        }
        public void SetI(int index, Vector2 v)
        {
            foreach(var f in floats) f.SetI(index, v);
        }
        public void SetI(int index, Vector3 v)
        {
            foreach(var f in floats) f.SetI(index, v);
        }
        public void SetI(int index, Vector4 v)
        {
            foreach(var f in floats) f.SetI(index, v);
        }
        public void Set(Matrix4 m)
        {
            foreach(var f in floats) f.SetI(m);
        }
        public void SetI(int index, Matrix4 m)
        {
            foreach(var f in floats) f.SetI(index, m);
        }
        public Matrix4 Get(int index)
        {
            return first.Get(index);
        }
    }

#if false
    public class Parameter
    {
        public Parameter(string name, Floats @default, IUniformBuffer bufferGL, IUniformBuffer bufferRL)
        {
            ValueGL = (bufferGL != null) && bufferGL.Contains(name) ? bufferGL.Floats(name) : @default;
            ValueRL = (bufferRL != null) && bufferRL.Contains(name) ? bufferRL.Floats(name) : @default;
        }
        public float[] ReadOnlyValue { get { return ValueGL.Value; } }
        public void Set(Matrix4 value)
        {
            ValueGL.Set(value);
            ValueRL.Set(value);
        }
        public void Set(int index, Matrix4 value)
        {
            ValueGL.Set(index, value);
            ValueRL.Set(index, value);
        }
        public void Set(Vector4 value)
        {
            ValueGL.Set(value);
            ValueRL.Set(value);
        }
        public void Set(float x)
        {
            ValueGL.Set(x);
            ValueRL.Set(x);
        }
        public void Set(float x, float y)
        {
            ValueGL.Set(x, y);
            ValueRL.Set(x, y);
        }
        public void Set(int index, float x, float y)
        {
            ValueGL.Set(index, x, y);
            ValueRL.Set(index, x, y);
        }
        public void Set(float x, float y, float z)
        {
            ValueGL.Set(x, y, z);
            ValueRL.Set(x, y, z);
        }
        public void Set(int index, float x, float y, float z)
        {
            ValueGL.Set(index, x, y, z);
            ValueRL.Set(index, x, y, z);
        }
        public void Set(float x, float y, float z, float w)
        {
            ValueGL.Set(x, y, z, w);
            ValueRL.Set(x, y, z, w);
        }
        public void Set(int index, float x, float y, float z, float w)
        {
            ValueGL.Set(index, x, y, z, w);
            ValueRL.Set(index, x, y, z, w);
        }
        public Matrix4 Get(int index)
        {
            return ValueGL.Get(index);
        }

        public Floats ValueGL;
        public Floats ValueRL;
    }
#endif
}

