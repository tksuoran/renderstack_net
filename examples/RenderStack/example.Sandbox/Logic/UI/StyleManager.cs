using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;
using RenderStack.Services;
using RenderStack.UI;

using example.Renderer;

namespace example.Sandbox
{
    public class StyleManager : Service
    {
        public override string Name
        {
            get { return "StyleManager"; }
        }

        MaterialManager     materialManager;
        IRenderer           renderer;
        TextRenderer        textRenderer;

        public void Connect(
            MaterialManager materialManager,
            IRenderer       renderer,
            TextRenderer    textRenderer
        )
        {
            this.materialManager = materialManager;
            this.renderer = renderer;
            this.textRenderer = textRenderer;

            InitializationDependsOn(renderer);
            InitializationDependsOn(materialManager);
            InitializationDependsOn(textRenderer);
        }

        protected override void InitializeService()
        {
            TextureGL         patch6              = materialManager.Texture("res/images/ninepatch6.png", false);
            TextureGL         patch7              = materialManager.Texture("res/images/ninepatch7.png", false);
            Vector2         padding             = new Vector2(6.0f, 6.0f); 
            Vector2         innerPadding        = new Vector2(2.0f, 2.0f);
            NinePatchStyle  ninePatchStyle      = new NinePatchStyle(patch6);
            NinePatchStyle  foreNinePatchStyle  = new NinePatchStyle(patch7);

            Style.NullPadding = new Style(
                new Vector2(0.0f, 0.0f), 
                new Vector2(0.0f, 0.0f), 
                null, 
                null,
                null
            );
            Style.Background = new Style(
                padding,
                innerPadding,
                (textRenderer != null) ? textRenderer.FontStyle : null,
                ninePatchStyle,
                renderer.Programs["Ninepatch"]
            );
            Style.Foreground = new Style(
                padding,
                innerPadding,
                (textRenderer != null) ? textRenderer.FontStyle : null,
                foreNinePatchStyle,
                renderer.Programs["Font"]
            );
            Style.Default = Style.Background;
        }
    }
}