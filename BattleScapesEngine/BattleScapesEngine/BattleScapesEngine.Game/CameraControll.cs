using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    public class CameraControll : AsyncScript
    {
        public Entity Camera;

        private float angle;
        private bool moveCamera;

        public override async Task Execute()
        {
            // UI button
            ToggleButton button = new ToggleButton
            {
                Content = new TextBlock
                {
                    Text = "Move Camera",
                    Font = Asset.Load<SpriteFont>("Font")
                }
            };
            button.Click += (sender, args) => moveCamera = !moveCamera;

            // Panel
            Entity.Get<UIComponent>().RootElement = new StackPanel
            {
                Orientation = SiliconStudio.Paradox.UI.Orientation.Vertical,
                HorizontalAlignment = SiliconStudio.Paradox.UI.HorizontalAlignment.Left,
                Margin = new SiliconStudio.Paradox.UI.Thickness(10, 10, 20, 0),
                Children = { button }
            };

            while (Game.IsActive)
            {
                await Script.NextFrame();

                if (!moveCamera) continue;
                float elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Rotate camera
                angle += (float)Math.PI * elapsedTime * 0.1f;
                Camera.Transform.Position = new Vector3(5f * (float)Math.Sin(angle), Camera.Transform.Position.Y, 5f * (float)Math.Cos(angle));
                Camera.Transform.Rotation = -Quaternion.RotationY(angle);
            }
        }
    }
}
