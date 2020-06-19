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
    public class LWSceneFile
    {
        private StreamReader                reader;
        private Dictionary<string, Token>   token_map = new Dictionary<string,Token>();
        private Queue<string>               lastLine = new Queue<string>();

        public LWSceneFile(string name)
        {
            mapTokens();
            open(name);
        }
        public bool isOk()
        {
            return (reader != null) && (reader.EndOfStream == false);
        }
        public void open(string name)
        {
            var file = new FileStream(name, FileMode.Open);
            var stream = new BufferedStream(file);
            this.reader = new StreamReader(stream);
        }
        public void close()
        {
            if(reader != null)
            {
                reader.Close();
            }
        }
        private string GetNextString()
        {
            while(lastLine.Count == 0)
            {
                string line = reader.ReadLine();
                if(line == null)
                {
                    return null;
                }
                string[] split = line.Split(' ');
                foreach(string part in split)
                {
                    if(part.Length > 0)
                    {
                        lastLine.Enqueue(part);
                    }
                }
            }
            return lastLine.Dequeue();
        }
        public Token read_token()
        {
            string str = GetNextString();
            if(str == null)
            {
                return Token.LWS_EOF;
            }
            if(token_map.ContainsKey(str))
            {
                return token_map[str];
            }
            else
            {
                //  Skipping unrecognized tokens
                return read_token();
            }
        }
        public Token read_token(out string str)
        {
            str = GetNextString();
            if(str == null)
            {
                return Token.LWS_EOF;
            }
            if(token_map.ContainsKey(str))
            {
                return token_map[str];
            }
            else
            {
                //  Skipping unrecognized tokens
                return read_token(out str);
            }
        }
        public void skip()
        {
            lastLine.Clear();
        }
        public bool read_begin_scope()
        {
            bool good = true;
            for(;;)
            {
                Token t = read_token();
                if(t == Token.LWS_BEGIN_SCOPE)
                {
                    return good;
                }
                else
                {
                    good = false;
                }
            }
        }
        public bool read_end_scope()
        {
            bool good = true;
            for(;;)
            {
                Token t = read_token();
                if(t == Token.LWS_EOF)
                {
                    return false;
                }
                if(t == Token.LWS_END_SCOPE)
                {
                    return good;
                }
                else
                {
                    good = false;
                }
            }
        }
        public int read_int()
        {
            string s = GetNextString();
            return int.Parse(s);
        }

        public ulong read_hex_int()
        {
            string s = GetNextString();
            ulong value = ulong.Parse(s);
            return value;
            // \todo XXX TODO MUSTFIX
        }

        public double read_double()
        {
            string s = GetNextString();
            return System.Double.Parse(s);
        }

        public string read_string()
        {
            return GetNextString();
        }

        public void mapTokens()
        {
            //  Scene
            token_map["{"                     ] = Token.LWS_BEGIN_SCOPE;
            token_map["}"                     ] = Token.LWS_END_SCOPE;
            token_map["LWSC"                  ] = Token.LWSC;
            token_map["FirstFrame"            ] = Token.FirstFrame;
            token_map["LastFrame"             ] = Token.LastFrame;
            token_map["FrameStep"             ] = Token.FrameStep;
            token_map["FramesPerSecond"       ] = Token.FramesPerSecond;
            token_map["PreviewFirstFrame"     ] = Token.PreviewFirstFrame;
            token_map["PreviewLastFrame"      ] = Token.PreviewLastFrame;
            token_map["PreviewFrameStep"      ] = Token.PreviewFrameStep;
            token_map["CurrentFrame"          ] = Token.CurrentFrame;
            token_map["Plugin"                ] = Token.Plugin;
            token_map["EndPlugin"             ] = Token.EndPlugin;

            //  Objects
            token_map["AddNullObject"         ] = Token.AddNullObject;
            token_map["LoadObject"            ] = Token.LoadObject;
            token_map["LoadObjectLayer"       ] = Token.LoadObjectLayer;
            token_map["ShowObject"            ] = Token.ShowObject;
            token_map["ItemActive"            ] = Token.ItemActive;
            token_map["ObjectMotion"          ] = Token.ObjectMotion;
            token_map["ObjectDissolve"        ] = Token.ObjectDissolve;
            token_map["SubdivisionOrder"      ] = Token.SubdivisionOrder;
            token_map["SubPatchLevel"         ] = Token.SubPatchLevel;
            token_map["ShadowOptions"         ] = Token.ShadowOptions;
            token_map["DistanceDissolve"      ] = Token.DistanceDissolve;
            token_map["MaxDissolveDistance"   ] = Token.MaxDissolveDistance;
            token_map["ParticleSize"          ] = Token.ParticleSize;
            token_map["LineSize"              ] = Token.LineSize;
            token_map["PolygonSize"           ] = Token.PolygonSize;
            token_map["UnseenByRays"          ] = Token.UnseenByRays;
            token_map["UnseenByCamera"        ] = Token.UnseenByCamera;
            token_map["UnaffectedByFog"       ] = Token.UnaffectedByFog;
            token_map["AffectedByFog"         ] = Token.AffectedByFog;
            token_map["ExcludeLight"          ] = Token.ExcludeLight;
            token_map["ObjPolygonEdges"       ] = Token.ObjPolygonEdges;
            token_map["ObjEdgeColor"          ] = Token.ObjEdgeColor;
            token_map["PolygonEdgeFlags"      ] = Token.PolygonEdgeFlags;
            token_map["PolygonEdgeThickness"  ] = Token.PolygonEdgeThickness;
            token_map["PolygonEdgesZScale"    ] = Token.PolygonEdgesZScale;
            token_map["EdgeNominalDistance"   ] = Token.EdgeNominalDistance;

            token_map["DisplacementMap"       ] = Token.DisplacementMap;
            token_map["ClipMap"               ] = Token.ClipMap;
            token_map["TextureImage"          ] = Token.TextureImage;
            token_map["TextureFlags"          ] = Token.TextureFlags;
            token_map["TextureAxis"           ] = Token.TextureAxis;
            token_map["TextureSize"           ] = Token.TextureSize;
            token_map["TextureCenter"         ] = Token.TextureCenter;
            token_map["TextureFalloff"        ] = Token.TextureFalloff;
            token_map["TextureVelocity"       ] = Token.TextureVelocity;
            token_map["TextureAmplitude"      ] = Token.TextureAmplitude;
            token_map["TextureValue"          ] = Token.TextureValue;
            token_map["TextureInt"            ] = Token.TextureInt;
            token_map["TextureFloat"          ] = Token.TextureFloat;

            //  (for lights and cameras as well)
            token_map["PivotPosition"         ] = Token.PivotPosition;
            token_map["ParentItem"            ] = Token.ParentItem;
            token_map["Behaviors"             ] = Token.Behaviors;
            token_map["NumChannels"           ] = Token.NumChannels;
            token_map["Channel"               ] = Token.Channel;
            token_map["Envelope"              ] = Token.Envelope;
            token_map["Key"                   ] = Token.Key;
            token_map["LockedChannels"        ] = Token.LockedChannels;
            token_map["ParentObject"          ] = Token.ParentObject;
            token_map["TargetObject"          ] = Token.TargetObject;
            token_map["GoalObject"            ] = Token.GoalObject;
            token_map["SchematicPosition"     ] = Token.SchematicPosition;

            token_map["MorphAmount"           ] = Token.MetaMorph;
            token_map["MorphTarget"           ] = Token.MorphTarget;
            token_map["MorphSurfaces"         ] = Token.MorphSurfaces;

            token_map["UseBonesFrom"          ] = Token.UseBonesFrom;
            token_map["BoneFalloffType"       ] = Token.BoneFalloffType;
            token_map["AddBone"               ] = Token.AddBone;
            token_map["BoneName"              ] = Token.BoneName;
            token_map["ShowBone"              ] = Token.ShowBone;
            token_map["BoneActive"            ] = Token.BoneActive;
            token_map["BoneRestPosition"      ] = Token.BoneRestPosition;
            token_map["BoneRestDirection"     ] = Token.BoneRestDirection;
            token_map["BoneRestLength"        ] = Token.BoneRestLength;
            token_map["BoneStrength"          ] = Token.BoneStrength;
            token_map["ScaleBoneStrength"     ] = Token.ScaleBoneStrength;
            token_map["BoneLimitedRange"      ] = Token.BoneLimitedRange;
            token_map["BoneMinRange"          ] = Token.BoneMinRange;
            token_map["BoneMaxRange"          ] = Token.BoneMaxRange;
            token_map["BoneMotion"            ] = Token.BoneMotion;
            token_map["HController"           ] = Token.HController;
            token_map["PController"           ] = Token.PController;
            token_map["BController"           ] = Token.BController;
            token_map["HLimits"               ] = Token.HLimits;
            token_map["PLimits"               ] = Token.PLimits;
            token_map["BLimits"               ] = Token.BLimits;
            token_map["IKAnchor"              ] = Token.IKAnchor;
            token_map["KeyableChannels"       ] = Token.KeyableChannels;
            token_map["BoneNormalization"     ] = Token.BoneNormalization;
            token_map["BoneWeightMapName"     ] = Token.BoneWeightMapName;
            token_map["BoneWeightMapOnly"     ] = Token.BoneWeightMapOnly;

            //  Lights - global
            token_map["AmbientColor"          ] = Token.AmbientColor;
            token_map["AmbientIntensity"      ] = Token.AmbientIntensity;
            token_map["GlobalFlareIntensity"  ] = Token.GlobalFlareIntensity;
            token_map["EnableLensFlares"      ] = Token.EnableLensFlares;
            token_map["EnableShadowMaps"      ] = Token.EnableShadowMaps;

            //  Lights
            token_map["AddLight"              ] = Token.AddLight;
            token_map["ShowLight"             ] = Token.ShowLight;
            token_map["LightName"             ] = Token.LightName;
            token_map["LightMotion"           ] = Token.LightMotion;
            token_map["LightColor"            ] = Token.LightColor;
            token_map["LightType"             ] = Token.LightType;
            token_map["LightFalloffType"      ] = Token.LightFalloffType;
            token_map["LightRange"            ] = Token.LightRange;
            token_map["LightConeAngle"        ] = Token.LightConeAngle;
            token_map["LightEdgeAngle"        ] = Token.LightEdgeAngle;
            token_map["LightIntensity"        ] = Token.LightIntensity;
            token_map["Falloff"               ] = Token.Falloff;
            token_map["AffectCaustics"        ] = Token.AffectCaustics;
            token_map["AffectDiffuse"         ] = Token.AffectDiffuse;
            token_map["AffectSpecular"        ] = Token.AffectSpecular;
            token_map["AffectOpenGL"          ] = Token.AffectOpenGL;
            token_map["UseConeAngle"          ] = Token.UseConeAngle;
            token_map["LensFlare"             ] = Token.LensFlare;
            token_map["FlareIntensity"        ] = Token.FlareIntensity;
            token_map["FlareDissolve"         ] = Token.FlareDissolve;
            token_map["LensFlareFade"         ] = Token.LensFlareFade;
            token_map["LensFlareOptions"      ] = Token.LensFlareOptions;
            token_map["FlareRandStreakInt"    ] = Token.FraeRandStreakInt;
            token_map["FlareRandStreakDens"   ] = Token.FlareRandStreakDens;
            token_map["FlareRandStreakSharp"  ] = Token.FlareRandStreakSharp;
            token_map["ShadowType"            ] = Token.ShadowType;
            token_map["ShadowCasting"         ] = Token.ShadowCasting;
            token_map["ShadowMapSize"         ] = Token.ShadowMapSize;
            token_map["ShadowMapAngle"        ] = Token.ShadowMapAngle;
            token_map["ShadowFuzziness"       ] = Token.ShadowFuzziness;
            token_map["ShadowColor"           ] = Token.ShadowColor;

            //  Cameras
            token_map["AddCamera"             ] = Token.AddCamera;
            token_map["CameraName"            ] = Token.CameraName;
            token_map["ShowCamera"            ] = Token.ShowCamera;
            token_map["CameraMotion"          ] = Token.CameraMotion;
            token_map["ZoomFactor"            ] = Token.ZoomFactor;
            token_map["MotionBlur"            ] = Token.MotionBlur;
            token_map["BlurLength"            ] = Token.BlurLength;
            token_map["DepthofField"          ] = Token.DepthOfField;
            token_map["FocalDistance"         ] = Token.FocalDistance;
            token_map["LensFStop"             ] = Token.LensFStop;

            token_map["ResolutionMultiplier"  ] = Token.ResolutionMultiplier;
            token_map["Resolution"            ] = Token.Resolution;
            token_map["FrameSize"             ] = Token.FrameSize;
            token_map["CustomSize"            ] = Token.CustomSize;
            token_map["FilmSize"              ] = Token.FilmSize;
            token_map["NTSCWidescreen"        ] = Token.NTSCWideScreen;
            token_map["PixelAspect"           ] = Token.PixelAspect;
            token_map["PixelAspectRatio"      ] = Token.PixelAspectRatio;
            token_map["CustomPixelRatio"      ] = Token.CustomPixelRatio;
            token_map["LimitedRegion"         ] = Token.LimitedRegion;
            token_map["MaskPosition"          ] = Token.MaskPosition;
            token_map["ApertureHeight"        ] = Token.ApertureHeight;
            token_map["RegionLimits"          ] = Token.RegionLimits;
            token_map["SegmentMemory"         ] = Token.SegmentMemory;
            token_map["Antialiasing"          ] = Token.AntiAliasing;
            token_map["EnhancedAA"            ] = Token.EnhancedAA;
            token_map["FilterType"            ] = Token.FilterType;
            token_map["AdaptiveSampling"      ] = Token.AdaptiveSampling;
            token_map["AdaptiveThreshold"     ] = Token.AdaptiveThreshold;
            token_map["FieldRendering"        ] = Token.FieldRendering;
            token_map["ReverseFields"         ] = Token.ReverseFields;

            //  Effects
            token_map["BGImage"               ] = Token.BGImage;
            token_map["FGImage"               ] = Token.FGImage;
            token_map["FGAlphaImage"          ] = Token.FGAlphaImage;
            token_map["FGDissolve"            ] = Token.FGDissolve;
            token_map["FGFaderAlphaMode"      ] = Token.FGFaderAlphaMode;
            token_map["ImageSequenceInfo"     ] = Token.ImageSequenceInfo;
            token_map["ForegroundKey"         ] = Token.ForegroundKey;
            token_map["LowClipColor"          ] = Token.LowClipColor;
            token_map["HighClipColor"         ] = Token.HighClipColor;
            token_map["SolidBackdrop"         ] = Token.SolidBackdrop;
            token_map["BackdropColor"         ] = Token.BackdropColor;
            token_map["ZenithColor"           ] = Token.ZenithColor;
            token_map["SkyColor"              ] = Token.SkyColor;
            token_map["GroundColor"           ] = Token.GroundColor;
            token_map["NadirColor"            ] = Token.NadirColor;
            token_map["SkySqueezeAmount"      ] = Token.SkySqueezeAmount;
            token_map["GroundSqueezeAmount"   ] = Token.GroundSqueezeAmount;
            token_map["FogType"               ] = Token.FogType;
            token_map["FogMinDistance"        ] = Token.FogMinDist;
            token_map["FogMaxDistance"        ] = Token.FogMaxDist;
            token_map["FogMinAmount"          ] = Token.FogMinAmount;
            token_map["FogMaxAmount"          ] = Token.FogMaxAmount;
            token_map["FogColor"              ] = Token.FogColor;
            token_map["BackdropFog"           ] = Token.BackdropFog;
            token_map["DitherIntensity"       ] = Token.DitherIntensity;
            token_map["AnimatedDither"        ] = Token.AnimatedDither;
            token_map["Saturation"            ] = Token.Saturation;
            token_map["GlowEffect"            ] = Token.GlowEffect;
            token_map["GlowIntensity"         ] = Token.GlowIntensity;
            token_map["GlowRadius"            ] = Token.GlowRadius;

            //  Render
            token_map["RenderMode"            ] = Token.RenderMode;
            token_map["RayTraceEffects"       ] = Token.RayTraceEffects;
            token_map["RayTraceOptimization"  ] = Token.RayTraceOptimization;
            token_map["RayRecursionLimit"     ] = Token.RayRecursionLimit;
            token_map["DataOverlay"           ] = Token.DataOverlay;
            token_map["DataOverlayLabel"      ] = Token.DataOverlayLabel;
            token_map["OutputFilenameFormat"  ] = Token.OutputFilenameFormat;
            token_map["SaveRGB"               ] = Token.SaveRGB;
            token_map["SaveAlpha"             ] = Token.SaveAlpha;
            token_map["SaveANIMFileName"      ] = Token.SaveAnimFileName;
            token_map["LockANIMPaletteFrame"  ] = Token.LockAnimPaletteFrame;
            token_map["BeginANIMLoopFrame"    ] = Token.BeginAnimLoopFrame;
            token_map["SaveRGBImagesPrefix"   ] = Token.SaveRGBImagesPrefix;
            token_map["RGBImageFormat"        ] = Token.RGBImageFormat;
            token_map["SaveAlphaImagesPrefix" ] = Token.SaveAlphaImagesPrefix;
            token_map["AlphaImageFormat"      ] = Token.AlphaImageFormat;
            token_map["AlphaMode"             ] = Token.AlphaMode;
            token_map["SaveFramestoresComment"] = Token.SaveFramestoresComment;
            token_map["FullSceneParamEval"    ] = Token.FullSceneParamEval;

            //  Layout Options
            token_map["ViewConfiguration"     ] = Token.ViewConfiguration;
            token_map["DefineView"            ] = Token.DefineView;
            token_map["ViewMode"              ] = Token.ViewMode;
            token_map["ViewAimpoint"          ] = Token.ViewAimpoint;
            token_map["ViewRotation"          ] = Token.ViewRotation;
            token_map["ViewZoomFactor"        ] = Token.ViewZoomFactor;
            token_map["ViewType"              ] = Token.ViewType;

            token_map["LayoutGrid"            ] = Token.LayoutGrid;
            token_map["GridNumber"            ] = Token.GridNumber;
            token_map["GridSize"              ] = Token.GridSize;
            token_map["CameraViewBG"          ] = Token.CameraViewBG;
            token_map["ShowMotionPath"        ] = Token.ShowMotionPath;
            token_map["ShowSafeAreas"         ] = Token.ShowSafeAreas;
            token_map["ShowBGImage"           ] = Token.ShowBGImage;
            token_map["ShowFogRadius"         ] = Token.ShowFogRadius;
            token_map["ShowFogEffect"         ] = Token.ShowFogEffect;
            token_map["ShowRedraw"            ] = Token.ShowRedraw;
            token_map["ShowFieldChart"        ] = Token.ShowFieldChart;
            token_map["OverlayColor"          ] = Token.OverlayColor;

            token_map["CurrentObject"         ] = Token.CurrentObject;
            token_map["CurrentLight"          ] = Token.CurrentLight;
            token_map["CurrentCamera"         ] = Token.CurrentCamera;
            token_map["GraphEditorData"       ] = Token.MapEditorData;
            token_map["GraphEd_Favorites"     ] = Token.GraphEditorFavorites;
        }
    }
}


