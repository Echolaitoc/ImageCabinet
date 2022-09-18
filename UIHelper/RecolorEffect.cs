using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ImageCabinet.UIHelper
{
    public class RecolorEffect : ShaderEffect
    {
        private static PixelShader RecolorShader { get; } = new PixelShader() { UriSource = UIHelper.MakePackUri(@"UIHelper/RecolorEffect.ps") };

        public RecolorEffect()
        {
            Initialize();
        }

        public RecolorEffect(Color color)
        {
            Color = color;
            Initialize();
        }

        private void Initialize()
        {
            PixelShader = RecolorShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ColorProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(RecolorEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("BlankColor", typeof(Color), typeof(RecolorEffect), new UIPropertyMetadata(Colors.Magenta, PixelShaderConstantCallback(0)));
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }

    internal class RecolorEffectExtension : MarkupExtension
    {
        public Color Color { get; set; }

        public RecolorEffectExtension(Color color)
        {
            Color = color;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
            var propertyType = (provideValueTarget?.TargetProperty as DependencyProperty)?.PropertyType;
            if (propertyType == typeof(Effect))
            {
                return new RecolorEffect(Color);
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
