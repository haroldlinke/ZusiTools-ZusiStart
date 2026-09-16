using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZusiStart.KI;

namespace ZusiStart.KI
{





  public class AnomalyAutoencoderService
  {
    private readonly InferenceSession _session;

    public AnomalyAutoencoderService(string modelPath)
    {
      _session = new InferenceSession(modelPath);
    }

    public float[] ComputeReconstructionError(IReadOnlyList<ZugNumericFeatures> zuege)
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

      var inputs = new[]
      {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

      using var results = _session.Run(inputs);
      var reconTensor = results.First(r => r.Name == "recon").AsTensor<float>();

      var errors = new float[n];

      for (int i = 0; i < n; i++)
      {
        float sumSq = 0f;
        for (int j = 0; j < 5; j++)
        {
          float orig = inputTensor[i, j];
          float rec = reconTensor[i, j];
          float diff = orig - rec;
          sumSq += diff * diff;
        }
        errors[i] = sumSq / 5f; // MSE pro Zug
      }

      return errors;
    }
  }
}

