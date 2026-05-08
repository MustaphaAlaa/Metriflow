using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_StageGARecords_Procedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
var stagedGAProcedure = """
                        CREATE PROCEDURE StageGARecords
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                        
                            BEGIN TRY
                                BEGIN TRANSACTION;
                        
                                WITH Batch AS (
                                    SELECT TOP (200000)
                                        Date,
                                        PageId,
                                        Users,
                                        Views,
                                        Sessions, 
                                        IsCorrelation
                                    FROM dbo.GARecords WITH (UPDLOCK, READPAST)
                                    WHERE IsCorrelation = 0
                                    ORDER BY Date
                                )
                                UPDATE Batch
                                SET IsCorrelation = 1
                                OUTPUT
                                     inserted.Date ,
                                    inserted.PageId,
                                    CASE
                                        WHEN DATEPART(HOUR, inserted.Date) < 4 THEN 1
                                        WHEN DATEPART(HOUR, inserted.Date) < 8 THEN 2
                                        WHEN DATEPART(HOUR, inserted.Date) < 12 THEN 3
                                        WHEN DATEPART(HOUR, inserted.Date) < 16 THEN 4
                                        WHEN DATEPART(HOUR, inserted.Date) < 20 THEN 5
                                        ELSE 6
                                    END,
                                    inserted.Users,
                                    inserted.Views,
                                    inserted.Sessions, 
                                    inserted.IsCorrelation
                                INTO dbo.GARecords_staged
                                    (Date, PageId, Interval, Users, Views, Sessions,  IsCorrelation);
                        
                                COMMIT;
                            END TRY
                            BEGIN CATCH
                                ROLLBACK;
                                THROW;
                            END CATCH
                        END;
                        """;
migrationBuilder.Sql(stagedGAProcedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql("DROP PROCEDURE StageGARecords");
        }
    }
}
