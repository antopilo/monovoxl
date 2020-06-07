
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace HelloMonoGame.Graphics.UI
{
    public static class InterfaceManager
    {
        private static ImGUIRenderer GuiRenderer;

        public static void Initialize(Game owner)
        {
           //GuiRenderer = new ImGUIRenderer(owner).Initialize().RebuildFontAtlas();
            
        }

        public static void RenderLayout(GameTime gameTime)
        {
            //GuiRenderer.BeginLayout(gameTime);
            //
            //
            //
            //ImGui.Button("Hello");
            //
            //
            //// ImGui.Render();
            //
            ////Insert Your ImGui code
            //
            //GuiRenderer.EndLayout();
        }
    }
}
