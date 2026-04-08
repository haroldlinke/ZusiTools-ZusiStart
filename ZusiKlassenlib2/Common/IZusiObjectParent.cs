namespace ZusiKlassenLib2.Common
{
    public interface IZusiObjectParent
    {
        IZusiObjectParent Parent { get; }

        T FindParent<T>() where T : IZusiObjectParent;
    }
}
