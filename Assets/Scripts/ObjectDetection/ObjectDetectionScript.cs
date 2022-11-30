using System;
using System.Collections.Generic;
using DobreKody.Basic;
using Unity.Barracuda;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DobreKody.Basic
{
    
    
    public class ObjectDetectionScript : MonoBehaviour
    {
        #region anchors required
        
        [SerializeField] private float[] anchors = new float[12] {10,14,23,27, 37,58,81,82 ,135,169,319,344};

        private float[] Identity1Anchors_x;
        private float[] Identity2Anchors_x; 
        private float[] Identity1Anchors_y;
        private float[] Identity2Anchors_y;

        #endregion

        // our Yolo model
        [SerializeField] private NNModel model;
        
        // all shaders required in the process
        [SerializeField] private List<ComputeShader> shaders;
        
        // required knowledge on neural network shape etc;
        private ImportantData _data;
        
        // worker tasked to help with neural network
        private IWorker _worker;
        
        // all important outputs for shaders
        private List<ComputeBuffer> shaders_output;
        
        // outputs from neural network
        private List<RenderTexture> _indentity;
        
        // counters needed to read amounts of objects
        private List<ComputeBuffer> _counter;

        public float threshold = 0.5f;

        // loaded model
        private Model _model;
        
        // output of full process put in digestible form
        public CachedData MainOutput;

        private void OnDisable()
        {
            Destroy();
        }
        
        
        /// <summary>
        /// here all important data is loaded
        /// </summary>

        private void OnEnable()
        {
            //Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            
            StaticMemory.DetectionScript = this;
            
            //_resource = ScriptableObject.CreateInstance<Resource>();
            _model = ModelLoader.Load(model);
            _worker = _model.CreateWorker();
            _data = new ImportantData();

            shaders_output = new List<ComputeBuffer>();
            shaders_output.Add(new ComputeBuffer(_data.Inputsize, sizeof(float)));
            shaders_output.Add(new ComputeBuffer(_data.MAXObjects, DetectedObject.Size));
            shaders_output.Add(new ComputeBuffer(_data.MAXObjects, DetectedObject.Size, ComputeBufferType.Append)); 
            
            _counter = new List<ComputeBuffer>();
            _counter.Add(new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Counter)); 
            _counter.Add(new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw));

            
            /////TUTAJ DEPTH BYL ZERO >>> TO BEZ SENSU >>>>
            _indentity = new List<RenderTexture>();
            _indentity.Add(new RenderTexture(_data.DataSize, _data.Identity0Size, 1, RenderTextureFormat.RFloat));
            _indentity.Add(new RenderTexture(_data.DataSize, _data.Identity1Size, 1, RenderTextureFormat.RFloat));


            Identity1Anchors_x = ExtensionFunctions.MakeAnchorArray(anchors, 6, 8, 10, 1.0f/_data.InputWidth);
            Identity1Anchors_y = ExtensionFunctions.MakeAnchorArray(anchors, 7, 9, 11, 1.0f/_data.InputWidth);
            Identity2Anchors_x = ExtensionFunctions.MakeAnchorArray(anchors, 0, 2, 4, 1.0f/_data.InputWidth);
            Identity2Anchors_y = ExtensionFunctions.MakeAnchorArray(anchors, 1, 3, 5, 1.0f/_data.InputWidth);

            MainOutput = new CachedData(shaders_output[2], _counter[1]);
        }

        /// <summary>
        /// 1) we restart all important counters (also one inside of shader_output)
        /// 2) the data is processed firstly by resizing image file inside of a shader
        /// 3) the now resized to 416/416 picture is put inside the neural network
        /// 4) output from neural network are 2 identities of different sizes. them are now copied out of buffers to be used inside shaders
        /// 5) we put one identity at a time inside of first shader which checks their data and what scored the largest in said pixel and
        ///    from this knowledge we now have a set of possible objects recognized
        /// 6) we take data out of shader with one of shader_outputs and put it inside next shader
        ///    which takes what we have and checks if any of the objects overlaps each other and if
        ///    overlapping objects score is smaller than object it overlaps it disappears
        /// 7) at the end we update the cache in MainOutput and with it our process is complete
        /// 
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="limit"></param>
        public void Process(Texture source)
        {
            
            _counter[0].SetCounterValue(0);
            shaders_output[2].SetCounterValue(0);
            
            
            ComputeShader preprocess = shaders[0];
            preprocess.SetInt("Size", _data.InputWidth);
            
            
            preprocess.SetTexture(0, "Image", source);
            preprocess.SetBuffer(0, "RestructuredImage", shaders_output[0]);
            preprocess.DispatchThreads(0, _data.InputWidth, _data.InputWidth, 1);


            using (var t = new Tensor(_data.InputShape, shaders_output[0])) _worker.Execute(t);
            _worker.CopyOutput("Identity", _indentity[0]);
            _worker.CopyOutput("Identity_1", _indentity[1]);


            
            ComputeShader DDA = shaders[1];
            DDA.SetBuffer(0,"Output",shaders_output[1]);
            DDA.SetBuffer(0,"OutputSize",_counter[0]);
            DDA.SetInt("ClassSize",_data.Classes);
            DDA.SetFloat("MinScore",threshold);
            
            DDA.SetTexture(0,"Input",_indentity[0]);
            DDA.SetInt("InputSize",_data.Identity0Width);
            DDA.SetFloats("anchors_x",Identity1Anchors_x);
            DDA.SetFloats("anchors_y", Identity1Anchors_y);
            DDA.DispatchThreads(0,_data.Identity0Width,_data.Identity0Width,1);

            DDA.SetTexture(0,"Input",_indentity[1]);
            DDA.SetInt("InputSize",_data.Identity1Width);
            DDA.SetFloats("anchors_x",Identity2Anchors_x);
            DDA.SetFloats("anchors_y",Identity2Anchors_y);
            DDA.DispatchThreads(0,_data.Identity1Width,_data.Identity1Width,1);

            ComputeShader RemoveOverlapping = shaders[2];
            RemoveOverlapping.SetFloat("limit",threshold);
            RemoveOverlapping.SetBuffer(0,"Input",shaders_output[1]);
            RemoveOverlapping.SetBuffer(0,"InputCount",_counter[0]);
            RemoveOverlapping.SetBuffer(0,"Output",shaders_output[2]);
            RemoveOverlapping.Dispatch(0,1,1,1);
            
            ComputeBuffer.CopyCount(shaders_output[2],_counter[1],0);
            
            MainOutput.getData();

        }
        /// <summary>
        /// cleans all cache and buffers to make sure there is no data leaks
        /// </summary>
        public void Destroy()
        {
            _worker.Dispose();
            _worker = null;

            shaders_output[0]?.Dispose();
            shaders_output[0] = null;
            shaders_output[1]?.Dispose();
            shaders_output[1] = null;
            shaders_output[2]?.Dispose();
            shaders_output[2] = null;
            _counter[0]?.Dispose();
            _counter[0] = null;
            _counter[1]?.Dispose();
            _counter[1] = null;
            
            _indentity[0].DiscardContents();
            _indentity[1].DiscardContents();
            
            MainOutput.Destroy();

        }

    }
}

/// <summary>
/// class that exist to take the cache from buffer 
/// </summary>
public class CachedData
{
    private ComputeBuffer data;
    private ComputeBuffer dataAmount;
    int[] args = new int[1];
    public DetectedObject[] _cached;
    
    public DetectedObject[] getData()
    {
        DateTime before = DateTime.Now;

        dataAmount.GetData(args, 0, 0, 1);
        
        DateTime after = DateTime.Now; 
        TimeSpan duration = after.Subtract(before);
        Debug.LogWarning("Duration before the big one in milliseconds: " + duration.Milliseconds);
        
        var count = args[0]; 
        _cached = new DetectedObject[count];
        data.GetData(_cached, 0, 0, count);
        
        return _cached;
    }

    public void Destroy()
    {
        data.Dispose();
        data = null;
        dataAmount.Dispose();
        data = null;
        _cached = null;
    }

    public DetectedObject[] Objects => getData();

    public CachedData(ComputeBuffer data1, ComputeBuffer dataAmount1) => (data, dataAmount) = (data1, dataAmount1);

}

/// <summary>
/// set of functions required to properly run code
/// </summary>
static class ExtensionFunctions
{

    public static void DispatchThreads
        (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

        x = (x + (int) xc - 1) / (int) xc;
        y = (y + (int) yc - 1) / (int) yc;
        z = (z + (int) zc - 1) / (int) zc;

        compute.Dispatch(kernel, x, y, z);
    }
    
    public static void CopyOutput
        (this IWorker worker, string tensorName, RenderTexture rt)
    {
        var output = worker.PeekOutput(tensorName);
        var shape = new TensorShape(1, rt.height, rt.width, 1);
        using var tensor = output.Reshape(shape);
        tensor.ToRenderTexture(rt);
    }
    
    public static Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RFloat, false);
        dest.Apply(false);
        Graphics.CopyTexture(rTex, dest);
        return dest;
    }
    
    public static float[] MakeAnchorArray(float[] anchors, int i1, int i2, int i3, float scale)
    {
        return new float[]
        {
            anchors[i1] * scale,0,0,0, anchors[i2 ] * scale,0,0,0, anchors[i3 ] *scale
        };
    }
}




