namespace Metriflow.Correlation.Worker.Interfaces;

public interface ICombiner
{
    // Task GA_PSI_Combiner(GARecord ga, PSIRecord psi);
    Task GA_PSI_Combiner(List<Tuple<GARecord, PSIRecord>>? GA_PSI_LIST);
}
