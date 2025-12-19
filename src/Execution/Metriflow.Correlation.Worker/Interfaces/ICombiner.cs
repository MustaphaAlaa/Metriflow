namespace Metriflow.Correlation.Worker.Interfaces;

public interface ICombiner
{
    // Task GA_PSI_Combiner(GARecord ga, PSIRecord psi);
    Task GA_PSI_Combiner(List<recordGA_PSI>? GA_PSI_LIST);
}
