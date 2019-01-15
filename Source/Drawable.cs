using System.Drawing;

namespace TruthTree2
{
    internal interface Drawable
    {
        SizeF ImageSize(Graphics g = null);
        void Draw(Graphics g);
        void SaveImage(string file);

        bool AntiAliasText { get; }
        bool AntiAliasDrawing { get; }
    }
}
