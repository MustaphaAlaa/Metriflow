using System.Data;
using System.Diagnostics.CodeAnalysis;
using Metriflow.Domain.Entities.Workers;

namespace Repositories.Ado;

public class GARecordDataReader(List<List<GARecord>> gARecords) : IDataReader


{

    private readonly IEnumerator<IEnumerable<GARecord>> _outer = gARecords.GetEnumerator();
    private IEnumerator<GARecord>? _inner;
    private GARecord _current ;

    public bool NextResult() => false;

    public bool Read()
    {
        while (true)
        {
            if (_inner != null && _inner.MoveNext())
            {
                _current = _inner.Current;
                return true;
            }

            if (!_outer.MoveNext())
                return false;

            _inner?.Dispose();
            _inner = _outer.Current.GetEnumerator();
            _current = _inner?.Current;
        }
    }



    public object GetValue(int i)
    {

        return i switch
        {
            0 => new DateTime(_current.Ticks, DateTimeKind.Utc),
            1 => _current.PageId,
            2 => _current.Users,
            3 => _current.Views,
            4 => _current.Sessions,
            5 => _current.IsCorrelation,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public int FieldCount => 6;

    public string GetName(int i) => i switch
    {
        0 => "Date",
        1 => "PageId",
        2 => "Users",
        3 => "Views",
        4 => "Sessions",
        5 => "IsCorrelation",
        _ => throw new IndexOutOfRangeException()
    };

    public void Dispose()
    {
        _inner?.Dispose();
        _outer?.Dispose();
    }
    
    //reset of methods


public object this[int i] => throw new NotImplementedException();

    public object this[string name] => throw new NotImplementedException();

    public int Depth => throw new NotImplementedException();

    public bool IsClosed => throw new NotImplementedException();

    public int RecordsAffected => throw new NotImplementedException();


    public void Close()
    {
        throw new NotImplementedException();
    }



    public bool GetBoolean(int i)
    {
        throw new NotImplementedException();
    }

    public byte GetByte(int i)
    {
        throw new NotImplementedException();
    }

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public char GetChar(int i)
    {
        throw new NotImplementedException();
    }

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public IDataReader GetData(int i)
    {
        throw new NotImplementedException();
    }

    public string GetDataTypeName(int i)
    {
        throw new NotImplementedException();
    }

    public DateTime GetDateTime(int i)
    {
        throw new NotImplementedException();
    }

    public decimal GetDecimal(int i)
    {
        throw new NotImplementedException();
    }

    public double GetDouble(int i)
    {
        throw new NotImplementedException();
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type GetFieldType(int i)
    {
        throw new NotImplementedException();
    }

    public float GetFloat(int i)
    {
        throw new NotImplementedException();
    }

    public Guid GetGuid(int i)
    {
        throw new NotImplementedException();
    }

    public short GetInt16(int i)
    {
        throw new NotImplementedException();
    }

    public int GetInt32(int i)
    {
        throw new NotImplementedException();
    }

    public long GetInt64(int i)
    {
        throw new NotImplementedException();
    }



    public int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public DataTable? GetSchemaTable()
    {
        throw new NotImplementedException();
    }

    public string GetString(int i)
    {
        throw new NotImplementedException();
    }



    public int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public bool IsDBNull(int i)
    {
        throw new NotImplementedException();
    }


}