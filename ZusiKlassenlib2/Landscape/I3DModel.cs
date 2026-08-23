using System.Windows.Media.Media3D;

namespace ZusiKlassenLib2.Landscape
{
    interface I3DModel
    {
        Model3D CreateModel(float distance, params AnimationInfo[] infos);
    }
}
