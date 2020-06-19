using System.Collections.Generic;
using System.Diagnostics;

using RenderStack.Math;

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
using COL12 = RenderStack.Math.Vector4;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public partial class LWModelParser
    {
        private LWModel         model;
        private LWFile          f;
        private LWLayer         currentLayer;
        private LWSurface       currentSurface;
        private LWTexture       currentTexture;
        private LWTextureStack  currentTextureStack;
        private LWClip          currentClip; // = new LWClip();
        private LWEnvelope      currentEnvelope;
        private LWEnvelopeKey   currentkey;

        public LWModel          Model { get { return model; } }

        public static LWModel Load(string fname)
        {
            var parser = new LWModelParser(fname);
            return parser.Model;
        }

        private LWModelParser(string fname)
        {
            string final_file_name = fname; //fix_file_name( "Data/Objects/", fname.c_str() );
            model = new LWModel();

#if !DEBUG
            try
#endif
            {
                f = new LWFile(final_file_name);

                f.Push(8);                  // Root domain only allows reading of first 8 bytes of FORM and length
                ID form = f.ReadID4();      // FORM
                f.Len = f.ReadU4();
                f.Push(f.Len);              // file length-8; File data domain allows reading of rest of the file
                f.Type = f.ReadID4();       // LWOB, LWLO, LWO2

                if(f.Type.value == ID.LWOB)
                {
                    currentLayer = model.MakeLayer(0, "", 0, Vector3.Zero, -1);
                }

                while(f.Left() > 0)
                /*f.BytesRead() - 8 < f.Len*/
                {
                    LAYRchunk();
                }
            }
#if !DEBUG
            catch(System.Exception)
            {
                throw;
            }
#endif

            f.Close();
        }

        /*  LWO2 Tag Strings 

            TAGS { tag-string[S0] * }

            This chunk lists the tags strings that can be associated with polygons by the PTAG chunk.
            Strings should be read until as many bytes as the chunk size specifies have been read, and
            each string is assigned an index starting from zero. 
        */
        void tags_d()
        {
            while(f.Left() > 0)
            {
                model.Tags.Add(f.ReadS0());
            }
        }
    }
}
