using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

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
using COL12 = RenderStack.Math.Vector3;
using VEC12 = RenderStack.Math.Vector3;
using FP4 = System.Single;
using ANG4 = System.Single;
using FNAM0 = System.String;

namespace RenderStack.LightWave
{
    public partial class LWSceneParser
    {
        private LWScene         scene;
        private LWSceneFile     file;
        private LWItem          currentItem;
        private LWSObject       currentObject;
        private LWMotion        currentMotion;
        private LWLight         currentLight;
        private LWCamera        currentCamera;
        private SFloatEnvelope  currentFloatEnvelope;
        private LWSEnvelope     currentEnvelope;
        private LWBone          currentBone;

        public  LWScene Scene { get { return scene; } }

#if false
void LWSceneParser::resolveParent( LWInstance *object ){
    ulong  all_bits  = object.getParentObjectId();
    if( all_bits == 0xffffffff ){
        return;
    }
    ulong  item_type = (0xf0000000 & all_bits) >> 28;
    ulong  item_num  = (0x0fffffff & all_bits);
    LWInstance    *parent    = NULL;

    switch( item_type ){

    case 1: {  //  Object
        int_to_LWInstance::iterator i_it = objects.find( item_num );
        if( i_it != objects.end() ){
            parent = (*i_it).second;
        }else{
            emsg( M_LWS, "Parent object %d not found", item_num );
        }
        break;
    }

    case 2: {  //  Light
        int_to_LWLight::iterator l_it = lights.find( item_num );
        if( l_it != lights.end() ){
            parent = (*l_it).second;
        }else{
            emsg( M_LWS, "Parent light %d not found", item_num );
        }
        break;
    }

    case 3: {  //  Camera
        int_to_LWCamera::iterator c_it = cameras.find( item_num );
        if( c_it != cameras.end() ){
            parent = (*c_it).second;
        }else{
            emsg( M_LWS, "Parent camera %d not found", item_num );
        }
        break;
    }

    case 4: {  //  Bone
        ulong  bone_num  = (0x0fff0000 & all_bits);
        ulong  obj_num   = (0x0000ffff & all_bits);
        int_to_LWInstance::iterator i_it = objects.find( obj_num );
        if( i_it != objects.end() ){
            LWInstance *obj = (*i_it).second;
            if( obj != NULL ){
                parent = obj.getBone( bone_num );
            }else{
                emsg( M_LWS, "Parent object %d found NULL", obj_num );
            }
        }else{
            emsg( M_LWS, "Parent object %d not found", obj_num );
        }
        break;
    }

    default:
        emsg( M_LWS, "Unknown item type %d", item_type );
        break;
    }

    object.setParentObject( parent );
}
#endif

        void BeginScope()
        {
            // Envelope?
        }

        void EndScope()
        {
        }

        public static LWScene Load(string name)
        {
            var parser = new LWSceneParser(name);
            return parser.scene;
        }

        LWSceneParser(string fname)
        {
            scene = new LWScene();
            file = new LWSceneFile(fname);
            bool cont = true;

            while(cont)
            {
                string str;
                Token token = file.read_token(out str);

                switch(token)
                {
                    //  Scene
                    case Token.LWSC                         : LWSC                   (); break;
                    case Token.LWS_BEGIN_SCOPE              : BeginScope(); break;

                    case Token.FirstFrame                   : FirstFrame             (); break;
                    case Token.LastFrame                    : LastFrame              (); break;
                    case Token.FrameStep                    : FrameStep              (); break;
                    case Token.FramesPerSecond              : FramesPerSecond        (); break;
                    case Token.PreviewFirstFrame            : PreviewFirstFrame      (); break;
                    case Token.PreviewLastFrame             : PreviewLastFrame       (); break;
                    case Token.PreviewFrameStep             : PreviewFrameStep       (); break;
                    case Token.CurrentFrame                 : CurrentFrame           (); break;
                    case Token.Plugin                       : Plugin                 (); break;
                    case Token.EndPlugin                    : EndPlugin              (); break;

                    //  Objects
                    case Token.AddNullObject                : AddNullObject          (); break;
                    case Token.LoadObject                   : LoadObject             (); break;
                    case Token.LoadObjectLayer              : LoadObjectLayer        (); break;
                    case Token.ShowObject                   : ShowObject             (); break;
                    case Token.ItemActive                   : ItemActive             (); break;
                    case Token.ObjectMotion                 : ObjectMotion           (); break;
                    case Token.ObjectDissolve               : ObjectDissolve         (); break;
                    case Token.SubdivisionOrder             : SubdivisionOrder       (); break;
                    case Token.SubPatchLevel                : SubPatchLevel          (); break;
                    case Token.ShadowOptions                : ShadowOptions          (); break;
                    case Token.DistanceDissolve             : DistanceDissolve       (); break;
                    case Token.MaxDissolveDistance          : MaxDissolveDistance    (); break;
                    case Token.ParticleSize                 : ParticleSize           (); break;
                    case Token.LineSize                     : LineSize               (); break;
                    case Token.PolygonSize                  : PolygonSize            (); break;
                    case Token.UnseenByRays                 : UnseenByRays           (); break;
                    case Token.UnseenByCamera               : UnseenByCamera         (); break;
                    case Token.UnaffectedByFog              : UnaffectedByFog        (); break;
                    case Token.AffectedByFog                : AffectedByFog          (); break;
                    case Token.ObjPolygonEdges              : ObjPolygonEdges        (); break;
                    case Token.ObjEdgeColor                 : ObjEdgeColor           (); break;
                    case Token.ExcludeLight                 : ExcludeLight           (); break;
                    case Token.PolygonEdgeFlags             : PolygonEdgeFlags       (); break;
                    case Token.PolygonEdgeThickness         : PolygonEdgeThickness   (); break;
                    case Token.PolygonEdgesZScale           : PolygonEdgesZScale     (); break;
                    case Token.EdgeNominalDistance          : EdgeNominalDistance    (); break;
                    case Token.MetaMorph                    : Metamorph                 (); break;
                    case Token.MorphTarget                  : MorphTarget            (); break;
                    case Token.MorphSurfaces                : MorphSurfaces          (); break;

                    case Token.UseBonesFrom                 : UseBonesFrom           (); break;
                    case Token.BoneFalloffType              : BoneFalloffType        (); break;
                    case Token.AddBone                      : AddBone                (); break;
                    case Token.BoneName                     : BoneName               (); break;
                    case Token.ShowBone                     : ShowBone               (); break;
                    case Token.BoneActive                   : BoneActive             (); break;
                    case Token.BoneRestPosition             : BoneRestPosition       (); break;
                    case Token.BoneRestDirection            : BoneRestDirection      (); break;
                    case Token.BoneRestLength               : BoneRestLength         (); break;
                    case Token.BoneStrength                 : BoneStrength           (); break;
                    case Token.ScaleBoneStrength            : ScaleBoneStrength      (); break;
                    case Token.BoneLimitedRange             : BoneLimitedRange       (); break;
                    case Token.BoneMinRange                 : BoneMinRange           (); break;
                    case Token.BoneMaxRange                 : BoneMaxRange           (); break;
                    case Token.BoneMotion                   : BoneMotion             (); break;
                    case Token.BoneNormalization            : BoneNormalization      (); break;
                    case Token.BoneWeightMapName            : BoneWeightMapName      (); break;
                    case Token.BoneWeightMapOnly            : BoneWeightMapOnly      (); break;

                    case Token.DisplacementMap              : DisplacementMap        (); break;
                    case Token.ClipMap                      : ClipMap                (); break;
                    case Token.TextureImage                 : TextureImage           (); break;
                    case Token.TextureFlags                 : TextureFlags           (); break;
                    case Token.TextureAxis                  : TextureAxis            (); break;
                    case Token.TextureSize                  : TextureSize            (); break;
                    case Token.TextureCenter                : TextureCenter          (); break;
                    case Token.TextureFalloff               : TextureFalloff         (); break;
                    case Token.TextureVelocity              : TextureVelocity        (); break;
                    case Token.TextureAmplitude             : TextureAmplitude       (); break;
                    case Token.TextureValue                 : TextureValue           (); break;
                    case Token.TextureInt                   : TextureInt             (); break;
                    case Token.TextureFloat                 : TextureFloat           (); break;

                    //  Items
                    case Token.PivotPosition                : PivotPosition          (); break;
                    case Token.ParentItem                   : ParentItem             (); break;
                    case Token.Behaviors                    : Behaviors              (); break;
                    case Token.NumChannels                  : NumChannels            (); break;
                    case Token.Channel                      : Channel                (); break;
                    case Token.Envelope                     : Envelope               (); break;
                    case Token.Key                          : Key                    (); break;
                    case Token.ParentObject                 : ParentObject           (); break;
                    case Token.TargetObject                 : TargetObject           (); break;
                    case Token.GoalObject                   : GoalObject             (); break;
                    case Token.SchematicPosition            : SchematicPosition      (); break;
                    case Token.HController                  : HController            (); break;
                    case Token.PController                  : PController            (); break;
                    case Token.BController                  : BController            (); break;
                    case Token.HLimits                      : HLimits                (); break;
                    case Token.PLimits                      : PLimits                (); break;
                    case Token.BLimits                      : BLimits                (); break;

                    //  Bones
                    case Token.IKAnchor                     : IKAnchor               (); break;

                    //  View
                    case Token.KeyableChannels              : KeyableChannels        (); break;

                    //  Lights - global
                    case Token.AmbientColor                 : AmbientColor           (); break;
                    case Token.AmbientIntensity             : AmbientIntensity       (); break;
                    case Token.GlobalFlareIntensity         : GlobalFlareIntensity   (); break;
                    case Token.EnableLensFlares             : EnableLensFlares       (); break;
                    case Token.EnableShadowMaps             : EnableShadowMaps       (); break;

                    //  Lights
                    case Token.AddLight                     : AddLight               (); break;
                    case Token.ShowLight                    : ShowLight              (); break;
                    case Token.LightName                    : LightName              (); break;
                    case Token.LightMotion                  : LightMotion            (); break;
                    case Token.LightColor                   : LightColor             (); break;
                    case Token.LightType                    : LightType              (); break;
                    case Token.LightFalloffType             : LightFalloffType       (); break;
                    case Token.LightRange                   : LightRange             (); break;
                    case Token.LightConeAngle               : LightConeAngle         (); break;
                    case Token.LightEdgeAngle               : LightEdgeAngle         (); break;
                    case Token.LightIntensity               : LightIntensity         (); break;
                    case Token.Falloff                      : Falloff                (); break;
                    case Token.AffectCaustics               : AffectCaustics         (); break;
                    case Token.AffectDiffuse                : AffectDiffuse          (); break;
                    case Token.AffectSpecular               : AffectSpecular         (); break;
                    case Token.AffectOpenGL                 : AffectOpenGL           (); break;
                    case Token.UseConeAngle                 : UseConeAngle           (); break;
                    case Token.LensFlare                    : LensFlare              (); break;
                    case Token.FlareIntensity               : FlareIntensity         (); break;
                    case Token.FlareDissolve                : FlareDissolve          (); break;
                    case Token.LensFlareFade                : LensFlareFade          (); break;
                    case Token.LensFlareOptions             : LensFlareOptions       (); break;
                    case Token.FraeRandStreakInt            : FlareRandStreakInt     (); break;
                    case Token.FlareRandStreakDens          : FlareRandStreakDens    (); break;
                    case Token.FlareRandStreakSharp         : FlareRandStreakSharp   (); break;
                    case Token.ShadowType                   : ShadowType             (); break;
                    case Token.ShadowCasting                : ShadowCasting          (); break;
                    case Token.ShadowMapSize                : ShadowMapSize          (); break;
                    case Token.ShadowMapAngle               : ShadowMapAngle         (); break;
                    case Token.ShadowFuzziness              : ShadowFuzziness        (); break;
                    case Token.ShadowColor                  : ShadowColor            (); break;

                    //  Cameras
                    case Token.AddCamera                    : AddCamera              (); break;
                    case Token.CameraName                   : CameraName             (); break;
                    case Token.ShowCamera                   : ShowCamera             (); break;
                    case Token.CameraMotion                 : CameraMotion           (); break;
                    case Token.ZoomFactor                   : ZoomFactor             (); break;
                    case Token.MotionBlur                   : MotionBlur             (); break;
                    case Token.BlurLength                   : BlurLength             (); break;
                    case Token.DepthOfField                 : DepthofField           (); break;
                    case Token.FocalDistance                : FocalDistance          (); break;
                    case Token.LensFStop                    : LensFStop              (); break;

                    case Token.ResolutionMultiplier         : ResolutionMultiplier   (); break;
                    case Token.Resolution                   : Resolution             (); break;
                    case Token.FrameSize                    : FrameSize              (); break;
                    case Token.CustomSize                   : CustomSize             (); break;
                    case Token.FilmSize                     : FilmSize               (); break;
                    case Token.NTSCWideScreen               : NTSCWidescreen         (); break;
                    case Token.PixelAspect                  : PixelAspect            (); break;
                    case Token.PixelAspectRatio             : PixelAspectRatio       (); break;
                    case Token.CustomPixelRatio             : CustomPixelRatio       (); break;
                    case Token.LimitedRegion                : LimitedRegion          (); break;
                    case Token.MaskPosition                 : MaskPosition           (); break;
                    case Token.ApertureHeight               : ApertureHeight         (); break;
                    case Token.RegionLimits                 : RegionLimits           (); break;
                    case Token.SegmentMemory                : SegmentMemory          (); break;
                    case Token.AntiAliasing                 : Antialiasing           (); break;
                    case Token.EnhancedAA                   : EnhancedAA             (); break;
                    case Token.FilterType                   : FilterType             (); break;
                    case Token.AdaptiveSampling             : AdaptiveSampling       (); break;
                    case Token.AdaptiveThreshold            : AdaptiveThreshold      (); break;
                    case Token.FieldRendering               : FieldRendering         (); break;
                    case Token.ReverseFields                : ReverseFields          (); break;

                    //  Effects
                    case Token.BGImage                      : BGImage                (); break;
                    case Token.FGImage                      : FGImage                (); break;
                    case Token.FGAlphaImage                 : FGAlphaImage           (); break;
                    case Token.FGDissolve                   : FGDissolve             (); break;
                    case Token.FGFaderAlphaMode             : FGFaderAlphaMode       (); break;
                    case Token.ImageSequenceInfo            : ImageSequenceInfo      (); break;
                    case Token.ForegroundKey                : ForegroundKey          (); break;
                    case Token.LowClipColor                 : LowClipColor           (); break;
                    case Token.HighClipColor                : HighClipColor          (); break;
                    case Token.SolidBackdrop                : SolidBackdrop          (); break;
                    case Token.BackdropColor                : BackdropColor          (); break;
                    case Token.ZenithColor                  : ZenithColor            (); break;
                    case Token.SkyColor                     : SkyColor               (); break;
                    case Token.GroundColor                  : GroundColor            (); break;
                    case Token.NadirColor                   : NadirColor             (); break;
                    case Token.SkySqueezeAmount             : SkySqueezeAmount       (); break;
                    case Token.GroundSqueezeAmount          : GroundSqueezeAmount    (); break;
                    case Token.FogType                      : FogType                (); break;
                    case Token.FogMinDist                   : FogMinDist             (); break;
                    case Token.FogMaxDist                   : FogMaxDist             (); break;
                    case Token.FogMinAmount                 : FogMinAmount           (); break;
                    case Token.FogMaxAmount                 : FogMaxAmount           (); break;
                    case Token.FogColor                     : FogColor               (); break;
                    case Token.BackdropFog                  : BackdropFog            (); break;
                    case Token.DitherIntensity              : DitherIntensity        (); break;
                    case Token.AnimatedDither               : AnimatedDither         (); break;
                    case Token.Saturation                   : Saturation             (); break;
                    case Token.GlowEffect                   : GlowEffect             (); break;
                    case Token.GlowIntensity                : GlowIntensity          (); break;
                    case Token.GlowRadius                   : GlowRadius             (); break;

                    //  Render
                    case Token.RenderMode                   : RenderMode             (); break;
                    case Token.RayTraceEffects              : RayTraceEffects        (); break;
                    case Token.RayTraceOptimization         : RayTraceOptimization   (); break;
                    case Token.RayRecursionLimit            : RayRecursionLimit      (); break;
                    case Token.DataOverlay                  : DataOverlay            (); break;
                    case Token.DataOverlayLabel             : DataOverlayLabel       (); break;
                    case Token.OutputFilenameFormat         : OutputFilenameFormat   (); break;
                    case Token.SaveRGB                      : SaveRGB                (); break;
                    case Token.SaveAlpha                    : SaveAlpha              (); break;
                    case Token.SaveAnimFileName             : SaveANIMFileName       (); break;
                    case Token.LockAnimPaletteFrame         : LockANIMPaletteFrame   (); break;
                    case Token.BeginAnimLoopFrame           : BeginANIMLoopFrame     (); break;
                    case Token.SaveRGBImagesPrefix          : SaveRGBImagesPrefix    (); break;
                    case Token.RGBImageFormat               : RGBImageFormat         (); break;
                    case Token.SaveAlphaImagesPrefix        : SaveAlphaImagesPrefix  (); break;
                    case Token.AlphaImageFormat             : AlphaImageFormat       (); break;
                    case Token.AlphaMode                    : AlphaMode              (); break;
                    case Token.SaveFramestoresComment       : SaveFramestoresComment (); break;
                    case Token.FullSceneParamEval           : FullSceneParamEval     (); break;

                    //  Layout Options
                    case Token.LockedChannels               : LockedChannels         (); break;
                    case Token.ViewConfiguration            : ViewConfiguration      (); break;
                    case Token.DefineView                   : DefineView             (); break;
                    case Token.ViewMode                     : ViewMode               (); break;
                    case Token.ViewAimpoint                 : ViewAimpoint           (); break;
                    case Token.ViewRotation                 : ViewRotation           (); break;
                    case Token.ViewZoomFactor               : ViewZoomFactor         (); break;
                    case Token.ViewType                     : ViewType               (); break;

                    case Token.LayoutGrid                   : LayoutGrid             (); break;
                    case Token.GridNumber                   : GridNumber             (); break;
                    case Token.GridSize                     : GridSize               (); break;
                    case Token.CameraViewBG                 : CameraViewBG           (); break;
                    case Token.ShowMotionPath               : ShowMotionPath         (); break;
                    case Token.ShowSafeAreas                : ShowSafeAreas          (); break;
                    case Token.ShowBGImage                  : ShowBGImage            (); break;
                    case Token.ShowFogRadius                : ShowFogRadius          (); break;
                    case Token.ShowFogEffect                : ShowFogEffect          (); break;
                    case Token.ShowRedraw                   : ShowRedraw             (); break;
                    case Token.ShowFieldChart               : ShowFieldChart         (); break;
                    case Token.OverlayColor                 : OverlayColor           (); break;

                    case Token.CurrentObject                : CurrentObject          (); break;
                    case Token.CurrentLight                 : CurrentLight           (); break;
                    case Token.CurrentCamera                : CurrentCamera          (); break;
                    case Token.MapEditorData                : GraphEditorData        (); break;
                    case Token.GraphEditorFavorites         : GraphEditorFavorites   (); break;

                    case Token.LWS_EOF:
                    {
                        Debug.WriteLine("LWS EOF");
                        cont = false;
                        break;
                    }

                    case Token.LWS_ERROR:
                    {
                        Debug.WriteLine("LWS ERROR");
                        cont = false;
                        break;
                    }

                    case Token.LWS_UNKNOWN:
                    {
                        Debug.WriteLine("LWS UNKNOWN");
                        break;
                    }

                    default:
                    {
                        Debug.WriteLine("Unknown token '" + str + "'");
                        break;
                    }
                }
            }
            Debug.WriteLine("Reading " + fname + " done");
        }

    }
}
