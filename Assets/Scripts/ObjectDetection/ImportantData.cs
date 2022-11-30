using System.Collections.Generic;
using Unity.Barracuda;

namespace DobreKody.Basic
{
    public sealed class ImportantData
    {
        public float[] anchors = new float[12] {10,14,23,27, 37,58,81,82 ,135,169,319,344};

        public int K = 5;
        public int Anchors = 3;
        public int Classes = 20;
        public int MAXObjects = 512;
    
        public int InputWidth = 416;
        public int Identity0Width = 13;
        public int Identity1Width = 26;

        public int DataSize => Anchors *(K+Classes);

        public int Inputsize => InputWidth *InputWidth *3;
        public int Identity0Size => Identity0Width *Identity0Width;
        public int Identity1Size => Identity1Width *Identity1Width;

        
        public TensorShape InputShape => new TensorShape(1, InputWidth, InputWidth, 3);
        public TensorShape Identity0Shape => new TensorShape(1, Identity0Width, DataSize, 1);
        public TensorShape Identity1Shape => new TensorShape(1, Identity0Width, DataSize, 1);


    }
}
