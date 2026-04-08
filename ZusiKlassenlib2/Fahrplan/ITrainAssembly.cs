using System.Collections.Generic;

namespace ZusiKlassenLib2.Fahrplan
{
    public interface ITrainAssembly
    {
        void BuildTrain(LinkedList<FahrzeugInfo> zugReihung);
    }
}
