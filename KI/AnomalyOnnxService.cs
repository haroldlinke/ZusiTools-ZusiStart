using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZusiStart.KI
{
  public class ZugNumericFeatures
  {
    public ulong ZugId { get; set; }

    public float Dauer { get; set; }
    public float StartDay { get; set; }
    public float EndDay { get; set; }
    public float AnzahlUnterwegsbahnhoefe { get; set; }
    public float AnzahlFahrzeuge { get; set; }
  }



  public class AnomalyOnnxService
  {
    private readonly InferenceSession _session;

    public AnomalyOnnxService(string modelPath)
    {
      _session = new InferenceSession(modelPath);
      Console.WriteLine("=== ONNX INPUTS ===");
      foreach (var inp in _session.InputMetadata)
        Console.WriteLine($"Input: {inp.Key}  Type: {inp.Value.ElementType}  Dim: {string.Join(",", inp.Value.Dimensions)}");

      Console.WriteLine("=== ONNX OUTPUTS ===");
      foreach (var outp in _session.OutputMetadata)
        Console.WriteLine($"Output: {outp.Key}  Type: {outp.Value.ElementType}  Dim: {string.Join(",", outp.Value.Dimensions)}");
    }

    public float[] Predict(IReadOnlyList<ZugNumericFeatures> zuege)
    {
      int n = zuege.Count;

      var inputTensor = new DenseTensor<float>(new[] { n, 5 });

      for (int i = 0; i < n; i++)
      {
        var z = zuege[i];
        inputTensor[i, 0] = z.Dauer;
        inputTensor[i, 1] = z.StartDay;
        inputTensor[i, 2] = z.EndDay;
        inputTensor[i, 3] = z.AnzahlUnterwegsbahnhoefe;
        inputTensor[i, 4] = 0; // z.AnzahlFahrzeuge;
      }

      string inputName = _session.InputMetadata.Keys.First();

      var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
    };

      using var results = _session.Run(inputs);

      string outputName = _session.OutputMetadata.Keys.First();
      var outputValue = results.First(r => r.Name == outputName);

      // Der Output ist ein DenseTensor<long>
      var tensorLong = outputValue.AsTensor<long>();

      if (tensorLong == null)
        throw new Exception("Output is not a long tensor.");

      // Das Modell gibt einen 2D-Tensor [N,1] zurück
      int n1 = tensorLong.Dimensions[0];
      var values = new float[n1];

      for (int i = 0; i < n1; i++)
      {
        values[i] = (float)tensorLong[i, 0];   // -1 oder +1
      }

      return values;



    }




  }
}

