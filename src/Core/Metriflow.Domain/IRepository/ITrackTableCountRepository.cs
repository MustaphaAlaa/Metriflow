namespace IRepository;

public interface ITrackTableCountRepository
{
    Task<int> AlterTableRowsCountAsync(string tableName, int rowsCount);
}