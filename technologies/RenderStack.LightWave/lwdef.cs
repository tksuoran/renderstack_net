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
    public class Constants
    {
        public readonly Vector3[] WireColors = 
        {
            new Vector3(  0.0f / 255.0f,   0.0f / 255.0f,   0.0f / 255.0f),
            new Vector3(  0.0f / 255.0f,  48.0f / 255.0f, 128.0f / 255.0f),
            new Vector3(  0.0f / 255.0f,  96.0f / 255.0f,   0.0f / 255.0f),
            new Vector3( 32.0f / 255.0f,  96.0f / 255.0f, 112.0f / 255.0f),
            new Vector3(112.0f / 255.0f,   0.0f / 255.0f,   0.0f / 255.0f),
            new Vector3(112.0f / 255.0f,  32.0f / 255.0f, 112.0f / 255.0f),
            new Vector3(112.0f / 255.0f,  80.0f / 255.0f,   0.0f / 255.0f),
            new Vector3(176.0f / 255.0f, 176.0f / 255.0f, 176.0f / 255.0f),
            new Vector3( 32.0f / 255.0f, 160.0f / 255.0f, 240.0f / 255.0f),
            new Vector3( 32.0f / 255.0f, 224.0f / 255.0f,  32.0f / 255.0f),
            new Vector3( 96.0f / 255.0f, 224.0f / 255.0f, 240.0f / 255.0f),
            new Vector3(240.0f / 255.0f,  32.0f / 255.0f,  32.0f / 255.0f),
            new Vector3(240.0f / 255.0f,  96.0f / 255.0f, 240.0f / 255.0f),
            new Vector3(240.0f / 255.0f, 192.0f / 255.0f,  32.0f / 255.0f),
            new Vector3(240.0f / 255.0f, 240.0f / 255.0f, 240.0f / 255.0f),
            new Vector3(255.0f / 255.0f, 255.0f / 255.0f,   0.0f / 255.0f)
        };

        //  LWOB Surface flags
        public const int LW_SF_LUMINOUS          = (1<< 0);
        public const int LW_SF_OUTLINE           = (1<< 1);
        public const int LW_SF_SMOOTHING         = (1<< 2);
        public const int LW_SF_COLOR_HILIGHTS    = (1<< 3);
        public const int LW_SF_COLOR_FILTER      = (1<< 4);
        public const int LW_SF_OPAQUE_EDGE       = (1<< 5);
        public const int LW_SF_TRANSPARENT_EDGE  = (1<< 6);
        public const int LW_SF_SHARP_TERMINATOR  = (1<< 7);
        public const int LW_SF_DOUBLE_SIDED      = (1<< 8);
        public const int LW_SF_ADDITIVE          = (1<< 9);
        public const int LW_SF_SHADOW_ALPHA      = (1<<10);

        public const int LW_PLANAR_IMAGE_MAP      = 1;
        public const int LW_CYLINDRICAL_IMAGE_MAP = 2;
        public const int LW_SPHERICAL_IMAGE_MAP   = 3;
        public const int LW_CUBIC_IMAGE_MAP       = 4;

        public const int LW_AXIS_X = 0;
        public const int LW_AXIS_Y = 1;
        public const int LW_AXIS_Z = 2;

        public const int LW_ALPHA_MODE_DISABLED   = 0;
        public const int LW_ALPHA_MODE_CONSTANT   = 1;
        public const int LW_ALPHA_MODE_OPACITY    = 2; //  default
        public const int LW_ALPHA_MODE_SHADOW     = 3;

        //  LWO2::BLOK::OPAC
        public const int LW_OPACITY_TYPE_ADDITIVE             = 0;
        public const int LW_OPACITY_TYPE_SUBSTRACTIVE         = 1;
        public const int LW_OPACITY_TYPE_DIFFERENCE           = 2;
        public const int LW_OPACITY_TYPE_MULTIPLY             = 3;
        public const int LW_OPACITY_TYPE_DIVIDE               = 4;
        public const int LW_OPACITY_TYPE_ALPHA                = 5;
        public const int LW_OPACITY_TYPE_TEXTURE_DISPLACEMENT = 6;

        //  LWO2::BLOK::PROJ
        public const int LW_PROJECTION_PLANAR      = 0;
        public const int LW_PROJECTION_CYLINDRICAL = 1;
        public const int LW_PROJECTION_SPHERICAL   = 2;
        public const int LW_PROJECTION_CUBIC       = 3;
        public const int LW_PROJECTION_FRONT       = 4;
        public const int LW_PROJECTION_UV          = 5;

        //  LWO2::BLOK_WRAP
        public const int LW_WRAP_RESET  = 0;
        public const int LW_WRAP_REPEAT = 1;
        public const int LW_WRAP_MIRROR = 2;
        public const int LW_WRAP_EDGE   = 3;
    }

    public class ID
    {
        public System.UInt32 value;
        public ID(System.UInt32 value)
        {
            this.value = value;
        }
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char a = (char)((value & 0xff000000)>>24);
            char b = (char)((value & 0x00ff0000)>>16);
            char c = (char)((value & 0x0000ff00)>> 8);
            char d = (char)((value & 0x000000ff)    );
            sb.Append(a);
            sb.Append(b);
            sb.Append(c);
            sb.Append(d);
            return sb.ToString();
        }

        //  File format
        public const System.UInt32 FORM = (U4)('F' )<<24|(U4)( 'O' )<<16|(U4)( 'R' )<<8|( 'M');
        public const System.UInt32 LWOB = (U4)('L' )<<24|(U4)( 'W' )<<16|(U4)( 'O' )<<8|( 'B');  //  LWOB
        public const System.UInt32 LWLO = (U4)('L' )<<24|(U4)( 'W' )<<16|(U4)( 'L' )<<8|( 'O');  //  LWLO
        public const System.UInt32 LWO2 = (U4)('L' )<<24|(U4)( 'W' )<<16|(U4)( 'O' )<<8|( '2');

        //  Primary chunk
        public const System.UInt32 LAYR = (U4)('L' )<<24|(U4)( 'A' )<<16|(U4)( 'Y' )<<8|( 'R');
        public const System.UInt32 PNTS = (U4)('P' )<<24|(U4)( 'N' )<<16|(U4)( 'T' )<<8|( 'S');
        public const System.UInt32 VMAP = (U4)('V' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'P');
        public const System.UInt32 VMAD = (U4)('V' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'D');
        public const System.UInt32 POLS = (U4)('P' )<<24|(U4)( 'O' )<<16|(U4)( 'L' )<<8|( 'S');

        public const System.UInt32 CRVS = (U4)('C' )<<24|(U4)( 'R' )<<16|(U4)( 'V' )<<8|( 'S');  // LWOB only
        public const System.UInt32 PCHS = (U4)('P' )<<24|(U4)( 'C' )<<16|(U4)( 'H' )<<8|( 'S');  // LWOB only
        public const System.UInt32 SRFS = (U4)('S' )<<24|(U4)( 'R' )<<16|(U4)( 'F' )<<8|( 'S');  // LWOB only
        public const System.UInt32 VLUM = (U4)('V' )<<24|(U4)( 'L' )<<16|(U4)( 'U' )<<8|( 'M');  // LWOB only
        public const System.UInt32 VDIF = (U4)('V' )<<24|(U4)( 'D' )<<16|(U4)( 'I' )<<8|( 'F');  // LWOB only
        public const System.UInt32 VSPC = (U4)('V' )<<24|(U4)( 'S' )<<16|(U4)( 'P' )<<8|( 'C');  // LWOB only
        public const System.UInt32 VRFL = (U4)('V' )<<24|(U4)( 'R' )<<16|(U4)( 'F' )<<8|( 'L');  // LWOB only
        public const System.UInt32 VTRN = (U4)('V' )<<24|(U4)( 'T' )<<16|(U4)( 'R' )<<8|( 'N');  // LWOB only

        public const System.UInt32 RFLT = (U4)('R' )<<24|(U4)( 'F' )<<16|(U4)( 'L' )<<8|( 'T');  // LWOB only
        public const System.UInt32 EDGE = (U4)('E' )<<24|(U4)( 'D' )<<16|(U4)( 'G' )<<8|( 'E');  // LWOB only
        public const System.UInt32 SDAT = (U4)('S' )<<24|(U4)( 'D' )<<16|(U4)( 'A' )<<8|( 'T');  // LWOB only
        public const System.UInt32 IMSQ = (U4)('I' )<<24|(U4)( 'M' )<<16|(U4)( 'S' )<<8|( 'Q');  // LWOB only
        public const System.UInt32 FLYR = (U4)('F' )<<24|(U4)( 'L' )<<16|(U4)( 'Y' )<<8|( 'R');  // LWOB only
        public const System.UInt32 IMCC = (U4)('I' )<<24|(U4)( 'M' )<<16|(U4)( 'C' )<<8|( 'C');  // LWOB only
        public const System.UInt32 CTEX = (U4)('C' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 DTEX = (U4)('D' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 STEX = (U4)('S' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 RTEX = (U4)('R' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 TTEX = (U4)('T' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 LTEX = (U4)('L' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only
        public const System.UInt32 BTEX = (U4)('B' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'X');  // LWOB only

        public const System.UInt32 TFLG = (U4)('T' )<<24|(U4)( 'F' )<<16|(U4)( 'L' )<<8|( 'G');  // LWOB only
        public const System.UInt32 TSIZ = (U4)('T' )<<24|(U4)( 'S' )<<16|(U4)( 'I' )<<8|( 'Z');  // LWOB only
        public const System.UInt32 TCTR = (U4)('T' )<<24|(U4)( 'C' )<<16|(U4)( 'T' )<<8|( 'R');  // LWOB only
        public const System.UInt32 TFAL = (U4)('T' )<<24|(U4)( 'F' )<<16|(U4)( 'A' )<<8|( 'L');  // LWOB only
        public const System.UInt32 TVEL = (U4)('T' )<<24|(U4)( 'V' )<<16|(U4)( 'E' )<<8|( 'L');  // LWOB only
        public const System.UInt32 TREF = (U4)('T' )<<24|(U4)( 'R' )<<16|(U4)( 'E' )<<8|( 'F');  // LWOB only
        public const System.UInt32 TCLR = (U4)('T' )<<24|(U4)( 'C' )<<16|(U4)( 'L' )<<8|( 'R');  // LWOB only
        public const System.UInt32 TVAL = (U4)('T' )<<24|(U4)( 'V' )<<16|(U4)( 'A' )<<8|( 'L');  // LWOB only
        public const System.UInt32 TFP  = (U4)('T' )<<24|(U4)( 'F' )<<16|(U4)( 'P' )<<8|( ' ');  // LWOB only
        public const System.UInt32 TIP  = (U4)('T' )<<24|(U4)( 'I' )<<16|(U4)( 'P' )<<8|( ' ');  // LWOB only
        public const System.UInt32 TSP  = (U4)('T' )<<24|(U4)( 'S' )<<16|(U4)( 'P' )<<8|( ' ');  // LWOB only
        public const System.UInt32 TFRQ = (U4)('T' )<<24|(U4)( 'F' )<<16|(U4)( 'R' )<<8|( 'Q');  // LWOB only
        public const System.UInt32 TALP = (U4)('T' )<<24|(U4)( 'A' )<<16|(U4)( 'L' )<<8|( 'P');  // LWOB only
        public const System.UInt32 TWRP = (U4)('T' )<<24|(U4)( 'W' )<<16|(U4)( 'R' )<<8|( 'P');  // LWOB only
        public const System.UInt32 TAAS = (U4)('T' )<<24|(U4)( 'A' )<<16|(U4)( 'A' )<<8|( 'S');  // LWOB only
        public const System.UInt32 TOPC = (U4)('T' )<<24|(U4)( 'O' )<<16|(U4)( 'P' )<<8|( 'C');  // LWOB only


        public const System.UInt32 TAGS = (U4)('T' )<<24|(U4)( 'A' )<<16|(U4)( 'G' )<<8|( 'S');
        public const System.UInt32 PTAG = (U4)('P' )<<24|(U4)( 'T' )<<16|(U4)( 'A' )<<8|( 'G');
        public const System.UInt32 ENVL = (U4)('E' )<<24|(U4)( 'N' )<<16|(U4)( 'V' )<<8|( 'L');
        public const System.UInt32 CLIP = (U4)('C' )<<24|(U4)( 'L' )<<16|(U4)( 'I' )<<8|( 'P');
        public const System.UInt32 SURF = (U4)('S' )<<24|(U4)( 'U' )<<16|(U4)( 'R' )<<8|( 'F');
        public const System.UInt32 BBOX = (U4)('B' )<<24|(U4)( 'B' )<<16|(U4)( 'O' )<<8|( 'X');
        public const System.UInt32 DESC = (U4)('D' )<<24|(U4)( 'E' )<<16|(U4)( 'S' )<<8|( 'C');
        public const System.UInt32 TEXT = (U4)('T' )<<24|(U4)( 'E' )<<16|(U4)( 'X' )<<8|( 'T');
        public const System.UInt32 ICON = (U4)('I' )<<24|(U4)( 'C' )<<16|(U4)( 'O' )<<8|( 'N');
        public const System.UInt32 VMPA = (U4)('V' )<<24|(U4)( 'M' )<<16|(U4)( 'P' )<<8|( 'A');

        //  Polygons
        public const System.UInt32 FACE = (U4)('F' )<<24|(U4)( 'A' )<<16|(U4)( 'C' )<<8|( 'E');
        public const System.UInt32 CURV = (U4)('C' )<<24|(U4)( 'U' )<<16|(U4)( 'R' )<<8|( 'V');  // was CRVS
        public const System.UInt32 PRCH = (U4)('P' )<<24|(U4)( 'T' )<<16|(U4)( 'C' )<<8|( 'H');  // was PCHS
        public const System.UInt32 MBAL = (U4)('M' )<<24|(U4)( 'B' )<<16|(U4)( 'A' )<<8|( 'L');
        public const System.UInt32 BONE = (U4)('B' )<<24|(U4)( 'O' )<<16|(U4)( 'N' )<<8|( 'E');

        //  Polygon tags
        public const System.UInt32 BNID = (U4)('B' )<<24|(U4)( 'N' )<<16|(U4)( 'I' )<<8|( 'D');
        public const System.UInt32 SGMP = (U4)('S' )<<24|(U4)( 'G' )<<16|(U4)( 'M' )<<8|( 'P');
        public const System.UInt32 PART = (U4)('P' )<<24|(U4)( 'A' )<<16|(U4)( 'R' )<<8|( 'T');

        //  Image
        public const System.UInt32 STIL = (U4)('S' )<<24|(U4)( 'T' )<<16|(U4)( 'I' )<<8|( 'L');
        public const System.UInt32 ISEQ = (U4)('I' )<<24|(U4)( 'S' )<<16|(U4)( 'E' )<<8|( 'Q');
        public const System.UInt32 ANIM = (U4)('A' )<<24|(U4)( 'N' )<<16|(U4)( 'I' )<<8|( 'M');
        public const System.UInt32 XREF = (U4)('X' )<<24|(U4)( 'R' )<<16|(U4)( 'E' )<<8|( 'F');
        public const System.UInt32 STCC = (U4)('S' )<<24|(U4)( 'T' )<<16|(U4)( 'C' )<<8|( 'C');
        public const System.UInt32 CONT = (U4)('C' )<<24|(U4)( 'O' )<<16|(U4)( 'N' )<<8|( 'T');
        public const System.UInt32 BRIT = (U4)('B' )<<24|(U4)( 'R' )<<16|(U4)( 'I' )<<8|( 'T');
        public const System.UInt32 SATR = (U4)('S' )<<24|(U4)( 'A' )<<16|(U4)( 'T' )<<8|( 'R');
        public const System.UInt32 HUE  = (U4)('H' )<<24|(U4)( 'U' )<<16|(U4)( 'E' )<<8|( ' ');
        public const System.UInt32 GAMM = (U4)('G' )<<24|(U4)( 'A' )<<16|(U4)( 'M' )<<8|( 'M');
        public const System.UInt32 NEGA = (U4)('N' )<<24|(U4)( 'E' )<<16|(U4)( 'G' )<<8|( 'A');
        public const System.UInt32 CROP = (U4)('C' )<<24|(U4)( 'R' )<<16|(U4)( 'O' )<<8|( 'P');
        public const System.UInt32 ALPH = (U4)('A' )<<24|(U4)( 'L' )<<16|(U4)( 'P' )<<8|( 'H');
        public const System.UInt32 COMP = (U4)('C' )<<24|(U4)( 'O' )<<16|(U4)( 'M' )<<8|( 'P');
        public const System.UInt32 IFLT = (U4)('I' )<<24|(U4)( 'F' )<<16|(U4)( 'L' )<<8|( 'T');
        public const System.UInt32 PFLT = (U4)('P' )<<24|(U4)( 'F' )<<16|(U4)( 'L' )<<8|( 'T');
        public const System.UInt32 TIME = (U4)('T' )<<24|(U4)( 'I' )<<16|(U4)( 'M' )<<8|( 'E');
        public const System.UInt32 CLRS = (U4)('C' )<<24|(U4)( 'L' )<<16|(U4)( 'R' )<<8|( 'S');
        public const System.UInt32 CLRA = (U4)('C' )<<24|(U4)( 'L' )<<16|(U4)( 'R' )<<8|( 'A');
        public const System.UInt32 FILT = (U4)('F' )<<24|(U4)( 'I' )<<16|(U4)( 'L' )<<8|( 'T');
        public const System.UInt32 DITH = (U4)('D' )<<24|(U4)( 'I' )<<16|(U4)( 'T' )<<8|( 'H');

        //  Envelope
        public const System.UInt32 PRE  = (U4)('P' )<<24|(U4)( 'R' )<<16|(U4)( 'E' )<<8|( ' ');
        public const System.UInt32 POST = (U4)('P' )<<24|(U4)( 'O' )<<16|(U4)( 'S' )<<8|( 'T');
        public const System.UInt32 KEY  = (U4)('K' )<<24|(U4)( 'E' )<<16|(U4)( 'Y' )<<8|( ' ');
        public const System.UInt32 SPAN = (U4)('S' )<<24|(U4)( 'P' )<<16|(U4)( 'A' )<<8|( 'N');
        public const System.UInt32 CHAN = (U4)('C' )<<24|(U4)( 'H' )<<16|(U4)( 'A' )<<8|( 'N');

        //  Surface
        public const System.UInt32 COLR = (U4)('C' )<<24|(U4)( 'O' )<<16|(U4)( 'L' )<<8|( 'R');
        public const System.UInt32 DIFF = (U4)('D' )<<24|(U4)( 'I' )<<16|(U4)( 'F' )<<8|( 'F');
        public const System.UInt32 LUMI = (U4)('L' )<<24|(U4)( 'U' )<<16|(U4)( 'M' )<<8|( 'I');
        public const System.UInt32 SPEC = (U4)('S' )<<24|(U4)( 'P' )<<16|(U4)( 'E' )<<8|( 'C');
        public const System.UInt32 REFL = (U4)('R' )<<24|(U4)( 'E' )<<16|(U4)( 'F' )<<8|( 'L');
        public const System.UInt32 TRAN = (U4)('T' )<<24|(U4)( 'R' )<<16|(U4)( 'A' )<<8|( 'N');
        public const System.UInt32 TRNL = (U4)('T' )<<24|(U4)( 'R' )<<16|(U4)( 'N' )<<8|( 'L');
        public const System.UInt32 GLOS = (U4)('G' )<<24|(U4)( 'L' )<<16|(U4)( 'O' )<<8|( 'S');
        public const System.UInt32 SHRP = (U4)('S' )<<24|(U4)( 'H' )<<16|(U4)( 'R' )<<8|( 'P');
        public const System.UInt32 BUMP = (U4)('B' )<<24|(U4)( 'U' )<<16|(U4)( 'M' )<<8|( 'P');
        public const System.UInt32 SIDE = (U4)('S' )<<24|(U4)( 'I' )<<16|(U4)( 'D' )<<8|( 'E');
        public const System.UInt32 SMAN = (U4)('S' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'N');
        public const System.UInt32 RFOP = (U4)('R' )<<24|(U4)( 'F' )<<16|(U4)( 'O' )<<8|( 'P');
        public const System.UInt32 RIMG = (U4)('R' )<<24|(U4)( 'I' )<<16|(U4)( 'M' )<<8|( 'G');
        public const System.UInt32 RSAN = (U4)('R' )<<24|(U4)( 'S' )<<16|(U4)( 'A' )<<8|( 'N');
        public const System.UInt32 RIND = (U4)('R' )<<24|(U4)( 'I' )<<16|(U4)( 'N' )<<8|( 'D');
        public const System.UInt32 CLRH = (U4)('C' )<<24|(U4)( 'L' )<<16|(U4)( 'R' )<<8|( 'H');
        public const System.UInt32 TROP = (U4)('T' )<<24|(U4)( 'R' )<<16|(U4)( 'O' )<<8|( 'P');
        public const System.UInt32 TIMG = (U4)('T' )<<24|(U4)( 'I' )<<16|(U4)( 'M' )<<8|( 'G');
        public const System.UInt32 CLRF = (U4)('C' )<<24|(U4)( 'L' )<<16|(U4)( 'R' )<<8|( 'F');
        public const System.UInt32 ADTR = (U4)('A' )<<24|(U4)( 'D' )<<16|(U4)( 'T' )<<8|( 'R');
        public const System.UInt32 GLOW = (U4)('G' )<<24|(U4)( 'L' )<<16|(U4)( 'O' )<<8|( 'W');
        public const System.UInt32 LINE = (U4)('L' )<<24|(U4)( 'I' )<<16|(U4)( 'N' )<<8|( 'E');
        public const System.UInt32 VCOL = (U4)('V' )<<24|(U4)( 'C' )<<16|(U4)( 'O' )<<8|( 'L');
        public const System.UInt32 AVAL = (U4)('A' )<<24|(U4)( 'V' )<<16|(U4)( 'A' )<<8|( 'L');
        public const System.UInt32 GVAL = (U4)('G' )<<24|(U4)( 'V' )<<16|(U4)( 'A' )<<8|( 'L');
        public const System.UInt32 BLOK = (U4)('B' )<<24|(U4)( 'L' )<<16|(U4)( 'O' )<<8|( 'K');
        public const System.UInt32 LCOL = (U4)('L' )<<24|(U4)( 'C' )<<16|(U4)( 'O' )<<8|( 'L');  //  Documentation?
        public const System.UInt32 LSIZ = (U4)('L' )<<24|(U4)( 'S' )<<16|(U4)( 'I' )<<8|( 'Z');  //
        public const System.UInt32 CMNT = (U4)('C' )<<24|(U4)( 'M' )<<16|(U4)( 'N' )<<8|( 'T');  //

        //  Texture layer
        public const System.UInt32 TYPE = (U4)('T' )<<24|(U4)( 'Y' )<<16|(U4)( 'P' )<<8|( 'E');
        public const System.UInt32 NAME = (U4)('N' )<<24|(U4)( 'A' )<<16|(U4)( 'M' )<<8|( 'E');
        public const System.UInt32 ENAB = (U4)('E' )<<24|(U4)( 'N' )<<16|(U4)( 'A' )<<8|( 'B');
        public const System.UInt32 OPAC = (U4)('O' )<<24|(U4)( 'P' )<<16|(U4)( 'A' )<<8|( 'C');
        public const System.UInt32 FLAG = (U4)('F' )<<24|(U4)( 'L' )<<16|(U4)( 'A' )<<8|( 'G');
        public const System.UInt32 PROJ = (U4)('P' )<<24|(U4)( 'R' )<<16|(U4)( 'O' )<<8|( 'J');
        public const System.UInt32 STCK = (U4)('S' )<<24|(U4)( 'T' )<<16|(U4)( 'C' )<<8|( 'K');
        public const System.UInt32 TAMP = (U4)('T' )<<24|(U4)( 'A' )<<16|(U4)( 'M' )<<8|( 'P');

        //  Texture mapping
        public const System.UInt32 TMAP = (U4)('T' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'P');
        public const System.UInt32 AXIS = (U4)('A' )<<24|(U4)( 'X' )<<16|(U4)( 'I' )<<8|( 'S');
        public const System.UInt32 CNTR = (U4)('C' )<<24|(U4)( 'N' )<<16|(U4)( 'T' )<<8|( 'R');
        public const System.UInt32 SIZE = (U4)('S' )<<24|(U4)( 'I' )<<16|(U4)( 'Z' )<<8|( 'E');
        public const System.UInt32 ROTA = (U4)('R' )<<24|(U4)( 'O' )<<16|(U4)( 'T' )<<8|( 'A');
        public const System.UInt32 OREF = (U4)('O' )<<24|(U4)( 'R' )<<16|(U4)( 'E' )<<8|( 'F');
        public const System.UInt32 FALL = (U4)('F' )<<24|(U4)( 'A' )<<16|(U4)( 'L' )<<8|( 'L');
        public const System.UInt32 CSYS = (U4)('C' )<<24|(U4)( 'S' )<<16|(U4)( 'Y' )<<8|( 'S');

        //  Image maps
        public const System.UInt32 IMAP = (U4)('I' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'P');
        public const System.UInt32 IMAG = (U4)('I' )<<24|(U4)( 'M' )<<16|(U4)( 'A' )<<8|( 'G');
        public const System.UInt32 WRAP = (U4)('W' )<<24|(U4)( 'R' )<<16|(U4)( 'A' )<<8|( 'P');
        public const System.UInt32 WRPW = (U4)('W' )<<24|(U4)( 'R' )<<16|(U4)( 'P' )<<8|( 'W');
        public const System.UInt32 WRPH = (U4)('W' )<<24|(U4)( 'R' )<<16|(U4)( 'P' )<<8|( 'H');
        public const System.UInt32 AAST = (U4)('A' )<<24|(U4)( 'A' )<<16|(U4)( 'S' )<<8|( 'T');
        public const System.UInt32 PIXB = (U4)('P' )<<24|(U4)( 'I' )<<16|(U4)( 'X' )<<8|( 'B');

        //  Procedural texture
        public const System.UInt32 PROC = (U4)('P' )<<24|(U4)( 'R' )<<16|(U4)( 'O' )<<8|( 'C');
        public const System.UInt32 VALU = (U4)('V' )<<24|(U4)( 'A' )<<16|(U4)( 'L' )<<8|( 'U');
        public const System.UInt32 FUNC = (U4)('F' )<<24|(U4)( 'U' )<<16|(U4)( 'N' )<<8|( 'C');
        public const System.UInt32 FTPS = (U4)('F' )<<24|(U4)( 'T' )<<16|(U4)( 'P' )<<8|( 'S');
        public const System.UInt32 ITPS = (U4)('I' )<<24|(U4)( 'T' )<<16|(U4)( 'P' )<<8|( 'S');
        public const System.UInt32 ETPS = (U4)('E' )<<24|(U4)( 'T' )<<16|(U4)( 'P' )<<8|( 'S');

        //  Gradient
        public const System.UInt32 GRAD = (U4)('G' )<<24|(U4)( 'R' )<<16|(U4)( 'A' )<<8|( 'D');
        public const System.UInt32 GRST = (U4)('G' )<<24|(U4)( 'R' )<<16|(U4)( 'S' )<<8|( 'T');
        public const System.UInt32 GREN = (U4)('G' )<<24|(U4)( 'R' )<<16|(U4)( 'E' )<<8|( 'N');

        //  Shader plugin
        public const System.UInt32 SHDR = (U4)('S' )<<24|(U4)( 'H' )<<16|(U4)( 'D' )<<8|( 'R');
        public const System.UInt32 DATA = (U4)('D' )<<24|(U4)( 'A' )<<16|(U4)( 'T' )<<8|( 'A');
        public const System.UInt32 AUVN = (U4)('A' )<<24|(U4)( 'U' )<<16|(U4)( 'V' )<<8|( 'N');
        public const System.UInt32 AUVU = (U4)('A' )<<24|(U4)( 'U' )<<16|(U4)( 'V' )<<8|( 'U');
        public const System.UInt32 AUVO = (U4)('A' )<<24|(U4)( 'U' )<<16|(U4)( 'V' )<<8|( 'O');

        //  Object vertex mapping
        public const System.UInt32 PICK = (U4)('P' )<<24|(U4)( 'I' )<<16|(U4)( 'C' )<<8|( 'K');
        public const System.UInt32 WGHT = (U4)('W' )<<24|(U4)( 'G' )<<16|(U4)( 'H' )<<8|( 'T');
        public const System.UInt32 MNVW = (U4)('M' )<<24|(U4)( 'N' )<<16|(U4)( 'V' )<<8|( 'W');
        public const System.UInt32 TXUV = (U4)('T' )<<24|(U4)( 'X' )<<16|(U4)( 'U' )<<8|( 'V');
        public const System.UInt32 RGB  = (U4)('R' )<<24|(U4)( 'G' )<<16|(U4)( 'B' )<<8|( ' ');
        public const System.UInt32 RGBA = (U4)('R' )<<24|(U4)( 'G' )<<16|(U4)( 'B' )<<8|( 'A');
        public const System.UInt32 MORF = (U4)('M' )<<24|(U4)( 'O' )<<16|(U4)( 'R' )<<8|( 'F');
        public const System.UInt32 SPOT = (U4)('S' )<<24|(U4)( 'P' )<<16|(U4)( 'O' )<<8|( 'T');

        public const System.UInt32 PNAM = (U4)('P' )<<24|(U4)( 'N' )<<16|(U4)( 'A' )<<8|( 'M'); 
        public const System.UInt32 INAM = (U4)('I' )<<24|(U4)( 'N' )<<16|(U4)( 'A' )<<8|( 'M'); 
        public const System.UInt32 GRPT = (U4)('G' )<<24|(U4)( 'R' )<<16|(U4)( 'P' )<<8|( 'T'); 
        public const System.UInt32 FKEY = (U4)('F' )<<24|(U4)( 'K' )<<16|(U4)( 'E' )<<8|( 'Y'); 
        public const System.UInt32 IKEY = (U4)('I' )<<24|(U4)( 'K' )<<16|(U4)( 'E' )<<8|( 'Y'); 

        //  Unknown?
        public const System.UInt32 APSL = (U4)('A' )<<24|(U4)( 'P' )<<16|(U4)( 'S' )<<8|( 'L');

        //  Animation
        public const System.UInt32 TCB  = (U4)('T' )<<24|(U4)( 'C' )<<16|(U4)( 'B' )<<8|( ' ');
        public const System.UInt32 HERM = (U4)('H' )<<24|(U4)( 'E' )<<16|(U4)( 'R' )<<8|( 'M');
        public const System.UInt32 BEZI = (U4)('B' )<<24|(U4)( 'E' )<<16|(U4)( 'Z' )<<8|( 'I');
        public const System.UInt32 STEP = (U4)('S' )<<24|(U4)( 'T' )<<16|(U4)( 'E' )<<8|( 'P');
        public const System.UInt32 BEZ2 = (U4)('B' )<<24|(U4)( 'E' )<<16|(U4)( 'Z' )<<8|( '2');
    }
}
