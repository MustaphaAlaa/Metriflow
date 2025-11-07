namespace Metriflow.Correlation.Worker.Interfaces;

public interface ICombiner
{
    Task GA_PSI_Combiner(GARecord ga, PSIRecord psi);
}