namespace LibgenServer.Core.Import.SqlDump
{
    public class ColumnDefinition
    {
        public ColumnDefinition(string columnName, ColumnType columnType)
        {
            ColumnName = columnName.ToLower();
            ColumnType = columnType;
        }

        public string ColumnName { get; }
        public ColumnType ColumnType { get; }
    }
}
