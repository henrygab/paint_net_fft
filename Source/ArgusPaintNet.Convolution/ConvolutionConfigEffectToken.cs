using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintDotNet;
using PaintDotNet.Effects;

namespace ArgusPaintNet.Convolution
{
    public class ConvolutionConfigEffectToken : EffectConfigToken
    {
        public ArgusPaintNet.Shared.Matrix Kernel;
        public float Factor;
        public bool Normalize;

        public ConvolutionConfigEffectToken()
            : base()
        {
            this.Kernel = new float[,] { { 1 } };
            this.Factor = 1;
            this.Normalize = true;
        }

        public ConvolutionConfigEffectToken(ConvolutionConfigEffectToken copyToken)
            : base(copyToken)
        {
            this.Kernel = new ArgusPaintNet.Shared.Matrix(copyToken.Kernel);
            this.Factor = copyToken.Factor;
            this.Normalize = copyToken.Normalize;
        }

        public ConvolutionConfigEffectToken(ArgusPaintNet.Shared.Matrix kernel, float factor, bool normalize)
        {
            this.Kernel = kernel;
            this.Factor = factor;
            this.Normalize = normalize;
        }

        public override object Clone()
        {
            return new ConvolutionConfigEffectToken(this);
        }
    }
}
