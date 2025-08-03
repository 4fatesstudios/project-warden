using UnityEngine;
using UnityEngine.UIElements;


namespace FourFatesStudios.ProjectWarden.UI
{
    [UxmlElement]
    public partial class TurnDial : VisualElement
    {
        public TurnDial(){
            generateVisualContent += GenerateVisualContent;
        }

        public void GenerateVisualContent(MeshGenerationContext context){
            float width = contentRect.width;
            float height = contentRect.height;

            var painter = context.painter2D;
            painter.BeginPath();
            painter.LineTo(new Vector2(width * 0.5f, height));
            painter.lineWidth = 10f;
            painter.Arc(new Vector2(width * 0.5f, height),width * 0.5f, 180f, -90);
            painter.ClosePath();
            painter.fillColor = Color.white;
            painter.Fill(FillRule.NonZero);
            painter.Stroke();
        }
    
    }
}