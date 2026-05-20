namespace IRepository.Generic;

public interface IRawDataStagingRepository
{
    Task ExecuteStagedProceduresAsync();
}
