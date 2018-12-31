namespace LibgenServer.Core.Entities
{
    public abstract class LibgenObject
    {
        protected LibgenObject(LibgenObjectType libgenObjectType)
        {
            LibgenObjectType = libgenObjectType;
        }

        public int Id { get; set; }
        public int LibgenId { get; set; }
        public int? FileId { get; set; }
        public LibgenObjectType LibgenObjectType { get; }
    }
}
