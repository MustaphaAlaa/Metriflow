public class GARecord : IAnalyticRecord
{
    public long Date { get; set; }
    public byte Page { get; set; }
    public long Users { get; set; }
    public long Views { get; set; }
    public long Sessions { get; set; }

    public override string ToString()
    {
        return $"Date: {this.Date}, Page: {this.Page}, Users: {this.Users},Views: {this.Views}, Sessions: {this.Sessions}";
    }
}
